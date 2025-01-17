using FilePrepper.Tasks;
using FilePrepper.Tasks.OneHotEncoding;
using Microsoft.Extensions.Logging;

namespace FilePrepper.CLI.Tools.OneHotEncoding;

public class OneHotEncodingHandler : BaseCommandHandler<OneHotEncodingParameters>
{
    public OneHotEncodingHandler(
        ILoggerFactory loggerFactory,
        ILogger<OneHotEncodingHandler> logger)
        : base(loggerFactory, logger)
    {
    }

    public override async Task<int> ExecuteAsync(ICommandParameters parameters)
    {
        var opts = (OneHotEncodingParameters)parameters;
        if (!ValidateParameters(opts))
        {
            return ExitCodes.InvalidArguments;
        }

        return await HandleExceptionAsync(async () =>
        {
            var options = new OneHotEncodingOption
            {
                InputPath = opts.InputPath,
                OutputPath = opts.OutputPath,
                TargetColumns = opts.TargetColumns.ToArray(),
                DropFirst = opts.DropFirst,
                KeepOriginalColumns = opts.KeepOriginalColumns,
                HasHeader = opts.HasHeader,
                IgnoreErrors = opts.IgnoreErrors
            };

            var taskLogger = _loggerFactory.CreateLogger<OneHotEncodingTask>();
            var task = new OneHotEncodingTask(taskLogger);
            var context = new TaskContext(options);

            _logger.LogInformation("Performing one-hot encoding on columns {Columns} in {Input}. Drop first: {DropFirst}, Keep original: {KeepOriginal}",
                string.Join(", ", opts.TargetColumns), opts.InputPath, opts.DropFirst, opts.KeepOriginalColumns);

            var success = await task.ExecuteAsync(context);
            return success ? ExitCodes.Success : ExitCodes.Error;
        });
    }
}