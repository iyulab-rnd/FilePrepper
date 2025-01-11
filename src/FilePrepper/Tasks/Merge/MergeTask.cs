namespace FilePrepper.Tasks.Merge;

public class MergeTask : BaseTask<MergeOption>
{
    public MergeTask(
        MergeOption options,
        ILogger<MergeTask> logger,
        ILogger<MergeValidator> validatorLogger)
        : base(options, logger, new MergeValidator(validatorLogger))
    {
    }

    protected override async Task<List<Dictionary<string, string>>> ProcessRecordsAsync(
        List<Dictionary<string, string>> _ignoredBecauseMergeOptionUsesInputPathsDirectly)
    {
        // BaseTask의 context.InputPath는 무시하고,
        // MergeOption.InputPaths를 사용해 Merge 수행

        var allRecords = new List<Dictionary<string, string>>();

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
        var allHeaders = new HashSet<string>();

        // 모든 파일에 대해 CSV를 읽어 합치기
        foreach (var path in Options.InputPaths)
        {
            var newRecords = await ReadCsvFileAsync(path);
            allRecords.AddRange(newRecords);

            // 헤더(컬럼) 정보 추출
            if (newRecords.Count > 0)
            {
                foreach (var header in newRecords[0].Keys)
                {
                    allHeaders.Add(header);
                }
            }
        }

        // 컬럼 수가 다른 CSV를 머지할 때, 부족한 컬럼은 빈 문자열로 채움
        foreach (var record in allRecords)
        {
            foreach (var header in allHeaders)
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
        var mergedRecords = await ReadCsvFileAsync(Options.InputPaths[0]);

        // 2) 두 번째 파일부터 차례로 Join
        for (int i = 1; i < Options.InputPaths.Count; i++)
        {
            var nextRecords = await ReadCsvFileAsync(Options.InputPaths[i]);
            mergedRecords = JoinTwoSets(mergedRecords, nextRecords);
        }

        return mergedRecords;
    }

    private List<Dictionary<string, string>> JoinTwoSets(
        List<Dictionary<string, string>> leftRecords,
        List<Dictionary<string, string>> rightRecords)
    {
        // Join에 등장하는 모든 컬럼(헤더)을 수집
        var allHeaders = new HashSet<string>();
        foreach (var rec in leftRecords)
            foreach (var col in rec.Keys) allHeaders.Add(col);
        foreach (var rec in rightRecords)
            foreach (var col in rec.Keys) allHeaders.Add(col);

        // 모든 레코드가 allHeaders를 갖도록 보정
        // (없는 컬럼은 빈 문자열로 채움)
        foreach (var rec in leftRecords)
        {
            foreach (var header in allHeaders)
            {
                if (!rec.ContainsKey(header))
                {
                    rec[header] = string.Empty;
                }
            }
        }
        foreach (var rec in rightRecords)
        {
            foreach (var header in allHeaders)
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

    private Dictionary<string, string> MergeRow(
        Dictionary<string, string> left,
        Dictionary<string, string> right)
    {
        // 왼쪽과 오른쪽을 합치되, 겹치는 컬럼은 오른쪽으로 overwrite할지?
        // 여기서는 단순히 오른쪽을 우선하는 식으로 구현
        var merged = new Dictionary<string, string>(left);
        foreach (var kv in right)
        {
            merged[kv.Key] = kv.Value;
        }
        return merged;
    }

    protected override IEnumerable<string> GetRequiredColumns()
    {
        // 이 Task에선 별도 최소 컬럼이 없으므로 비워둠
        return Enumerable.Empty<string>();
    }
}
