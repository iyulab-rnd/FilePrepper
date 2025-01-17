using FilePrepper.Tasks;
using FilePrepper.Tasks.RemoveColumns;
using Microsoft.Extensions.Logging;

namespace FilePrepper.CLI.Tools.RemoveColumns;

public class RemoveColumnsHandler : BaseCommandHandler<RemoveColumnsParameters>
{
    public RemoveColumnsHandler(
        ILoggerFactory loggerFactory,
        ILogger<RemoveColumnsHandler> logger)
        : base(loggerFactory, logger)
    {
    }

    public override async Task<int> ExecuteAsync(ICommandParameters parameters)
    {
        var opts = (RemoveColumnsParameters)parameters;
        if (!ValidateParameters(opts))
        {
            return ExitCodes.InvalidArguments;
        }

        return await HandleExceptionAsync(async () =>
        {
            var options = new RemoveColumnsOption
            {
                InputPath = opts.InputPath,
                OutputPath = opts.OutputPath,
                RemoveColumns = opts.Columns.ToList(),
                HasHeader = opts.HasHeader,
                IgnoreErrors = opts.IgnoreErrors
            };

            var taskLogger = _loggerFactory.CreateLogger<RemoveColumnsTask>();
            var task = new RemoveColumnsTask(taskLogger);
            var context = new TaskContext(options);

            _logger.LogInformation("Removing columns {Columns} from {Input}",
                string.Join(", ", opts.Columns), opts.InputPath);

            var success = await task.ExecuteAsync(context);
            return success ? ExitCodes.Success : ExitCodes.Error;
        });
    }
}