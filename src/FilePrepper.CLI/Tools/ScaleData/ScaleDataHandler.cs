using FilePrepper.Tasks.ScaleData;
using FilePrepper.Tasks;
using Microsoft.Extensions.Logging;
using FilePrepper.CLI.Tools;

namespace FilePrepper.CLI.Tools.ScaleData;

public class ScaleDataHandler : ICommandHandler
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<ScaleDataHandler> _logger;

    public ScaleDataHandler(
        ILoggerFactory loggerFactory,
        ILogger<ScaleDataHandler> logger)
    {
        _loggerFactory = loggerFactory;
        _logger = logger;
    }

    public async Task<int> ExecuteAsync(ICommandParameters parameters)
    {
        var opts = (ScaleDataParameters)parameters;

        try
        {
            var scaleColumns = new List<ScaleColumnOption>();

            foreach (var scaleStr in opts.ScaleColumns)
            {
                var parts = scaleStr.Split(':');
                if (parts.Length != 2)
                {
                    _logger.LogError("Invalid scaling format: {Scale}. Expected format: column:method", scaleStr);
                    return 1;
                }

                if (!Enum.TryParse<ScaleMethod>(parts[1], true, out var method))
                {
                    _logger.LogError("Invalid scale method: {Method}. Valid values are: {ValidValues}",
                        parts[1], string.Join(", ", Enum.GetNames<ScaleMethod>()));
                    return 1;
                }

                scaleColumns.Add(new ScaleColumnOption
                {
                    ColumnName = parts[0],
                    Method = method
                });
            }

            if (!scaleColumns.Any())
            {
                _logger.LogError("At least one column scaling must be specified");
                return 1;
            }

            var options = new ScaleDataOption
            {
                ScaleColumns = scaleColumns,
                Common = opts.GetCommonOptions()
            };

            var taskLogger = _loggerFactory.CreateLogger<ScaleDataTask>();
            var task = new ScaleDataTask(options, taskLogger);
            var context = new TaskContext
            {
                InputPath = opts.InputPath,
                OutputPath = opts.OutputPath
            };

            return await task.ExecuteAsync(context) ? 0 : 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing scale command");
            return 1;
        }
    }

    public string? GetExample() =>
    "scale -i input.csv -o output.csv -s \"Price:MinMax,Score:Standardization\"";
}