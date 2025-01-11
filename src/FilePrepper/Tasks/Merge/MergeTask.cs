namespace FilePrepper.Tasks.Merge;

public class MergeTask : BaseTask<MergeOption>
{
    // 모든 Merge 수행 후 최종 CSV를 출력할 때 사용할 헤더
    private HashSet<string> _allHeaders = new();

    public MergeTask(
        MergeOption options,
        ILogger<MergeTask> logger)
        : base(options, logger)
    {
    }

    protected override async Task<List<Dictionary<string, string>>> ProcessRecordsAsync(
        List<Dictionary<string, string>> _ignoredBecauseMergeOptionUsesInputPathsDirectly)
    {
        // BaseTask의 context.InputPath는 무시하고,
        // MergeOption.InputPaths를 사용해 Merge 수행

        List<Dictionary<string, string>> allRecords;

        if (Options.MergeType == MergeType.Vertical)
        {
            // 1) 세로(Union)로 머지
            allRecords = await MergeVerticalAsync();
        }
        else
        {
            // 2) 가로(Join)로 머지
            allRecords = await MergeHorizontalAsync();
        }

        return allRecords;
    }

    /// <summary>
    /// 여러 CSV 파일을 세로로 머지 (Union)한다.
    /// </summary>
    private async Task<List<Dictionary<string, string>>> MergeVerticalAsync()
    {
        var allRecords = new List<Dictionary<string, string>>();
        _allHeaders = [];

        // 모든 파일에 대해 CSV를 읽어 합치기
        foreach (var path in Options.InputPaths)
        {
            var (newRecords, _) = await ReadCsvFileAsync(path);  // 튜플 분해
            allRecords.AddRange(newRecords);

            if (newRecords.Count > 0)
            {
                foreach (var header in newRecords[0].Keys)
                {
                    _allHeaders.Add(header);
                }
            }
        }


        // 컬럼 수가 다른 CSV를 머지할 때, 부족한 컬럼은 빈 문자열로 채움
        foreach (var record in allRecords)
        {
            foreach (var header in _allHeaders)
            {
                if (!record.ContainsKey(header))
                {
                    record[header] = string.Empty;
                }
            }
        }
        return allRecords;
    }

    /// <summary>
    /// 여러 CSV 파일을 가로로 머지 (Join)한다.
    /// 2개 이상 파일도 순차적으로 머지.
    /// </summary>
    private async Task<List<Dictionary<string, string>>> MergeHorizontalAsync()
    {
        // [중요] 가로 머지는 순차적으로 2개씩 Join하는 식으로 처리
        // (파일이 N개면, 1,2번 -> 결과 -> 3번과 Join -> 결과 -> 4번과 Join ...)

        // 1) 일단 첫 번째 파일 읽기
        var (mergedRecords, _) = await ReadCsvFileAsync(Options.InputPaths[0]);

        // allHeaders 초기화
        _allHeaders = new HashSet<string>(mergedRecords.SelectMany(r => r.Keys));

        // 2) 두 번째 파일부터 차례로 Join
        for (int i = 1; i < Options.InputPaths.Count; i++)
        {
            var (nextRecords, _) = await ReadCsvFileAsync(Options.InputPaths[i]);
            mergedRecords = JoinTwoSets(mergedRecords, nextRecords);
        }

        return mergedRecords;
    }

    private List<Dictionary<string, string>> JoinTwoSets(
        List<Dictionary<string, string>> leftRecords,
        List<Dictionary<string, string>> rightRecords)
    {
        // Join에 등장하는 모든 컬럼(헤더)을 수집
        foreach (var rec in rightRecords)
        {
            foreach (var col in rec.Keys)
                _allHeaders.Add(col);
        }

        // 모든 레코드가 _allHeaders를 갖도록 보정
        // (없는 컬럼은 빈 문자열로 채움)
        foreach (var rec in leftRecords)
        {
            foreach (var header in _allHeaders)
            {
                if (!rec.ContainsKey(header))
                {
                    rec[header] = string.Empty;
                }
            }
        }
        foreach (var rec in rightRecords)
        {
            foreach (var header in _allHeaders)
            {
                if (!rec.ContainsKey(header))
                {
                    rec[header] = string.Empty;
                }
            }
        }

        var joinResult = new List<Dictionary<string, string>>();

        if (Options.JoinType == JoinType.Inner)
        {
            // INNER JOIN: 두 쪽 모두 키가 있는 레코드만
            foreach (var lRec in leftRecords)
            {
                foreach (var rRec in rightRecords)
                {
                    if (IsKeyMatch(lRec, rRec))
                    {
                        joinResult.Add(MergeRow(lRec, rRec));
                    }
                }
            }
        }
        else if (Options.JoinType == JoinType.Left)
        {
            // LEFT JOIN
            foreach (var lRec in leftRecords)
            {
                bool matchFound = false;
                foreach (var rRec in rightRecords)
                {
                    if (IsKeyMatch(lRec, rRec))
                    {
                        joinResult.Add(MergeRow(lRec, rRec));
                        matchFound = true;
                    }
                }
                // 오른쪽에 매칭이 없으면 왼쪽 정보만 들어간 레코드 추가
                if (!matchFound)
                {
                    joinResult.Add(new Dictionary<string, string>(lRec));
                }
            }
        }
        else if (Options.JoinType == JoinType.Right)
        {
            // RIGHT JOIN
            foreach (var rRec in rightRecords)
            {
                bool matchFound = false;
                foreach (var lRec in leftRecords)
                {
                    if (IsKeyMatch(lRec, rRec))
                    {
                        joinResult.Add(MergeRow(lRec, rRec));
                        matchFound = true;
                    }
                }
                // 왼쪽에 매칭이 없으면 오른쪽 정보만 들어간 레코드 추가
                if (!matchFound)
                {
                    joinResult.Add(new Dictionary<string, string>(rRec));
                }
            }
        }
        else
        {
            // FULL OUTER JOIN
            // 1) Inner join 결과
            var inner = new List<Dictionary<string, string>>();
            var usedInLeft = new HashSet<int>();
            var usedInRight = new HashSet<int>();

            for (int li = 0; li < leftRecords.Count; li++)
            {
                for (int ri = 0; ri < rightRecords.Count; ri++)
                {
                    if (IsKeyMatch(leftRecords[li], rightRecords[ri]))
                    {
                        var merged = MergeRow(leftRecords[li], rightRecords[ri]);
                        inner.Add(merged);
                        usedInLeft.Add(li);
                        usedInRight.Add(ri);
                    }
                }
            }

            // 2) 매칭이 안 된 왼쪽 레코드
            var leftOnly = leftRecords
                .Select((rec, idx) => (rec, idx))
                .Where(t => !usedInLeft.Contains(t.idx))
                .Select(t => t.rec)
                .ToList();

            // 3) 매칭이 안 된 오른쪽 레코드
            var rightOnly = rightRecords
                .Select((rec, idx) => (rec, idx))
                .Where(t => !usedInRight.Contains(t.idx))
                .Select(t => t.rec)
                .ToList();

            joinResult = inner
                .Concat(leftOnly)
                .Concat(rightOnly)
                .ToList();
        }

        // [추가] JOIN 결과를 JoinKeyColumns[0] 기준으로 정렬 (테스트가 특정 순서를 기대할 경우)
        if (Options.JoinKeyColumns.Count > 0)
        {
            var firstKey = Options.JoinKeyColumns[0];
            joinResult = joinResult
                .OrderBy(r => r[firstKey])  // 단순 문자열 정렬
                .ToList();
        }

        return joinResult;
    }

    private bool IsKeyMatch(Dictionary<string, string> left, Dictionary<string, string> right)
    {
        // JoinKeyColumns 전부 일치하면 true
        foreach (var keyCol in Options.JoinKeyColumns)
        {
            // 키값이 모두 같은지 비교 (단순 문자열 비교)
            if (left[keyCol] != right[keyCol])
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 왼쪽과 오른쪽 Dictionary를 합치되, 오른쪽 값이 빈 문자열이면 덮어쓰지 않고, 실제 값이 있으면 우선 적용
    /// </summary>
    private Dictionary<string, string> MergeRow(
        Dictionary<string, string> left,
        Dictionary<string, string> right)
    {
        // 기본적으로 왼쪽 복사
        var merged = new Dictionary<string, string>(left);

        // 오른쪽 값이 비어있지 않을 때만 덮어쓰기
        foreach (var kv in right)
        {
            // 오른쪽 Value가 비어있지 않으면 overwrite
            if (!string.IsNullOrEmpty(kv.Value))
            {
                merged[kv.Key] = kv.Value;
            }
            else if (!merged.ContainsKey(kv.Key))
            {
                // 혹시 왼쪽에도 없었던 컬럼이면 추가
                merged[kv.Key] = string.Empty;
            }
        }

        return merged;
    }

    protected override IEnumerable<string> GetRequiredColumns()
    {
        // 이 Task에선 별도 최소 컬럼이 없으므로 비워둠
        return Enumerable.Empty<string>();
    }

    /// <summary>
    /// 레코드가 없어도 헤더를 출력하기 위해 WriteOutputAsync를 오버라이드
    /// </summary>
    protected override async Task WriteOutputAsync(
        string outputPath,
        IEnumerable<string> headers,
        IEnumerable<Dictionary<string, string>> records)
    {
        // 레코드가 하나도 없어도, _allHeaders를 써서 헤더를 출력
        if (!headers.Any() && _allHeaders.Any())
        {
            headers = _allHeaders;
        }

        // 기본 베이스 로직 호출
        await base.WriteOutputAsync(outputPath, headers, records);
    }
}
