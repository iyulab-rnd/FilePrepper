using CommandLine;
using FilePrepper.Tasks.BasicStatistics;
using Microsoft.Extensions.Logging;


namespace FilePrepper.CLI.Tools.BasicStatistics;

/// <summary>
/// stats 명령어의 매개변수를 정의하는 클래스
/// </summary>
[Verb("stats", HelpText = "Calculate basic statistics on numeric columns")]
public class BasicStatisticsParameters : BaseColumnParameters, IDefaultValueParameters
{
    [Option('s', "stats", Required = true, Separator = ',',
        HelpText = "Statistics to calculate (Mean/StandardDeviation/Min/Max/Median/Q1/Q3/ZScore/RobustZScore/PercentRank/MAD)")]
    public IEnumerable<string> Statistics { get; set; } = Array.Empty<string>();

    [Option("suffix", Default = "_stat",
        HelpText = "Suffix for output column names")]
    public string Suffix { get; set; } = "_stat";

    public override Type GetHandlerType() => typeof(BasicStatisticsHandler);

    protected override bool ValidateInternal(ILogger logger)
    {
        if (!base.ValidateInternal(logger))
        {
            return false;
        }

        if (!Statistics.Any())
        {
            logger.LogError("At least one statistic type must be specified");
            return false;
        }

        // Validate suffix
        if (string.IsNullOrWhiteSpace(Suffix))
        {
            logger.LogError("Suffix cannot be empty");
            return false;
        }

        // Validate statistic types
        foreach (var stat in Statistics)
        {
            if (!Enum.TryParse<StatisticType>(stat, true, out _))
            {
                logger.LogError("Invalid statistic type: {Type}. Valid values are: {ValidValues}",
                    stat, string.Join(", ", Enum.GetNames<StatisticType>()));
                return false;
            }
        }

        return true;
    }
}