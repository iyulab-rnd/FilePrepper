namespace FilePrepper.Tasks.FillMissingValues;

public enum FillMethod
{
    FixedValue,      // 고정값으로 대체
    Mean,            // 평균값으로 대체
    Median,          // 중앙값으로 대체
    Mode,            // 최빈값으로 대체
    ForwardFill,     // 앞의 값으로 대체
    BackwardFill,    // 뒤의 값으로 대체
    LinearInterpolation  // 선형 보간
}

public class ColumnFillMethod
{
    public string ColumnName { get; set; } = string.Empty;
    public FillMethod Method { get; set; }
    public string? FixedValue { get; set; }
}

public class FillMissingValuesOption : BaseColumnOption
{
    public List<ColumnFillMethod> FillMethods { get; set; } = new();

    public override string[] Validate()
    {
        // 1) 부모 검증 호출 전, FillMethods의 컬럼들을 TargetColumns에 미리 반영
        foreach (var method in FillMethods)
        {
            if (!TargetColumns.Contains(method.ColumnName))
            {
                TargetColumns = TargetColumns.Concat(new[] { method.ColumnName }).ToArray();
            }
        }

        // 2) 이제 부모 검증
        return base.Validate();
    }

    protected override string[] ValidateInternal()
    {
        var errors = new List<string>();

        // FillMethods가 하나도 없으면 오류
        if (FillMethods.Count == 0)
        {
            errors.Add("At least one fill method must be specified");
            return [.. errors];
        }

        // 각 FillMethod마다 검증
        foreach (var method in FillMethods)
        {
            if (string.IsNullOrWhiteSpace(method.ColumnName))
            {
                errors.Add("Column name cannot be empty or whitespace");
            }

            if (method.Method == FillMethod.FixedValue && string.IsNullOrEmpty(method.FixedValue))
            {
                errors.Add($"Fixed value must be specified for column {method.ColumnName}");
            }

            // (예전 코드에서 TargetColumns에 컬럼을 추가하던 부분은 Validate()로 옮겼음)
        }

        return [.. errors];
    }
}