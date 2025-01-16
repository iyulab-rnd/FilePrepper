using FilePrepper.CLI.Tools;
using FilePrepper.Tasks;
using FilePrepper.Tasks.OneHotEncoding;
using Microsoft.Extensions.Logging;

namespace FilePrepper.CLI.Tools.OneHotEncoding;

public class OneHotEncodingHandler : ICommandHandler
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<OneHotEncodingHandler> _logger;

    public OneHotEncodingHandler(
        ILoggerFactory loggerFactory,
        ILogger<OneHotEncodingHandler> logger)
    {
        _loggerFactory = loggerFactory;
        _logger = logger;
    }

    public async Task<int> ExecuteAsync(ICommandParameters parameters)
    {
        var opts = (OneHotEncodingParameters)parameters;

        try
        {
            if (!opts.Columns.Any())
            {
                _logger.LogError("At least one column must be specified for one-hot encoding");
                return 1;
            }

            var options = new OneHotEncodingOption
            {
                TargetColumns = opts.Columns.ToArray(),
                DropFirst = opts.DropFirst,
                KeepOriginalColumns = opts.KeepOriginalColumns,
                Common = opts.GetCommonOptions()
            };

            var taskLogger = _loggerFactory.CreateLogger<OneHotEncodingTask>();
            var task = new OneHotEncodingTask(options, taskLogger);
            var context = new TaskContext
            {
                InputPath = opts.InputPath,
                OutputPath = opts.OutputPath
            };

            return await task.ExecuteAsync(context) ? 0 : 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing one-hot-encoding command");
            return 1;
        }
    }

    public string? GetExample() =>
    "one-hot-encoding -i input.csv -o output.csv -c \"Category,Status\" --keep-original";
}
