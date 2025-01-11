namespace FilePrepper.Tasks.BasicStatistics;

public enum StatisticType
{
    Mean,
    StandardDeviation,
    Min,
    Max,
    Median,
    Q1,
    Q3,
    ZScore,        // (x - mean) / std
    RobustZScore,  // (x - median) / (mad * 1.4826)
    PercentRank,   // 백분위 순위
    MAD            // Median Absolute Deviation
}


public class BasicStatisticsOption : BaseOption, IColumnOption
{
    public string[] TargetColumns { get; set; } = Array.Empty<string>();
    public StatisticType[] Statistics { get; set; } = Array.Empty<StatisticType>();
    public string Suffix { get; set; } = "_stat";

    protected override string[] ValidateInternal()
    {
        var errors = new List<string>();

        if (Statistics == null || Statistics.Length == 0)
        {
            errors.Add("At least one statistic type must be specified");
        }

        if (string.IsNullOrWhiteSpace(Suffix))
        {
            errors.Add("Suffix cannot be empty or whitespace");
        }

        return errors.ToArray();
    }
}