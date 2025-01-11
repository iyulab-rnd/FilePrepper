namespace FilePrepper.Tasks.BasicStatistics;

public class BasicStatisticsValidator : BaseValidator<BasicStatisticsOption>
{
    public BasicStatisticsValidator(ILogger<BasicStatisticsValidator> logger)
        : base(logger)
    {
    }

    protected override string[] ValidateSpecific(BasicStatisticsOption option)
    {
        var errors = new List<string>();

        // 컬럼 검증
        if (option.TargetColumns == null || option.TargetColumns.Length == 0)
        {
            errors.Add("At least one target column must be specified");
        }
        else if (option.TargetColumns.Any(string.IsNullOrWhiteSpace))
        {
            errors.Add("Target column names cannot be empty or whitespace");
        }

        // 통계 유형 검증
        if (option.Statistics == null || option.Statistics.Length == 0)
        {
            errors.Add("At least one statistic type must be specified");
        }

        // Suffix 검증
        if (string.IsNullOrWhiteSpace(option.Suffix))
        {
            errors.Add("Suffix cannot be empty or whitespace");
        }

        return [.. errors];
    }
}