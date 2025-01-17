using FilePrepper.Tasks.ReorderColumns;
using FilePrepper.Tasks;
using Microsoft.Extensions.Logging;

namespace FilePrepper.CLI.Tools.ReorderColumns;

public class ReorderColumnsHandler : BaseCommandHandler<ReorderColumnsParameters>
{
    public ReorderColumnsHandler(
        ILoggerFactory loggerFactory,
        ILogger<ReorderColumnsHandler> logger)
        : base(loggerFactory, logger)
    {
    }

    public override async Task<int> ExecuteAsync(ICommandParameters parameters)
    {
        var opts = (ReorderColumnsParameters)parameters;
        if (!ValidateParameters(opts))
        {
            return ExitCodes.InvalidArguments;
        }

        return await HandleExceptionAsync(async () =>
        {
            var options = new ReorderColumnsOption
            {
                Order = opts.Order.ToList(),
                InputPath = opts.InputPath,
                OutputPath = opts.OutputPath,
                HasHeader = opts.HasHeader,
                IgnoreErrors = opts.IgnoreErrors
            };

            var taskLogger = _loggerFactory.CreateLogger<ReorderColumnsTask>();
            var task = new ReorderColumnsTask(taskLogger);
            var context = new TaskContext(options);

            _logger.LogInformation("Reordering columns in {Input}. Order: {Order}",
                opts.InputPath, string.Join(", ", opts.Order));

            var success = await task.ExecuteAsync(context);
            return success ? ExitCodes.Success : ExitCodes.Error;
        });
    }
}