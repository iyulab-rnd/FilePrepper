using FilePrepper.CLI.Tools;
using FilePrepper.Tasks;
using FilePrepper.Tasks.ColumnInteraction;
using Microsoft.Extensions.Logging;

namespace FilePrepper.CLI.Tools.ColumnInteraction;

public class ColumnInteractionHandler : ICommandHandler
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<ColumnInteractionHandler> _logger;

    public ColumnInteractionHandler(
        ILoggerFactory loggerFactory,
        ILogger<ColumnInteractionHandler> logger)
    {
        _loggerFactory = loggerFactory;
        _logger = logger;
    }

    public async Task<int> ExecuteAsync(ICommandParameters parameters)
    {
        var opts = (ColumnInteractionParameters)parameters;

        try
        {
            // Validate operation type
            if (!Enum.TryParse<OperationType>(opts.Operation, true, out var operationType))
            {
                _logger.LogError("Invalid operation type: {Operation}. Valid values are: {ValidValues}",
                    opts.Operation, string.Join(", ", Enum.GetNames<OperationType>()));
                return 1;
            }

            // Validate custom expression when needed
            if (operationType == OperationType.Custom && string.IsNullOrWhiteSpace(opts.CustomExpression))
            {
                _logger.LogError("Custom expression is required when using Custom operation type");
                return 1;
            }

            // Validate source columns count
            if (opts.SourceColumns.Count() < 2)
            {
                _logger.LogError("At least two source columns must be specified");
                return 1;
            }

            var options = new ColumnInteractionOption
            {
                SourceColumns = opts.SourceColumns.ToArray(),
                Operation = operationType,
                OutputColumn = opts.OutputColumn,
                CustomExpression = opts.CustomExpression,
                Common = opts.GetCommonOptions()
            };

            var taskLogger = _loggerFactory.CreateLogger<ColumnInteractionTask>();
            var task = new ColumnInteractionTask(options, taskLogger);
            var context = new TaskContext
            {
                InputPath = opts.InputPath,
                OutputPath = opts.OutputPath
            };

            return await task.ExecuteAsync(context) ? 0 : 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing column-interaction command");
            return 1;
        }
    }

    public string? GetExample() =>
    "column-interaction -i input.csv -o output.csv -s \"Price,Quantity\" -t Multiply -o Total";
}