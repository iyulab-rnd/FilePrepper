using CommandLine;
using FilePrepper.Tasks;
using FilePrepper.Tasks.BasicStatistics;
using Microsoft.Extensions.Logging;

namespace FilePrepper.CLI.Tools.BasicStatistics;

/// <summary>
/// CLI의 stats 명령어 핸들러
/// </summary>
public class BasicStatisticsHandler : BaseCommandHandler<BasicStatisticsParameters>
{
    public BasicStatisticsHandler(
        ILoggerFactory loggerFactory,
        ILogger<BasicStatisticsHandler> logger)
        : base(loggerFactory, logger)
    {
    }

    public override async Task<int> ExecuteAsync(ICommandParameters parameters)
    {
        var opts = (BasicStatisticsParameters)parameters;
        if (!ValidateParameters(opts))
        {
            return ExitCodes.InvalidArguments;
        }

        return await HandleExceptionAsync(async () =>
        {
            var statistics = ParseStatisticTypes(opts.Statistics);
            if (statistics == null)
            {
                return ExitCodes.InvalidArguments;
            }

            var options = new BasicStatisticsOption
            {
                InputPath = opts.InputPath,
                OutputPath = opts.OutputPath,
                TargetColumns = opts.TargetColumns.ToArray(),
                Statistics = statistics.ToArray(),
                Suffix = opts.Suffix,
                HasHeader = opts.HasHeader,
                IgnoreErrors = opts.IgnoreErrors,
                DefaultValue = opts.DefaultValue
            };

            var taskLogger = _loggerFactory.CreateLogger<BasicStatisticsTask>();
            var task = new BasicStatisticsTask(taskLogger);
            var context = new TaskContext(options);

            _logger.LogInformation("Calculating statistics {Stats} for columns {Columns}",
                string.Join(", ", statistics), string.Join(", ", opts.TargetColumns));

            var success = await task.ExecuteAsync(context);
            return success ? ExitCodes.Success : ExitCodes.Error;
        });
    }

    private List<StatisticType>? ParseStatisticTypes(IEnumerable<string> statDefs)
    {
        try
        {
            var statistics = new List<StatisticType>();

            foreach (var stat in statDefs)
            {
                if (!Enum.TryParse<StatisticType>(stat, true, out var statType))
                {
                    _logger.LogError("Invalid statistic type: {Type}. Valid values are: {ValidValues}",
                        stat, string.Join(", ", Enum.GetNames<StatisticType>()));
                    return null;
                }
                statistics.Add(statType);
            }

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing statistic types");
            return null;
        }
    }
}