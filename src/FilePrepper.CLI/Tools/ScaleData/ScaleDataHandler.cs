using FilePrepper.Tasks.ScaleData;
using FilePrepper.Tasks;
using Microsoft.Extensions.Logging;

namespace FilePrepper.CLI.Tools.ScaleData;

public class ScaleDataHandler : BaseCommandHandler<ScaleDataParameters>
{
    public ScaleDataHandler(
        ILoggerFactory loggerFactory,
        ILogger<ScaleDataHandler> logger)
        : base(loggerFactory, logger)
    {
    }

    public override async Task<int> ExecuteAsync(ICommandParameters parameters)
    {
        var opts = (ScaleDataParameters)parameters;
        if (!ValidateParameters(opts))
        {
            return ExitCodes.InvalidArguments;
        }

        return await HandleExceptionAsync(async () =>
        {
            var scaleColumns = opts.ScaleColumns.Select(scaleStr =>
            {
                var parts = scaleStr.Split(':');
                return new ScaleColumnOption
                {
                    ColumnName = parts[0],
                    Method = Enum.Parse<ScaleMethod>(parts[1], true)
                };
            }).ToList();

            var options = new ScaleDataOption
            {
                ScaleColumns = scaleColumns,
                InputPath = opts.InputPath,
                OutputPath = opts.OutputPath,
                HasHeader = opts.HasHeader,
                IgnoreErrors = opts.IgnoreErrors
            };

            var taskLogger = _loggerFactory.CreateLogger<ScaleDataTask>();
            var task = new ScaleDataTask(taskLogger);
            var context = new TaskContext(options);

            _logger.LogInformation("Scaling columns in {Input}. Methods: {Methods}",
                opts.InputPath, string.Join(", ", opts.ScaleColumns));

            var success = await task.ExecuteAsync(context);
            return success ? ExitCodes.Success : ExitCodes.Error;
        });
    }
}