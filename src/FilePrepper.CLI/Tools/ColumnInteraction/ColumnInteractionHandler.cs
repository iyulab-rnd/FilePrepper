using CommandLine;
using FilePrepper.Tasks;
using FilePrepper.Tasks.ColumnInteraction;
using Microsoft.Extensions.Logging;

namespace FilePrepper.CLI.Tools.ColumnInteraction;

/// <summary>
/// CLI의 column-interaction 명령어 핸들러
/// </summary>
public class ColumnInteractionHandler : BaseCommandHandler<ColumnInteractionParameters>
{
    public ColumnInteractionHandler(
        ILoggerFactory loggerFactory,
        ILogger<ColumnInteractionHandler> logger)
        : base(loggerFactory, logger)
    {
    }

    public override async Task<int> ExecuteAsync(ICommandParameters parameters)
    {
        var opts = (ColumnInteractionParameters)parameters;
        if (!ValidateParameters(opts))
        {
            return ExitCodes.InvalidArguments;
        }

        return await HandleExceptionAsync(async () =>
        {
            if (!Enum.TryParse<OperationType>(opts.Operation, true, out var operationType))
            {
                _logger.LogError("Invalid operation type: {Operation}. Valid values are: {ValidValues}",
                    opts.Operation, string.Join(", ", Enum.GetNames<OperationType>()));
                return ExitCodes.InvalidArguments;
            }

            var options = new ColumnInteractionOption
            {
                InputPath = opts.InputPath,
                OutputPath = opts.OutputPath,
                SourceColumns = opts.SourceColumns.ToArray(),
                Operation = operationType,
                OutputColumn = opts.OutputColumn,
                CustomExpression = opts.CustomExpression,
                DefaultValue = opts.DefaultValue,
                HasHeader = opts.HasHeader,
                IgnoreErrors = opts.IgnoreErrors
            };

            var taskLogger = _loggerFactory.CreateLogger<ColumnInteractionTask>();
            var task = new ColumnInteractionTask(taskLogger);
            var context = new TaskContext(options);

            _logger.LogInformation("Performing {Operation} operation on columns {Columns} to create {Output}",
                operationType, string.Join(", ", opts.SourceColumns), opts.OutputColumn);

            var success = await task.ExecuteAsync(context);
            return success ? ExitCodes.Success : ExitCodes.Error;
        });
    }
}