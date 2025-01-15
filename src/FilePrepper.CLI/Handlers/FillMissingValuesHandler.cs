using FilePrepper.Tasks.FillMissingValues;
using FilePrepper.Tasks;
using Microsoft.Extensions.Logging;
using FilePrepper.CLI.Parameters;

namespace FilePrepper.CLI.Handlers;

public class FillMissingValuesHandler : ICommandHandler
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<FillMissingValuesHandler> _logger;

    public FillMissingValuesHandler(
        ILoggerFactory loggerFactory,
        ILogger<FillMissingValuesHandler> logger)
    {
        _loggerFactory = loggerFactory;
        _logger = logger;
    }

    public async Task<int> ExecuteAsync(ICommandParameters parameters)
    {
        var opts = (FillMissingValuesParameters)parameters;

        try
        {
            var fillMethods = new List<ColumnFillMethod>();
            foreach (var methodStr in opts.FillMethods)
            {
                var parts = methodStr.Split(':');
                if (parts.Length < 2 || parts.Length > 3)
                {
                    _logger.LogError("Invalid fill method format: {Method}. Expected format: column:method[:value]", methodStr);
                    return 1;
                }

                if (!Enum.TryParse<FillMethod>(parts[1], true, out var method))
                {
                    _logger.LogError("Invalid fill method: {Method}. Valid values are: {ValidValues}",
                        parts[1], string.Join(", ", Enum.GetNames<FillMethod>()));
                    return 1;
                }

                // FixedValue 메서드는 값이 반드시 필요
                if (method == FillMethod.FixedValue && parts.Length != 3)
                {
                    _logger.LogError("Fixed value must be specified for FixedValue method: {Method}", methodStr);
                    return 1;
                }

                fillMethods.Add(new ColumnFillMethod
                {
                    ColumnName = parts[0],
                    Method = method,
                    FixedValue = parts.Length > 2 ? parts[2] : null
                });
            }

            var options = new FillMissingValuesOption
            {
                FillMethods = fillMethods,
                Common = opts.GetCommonOptions()
            };

            var taskLogger = _loggerFactory.CreateLogger<FillMissingValuesTask>();
            var task = new FillMissingValuesTask(options, taskLogger);
            var context = new TaskContext
            {
                InputPath = opts.InputPath,
                OutputPath = opts.OutputPath
            };

            return await task.ExecuteAsync(context) ? 0 : 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing fill-missing command");
            return 1;
        }
    }

    public string? GetExample() =>
    "fill-missing -i input.csv -o output.csv -m \"Age:Mean,Name:FixedValue:Unknown,Score:Median\"";
}