using FilePrepper.Tasks.BasicStatistics;
using FilePrepper.Tasks;
using Microsoft.Extensions.Logging;
using FilePrepper.CLI.Parameters;

namespace FilePrepper.CLI.Handlers;

public class BasicStatisticsHandler : ICommandHandler
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<BasicStatisticsHandler> _logger;

    public BasicStatisticsHandler(
        ILoggerFactory loggerFactory,
        ILogger<BasicStatisticsHandler> logger)
    {
        _loggerFactory = loggerFactory;
        _logger = logger;
    }

    public async Task<int> ExecuteAsync(ICommandParameters parameters)
    {
        var opts = (BasicStatisticsParameters)parameters;

        try
        {
            if (!opts.TargetColumns.Any())
            {
                _logger.LogError("At least one target column must be specified");
                return 1;
            }

            // Validate and parse statistics types
            var statistics = new List<StatisticType>();
            foreach (var statStr in opts.Statistics)
            {
                if (!Enum.TryParse<StatisticType>(statStr, true, out var statType))
                {
                    _logger.LogError("Invalid statistic type: {Type}. Valid values are: {ValidValues}",
                        statStr, string.Join(", ", Enum.GetNames<StatisticType>()));
                    return 1;
                }
                statistics.Add(statType);
            }

            if (!statistics.Any())
            {
                _logger.LogError("At least one statistic type must be specified");
                return 1;
            }

            // Validate suffix
            if (string.IsNullOrWhiteSpace(opts.Suffix))
            {
                _logger.LogError("Suffix cannot be empty");
                return 1;
            }

            var options = new BasicStatisticsOption
            {
                TargetColumns = opts.TargetColumns.ToArray(),
                Statistics = statistics.ToArray(),
                Suffix = opts.Suffix,
                Common = opts.GetCommonOptions()
            };

            var taskLogger = _loggerFactory.CreateLogger<BasicStatisticsTask>();
            var task = new BasicStatisticsTask(options, taskLogger);
            var context = new TaskContext
            {
                InputPath = opts.InputPath,
                OutputPath = opts.OutputPath
            };

            _logger.LogInformation("Calculating statistics: {Stats} for columns: {Columns}",
                string.Join(", ", statistics), string.Join(", ", opts.TargetColumns));

            return await task.ExecuteAsync(context) ? 0 : 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing stats command");
            return 1;
        }
    }

    public string? GetExample() =>
    "stats -i input.csv -o output.csv -c \"Price,Quantity\" -s \"Mean,Median,StandardDeviation\"";
}