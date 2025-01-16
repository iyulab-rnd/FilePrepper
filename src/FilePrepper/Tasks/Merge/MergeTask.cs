using CsvHelper;

namespace FilePrepper.Tasks.Merge;

public class MergeTask : BaseTask<MergeOption>
{
    private HashSet<string> _allHeaders = new();

    public MergeTask(MergeOption options, ILogger<MergeTask> logger) : base(options, logger)
    {
    }

    protected override async Task<List<Dictionary<string, string>>> ProcessRecordsAsync(
        List<Dictionary<string, string>> _ignoredBecauseMergeOptionUsesInputPathsDirectly)
    {
        try
        {
            List<Dictionary<string, string>> allRecords;

            if (Options.MergeType == MergeType.Vertical)
            {
                allRecords = await MergeVerticalAsync();
            }
            else
            {
                allRecords = await MergeHorizontalAsync();
            }

            if (allRecords == null || !allRecords.Any())
            {
                throw new ValidationException("No records were produced during merge operation.", ValidationExceptionErrorCode.General);
            }

            return allRecords;
        }
        catch (Exception ex) when (ex is not ValidationException)
        {
            throw new ValidationException(ex.Message, ValidationExceptionErrorCode.General);
        }
    }

    private async Task<List<Dictionary<string, string>>> MergeVerticalAsync()
    {
        var allRecords = new List<Dictionary<string, string>>();
        _allHeaders = new HashSet<string>();
        int? expectedColumnCount = null;

        foreach (var path in Options.InputPaths)
        {
            try
            {
                var (newRecords, headers) = await ReadCsvFileAsync(path);

                // 세로 병합 시에는 모든 파일의 열 개수가 동일해야 함
                if (expectedColumnCount == null)
                {
                    expectedColumnCount = headers.Count;
                }
                else if (headers.Count != expectedColumnCount)
                {
                    throw new ValidationException(
                        $"Column count mismatch in {path}. Expected: {expectedColumnCount}, Actual: {headers.Count}",
                        ValidationExceptionErrorCode.General);
                }

                headers.ForEach(h => _allHeaders.Add(h));
                allRecords.AddRange(newRecords);
            }
            catch (Exception ex) when (ex is not ValidationException)
            {
                throw new ValidationException(ex.Message, ValidationExceptionErrorCode.General);
            }
        }

        return allRecords;
    }

    private async Task<List<Dictionary<string, string>>> MergeHorizontalAsync()
    {
        // 첫 번째 파일 읽기
        var (records, headers) = await ReadCsvFileAsync(Options.InputPaths[0]);
        var mergedRecords = records;
        _allHeaders = new HashSet<string>(headers);

        // Join Key가 있는 경우는 JoinType에 따라 Join 수행
        if (Options.JoinKeyColumns?.Count > 0)
        {
            ValidateJoinKeyColumns(headers);

            for (int i = 1; i < Options.InputPaths.Count; i++)
            {
                var (rightRecords, rightHeaders) = await ReadCsvFileAsync(Options.InputPaths[i]);
                ValidateJoinKeyColumns(rightHeaders);

                mergedRecords = JoinTwoSets(mergedRecords, rightRecords);
            }
            return mergedRecords;
        }

        // Join Key가 없는 경우는 단순히 열 추가 (행 개수가 같아야 함)
        for (int i = 1; i < Options.InputPaths.Count; i++)
        {
            var (rightRecords, rightHeaders) = await ReadCsvFileAsync(Options.InputPaths[i]);

            // 행 개수가 같은지 검증
            if (rightRecords.Count != mergedRecords.Count)
            {
                throw new ValidationException(
                    $"Row count mismatch in {Options.InputPaths[i]}. " +
                    $"Expected: {mergedRecords.Count}, Actual: {rightRecords.Count}",
                    ValidationExceptionErrorCode.General);
            }

            // 중복 컬럼 이름 처리
            var headerMapping = new Dictionary<string, string>();
            foreach (var header in rightHeaders)
            {
                var finalHeader = _allHeaders.Contains(header) ? GetUniqueHeader(header) : header;
                headerMapping[header] = finalHeader;
                _allHeaders.Add(finalHeader);
            }

            // 데이터 병합
            for (int j = 0; j < mergedRecords.Count; j++)
            {
                foreach (var kvp in headerMapping)
                {
                    var originalHeader = kvp.Key;
                    var newHeader = kvp.Value;
                    mergedRecords[j][newHeader] = rightRecords[j][originalHeader];
                }
            }
        }

        return mergedRecords;
    }

    private List<Dictionary<string, string>> JoinTwoSets(
        List<Dictionary<string, string>> leftRecords,
        List<Dictionary<string, string>> rightRecords)
    {
        var result = new List<Dictionary<string, string>>();

        // 조인 키 값을 문자열로 조합하여 비교
        string GetKeyValue(Dictionary<string, string> record)
        {
            return string.Join("|", Options.JoinKeyColumns.Select(keyCol =>
            {
                if (keyCol.Name != null)
                {
                    return record.GetValueOrDefault(keyCol.Name, string.Empty);
                }
                else if (keyCol.Index.HasValue)
                {
                    var header = _allHeaders.ElementAt(keyCol.Index.Value);
                    return record.GetValueOrDefault(header, string.Empty);
                }
                return string.Empty;
            }));
        }

        // 오른쪽 레코드를 키로 인덱싱
        var rightDict = rightRecords.GroupBy(GetKeyValue)
            .ToDictionary(g => g.Key, g => g.ToList());

        // 왼쪽 레코드를 기준으로 오른쪽과 조인
        foreach (var leftRecord in leftRecords)
        {
            var leftKey = GetKeyValue(leftRecord);

            if (rightDict.TryGetValue(leftKey, out var matchingRightRecords))
            {
                // 매칭되는 레코드가 있는 경우
                foreach (var rightRecord in matchingRightRecords)
                {
                    var joinedRecord = new Dictionary<string, string>(leftRecord);

                    // Join 키가 아닌 컬럼만 추가
                    foreach (var kvp in rightRecord)
                    {
                        var isJoinKey = Options.JoinKeyColumns.Any(keyCol =>
                            (keyCol.Name != null && keyCol.Name == kvp.Key) ||
                            (keyCol.Index.HasValue && _allHeaders.ElementAt(keyCol.Index.Value) == kvp.Key));

                        if (!isJoinKey)
                        {
                            joinedRecord[kvp.Key] = kvp.Value;
                        }
                    }
                    result.Add(joinedRecord);
                }
            }
            else if (Options.JoinType == JoinType.Left || Options.JoinType == JoinType.Full)
            {
                // LEFT/FULL OUTER JOIN에서 매칭되는 레코드가 없는 경우
                var joinedRecord = new Dictionary<string, string>(leftRecord);
                foreach (var header in rightRecords.FirstOrDefault()?.Keys ?? Enumerable.Empty<string>())
                {
                    var isJoinKey = Options.JoinKeyColumns.Any(keyCol =>
                        (keyCol.Name != null && keyCol.Name == header) ||
                        (keyCol.Index.HasValue && _allHeaders.ElementAt(keyCol.Index.Value) == header));

                    if (!isJoinKey)
                    {
                        joinedRecord[header] = string.Empty;
                    }
                }
                result.Add(joinedRecord);
            }
        }

        // RIGHT/FULL OUTER JOIN의 경우 왼쪽에 매칭되지 않은 오른쪽 레코드도 추가
        if (Options.JoinType == JoinType.Right || Options.JoinType == JoinType.Full)
        {
            var processedKeys = result.Select(GetKeyValue).ToHashSet();

            foreach (var rightRecord in rightRecords)
            {
                var rightKey = GetKeyValue(rightRecord);
                if (!processedKeys.Contains(rightKey))
                {
                    var joinedRecord = new Dictionary<string, string>();

                    // 왼쪽 레코드의 모든 컬럼을 빈 값으로 설정
                    foreach (var header in leftRecords.FirstOrDefault()?.Keys ?? Enumerable.Empty<string>())
                    {
                        joinedRecord[header] = string.Empty;
                    }

                    // 오른쪽 레코드의 값을 추가
                    foreach (var kvp in rightRecord)
                    {
                        joinedRecord[kvp.Key] = kvp.Value;
                    }
                    result.Add(joinedRecord);
                }
            }
        }

        return result;
    }

    private void ValidateJoinKeyColumns(List<string> headers)
    {
        foreach (var keyCol in Options.JoinKeyColumns)
        {
            if (keyCol.Name != null)
            {
                if (!headers.Contains(keyCol.Name))
                {
                    throw new ValidationException(
                        $"Join key column '{keyCol.Name}' not found in headers: {string.Join(", ", headers)}",
                        ValidationExceptionErrorCode.General);
                }
            }
            else if (keyCol.Index.HasValue)
            {
                if (keyCol.Index.Value >= headers.Count)
                {
                    throw new ValidationException(
                        $"Join key column index {keyCol.Index.Value} is out of range. Header count: {headers.Count}",
                        ValidationExceptionErrorCode.General);
                }
            }
        }
    }

    private string GetUniqueHeader(string baseHeader)
    {
        int suffix = 2;
        string newHeader = $"{baseHeader}_{suffix}";
        while (_allHeaders.Contains(newHeader))
        {
            suffix++;
            newHeader = $"{baseHeader}_{suffix}";
        }
        return newHeader;
    }
}