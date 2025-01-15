using FilePrepper.Tasks.NormalizeData;
using FilePrepper.Tasks;
using Microsoft.Extensions.Logging;
using FilePrepper.CLI.Parameters;

namespace FilePrepper.CLI.Handlers;

public class NormalizeDataHandler : ICommandHandler
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<NormalizeDataHandler> _logger;

    public NormalizeDataHandler(
        ILoggerFactory loggerFactory,
        ILogger<NormalizeDataHandler> logger)
    {
        _loggerFactory = loggerFactory;
        _logger = logger;
    }

    public async Task<int> ExecuteAsync(ICommandParameters parameters)
    {
        var opts = (NormalizeDataParameters)parameters;

        try
        {
            if (!opts.TargetColumns.Any())
            {
                _logger.LogError("At least one target column must be specified");
                return 1;
            }

            if (!Enum.TryParse<NormalizationMethod>(opts.Method, true, out var method))
            {
                _logger.LogError("Invalid normalization method: {Method}. Valid values are: {ValidValues}",
                    opts.Method, string.Join(", ", Enum.GetNames<NormalizationMethod>()));
                return 1;
            }

            if (method == NormalizationMethod.MinMax)
            {
                if (opts.MinValue >= opts.MaxValue)
                {
                    _logger.LogError("Min value must be less than max value for MinMax normalization");
                    return 1;
                }
            }

            var options = new NormalizeDataOption
            {
                Method = method,
                MinValue = opts.MinValue,
                MaxValue = opts.MaxValue,
                TargetColumns = opts.TargetColumns.ToArray(),
                Common = opts.GetCommonOptions()
            };

            var taskLogger = _loggerFactory.CreateLogger<NormalizeDataTask>();
            var task = new NormalizeDataTask(options, taskLogger);
            var context = new TaskContext
            {
                InputPath = opts.InputPath,
                OutputPath = opts.OutputPath
            };

            return await task.ExecuteAsync(context) ? 0 : 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing normalize command");
            return 1;
        }
    }

    public string? GetExample() =>
    "normalize -i input.csv -o output.csv -c \"Price,Quantity\" -m MinMax --min 0 --max 1";
}