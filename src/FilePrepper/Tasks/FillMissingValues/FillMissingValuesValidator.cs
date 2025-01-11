namespace FilePrepper.Tasks.FillMissingValues;

public class FillMissingValuesValidator : BaseValidator<FillMissingValuesOption>
{
    public FillMissingValuesValidator(ILogger<FillMissingValuesValidator> logger)
        : base(logger)
    {
    }

    protected override string[] ValidateSpecific(FillMissingValuesOption option)
    {
        var errors = new List<string>();

        // FillMethods 검증
        if (option.FillMethods == null || !option.FillMethods.Any())
        {
            errors.Add("At least one fill method must be specified");
            return [.. errors];
        }

        // 각 FillMethod 검증
        foreach (var method in option.FillMethods)
        {
            // 컬럼명 검증
            if (string.IsNullOrWhiteSpace(method.ColumnName))
            {
                errors.Add("Column name cannot be empty or whitespace");
            }

            // FixedValue 검증
            if (method.Method == FillMethod.FixedValue && string.IsNullOrEmpty(method.FixedValue))
            {
                errors.Add($"Fixed value must be specified for column {method.ColumnName}");
            }

            // FillMethod에 지정된 모든 컬럼이 TargetColumns에 포함되어 있는지 확인
            if (!option.TargetColumns.Contains(method.ColumnName))
            {
                errors.Add($"Column '{method.ColumnName}' specified in FillMethods must be included in TargetColumns");
            }
        }

        return [.. errors];
    }
}