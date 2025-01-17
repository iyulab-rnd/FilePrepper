using FilePrepper.Tasks;
using FilePrepper.Tasks.DropDuplicates;
using Microsoft.Extensions.Logging;

namespace FilePrepper.CLI.Tools.DropDuplicates;

public class DropDuplicatesHandler : BaseCommandHandler<DropDuplicatesParameters>
{
    public DropDuplicatesHandler(
        ILoggerFactory loggerFactory,
        ILogger<DropDuplicatesHandler> logger)
        : base(loggerFactory, logger)
    {
    }

    public override async Task<int> ExecuteAsync(ICommandParameters parameters)
    {
        var opts = (DropDuplicatesParameters)parameters;
        if (!ValidateParameters(opts))
        {
            return ExitCodes.InvalidArguments;
        }

        return await HandleExceptionAsync(async () =>
        {
            var options = new DropDuplicatesOption
            {
                InputPath = opts.InputPath,
                OutputPath = opts.OutputPath,
                KeepFirst = opts.KeepFirst,
                SubsetColumnsOnly = opts.SubsetColumnsOnly,
                TargetColumns = opts.TargetColumns.ToArray(),
                HasHeader = opts.HasHeader,
                IgnoreErrors = opts.IgnoreErrors
            };

            var taskLogger = _loggerFactory.CreateLogger<DropDuplicatesTask>();
            var task = new DropDuplicatesTask(taskLogger);
            var context = new TaskContext(options);

            _logger.LogInformation("Removing duplicates from {Input}. Keep first: {KeepFirst}, Subset only: {SubsetOnly}",
                opts.InputPath, opts.KeepFirst, opts.SubsetColumnsOnly);

            var success = await task.ExecuteAsync(context);
            return success ? ExitCodes.Success : ExitCodes.Error;
        });
    }
}