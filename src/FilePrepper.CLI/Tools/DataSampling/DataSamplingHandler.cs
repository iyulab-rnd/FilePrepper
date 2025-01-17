using FilePrepper.Tasks;
using FilePrepper.Tasks.DataSampling;
using Microsoft.Extensions.Logging;

namespace FilePrepper.CLI.Tools.DataSampling;

public class DataSamplingHandler : BaseCommandHandler<DataSamplingParameters>
{
    public DataSamplingHandler(
        ILoggerFactory loggerFactory,
        ILogger<DataSamplingHandler> logger)
        : base(loggerFactory, logger)
    {
    }

    public override async Task<int> ExecuteAsync(ICommandParameters parameters)
    {
        var opts = (DataSamplingParameters)parameters;
        if (!ValidateParameters(opts))
        {
            return ExitCodes.InvalidArguments;
        }

        return await HandleExceptionAsync(async () =>
        {
            if (!Enum.TryParse<SamplingMethod>(opts.Method, true, out var method))
            {
                _logger.LogError("Invalid sampling method: {Method}", opts.Method);
                return ExitCodes.InvalidArguments;
            }

            var options = new DataSamplingOption
            {
                InputPath = opts.InputPath,
                OutputPath = opts.OutputPath,
                Method = method,
                SampleSize = opts.SampleSize,
                Seed = opts.Seed,
                StratifyColumn = opts.StratifyColumn,
                SystematicInterval = opts.SystematicInterval,
                HasHeader = opts.HasHeader,
                IgnoreErrors = opts.IgnoreErrors
            };

            var taskLogger = _loggerFactory.CreateLogger<DataSamplingTask>();
            var task = new DataSamplingTask(taskLogger);
            var context = new TaskContext(options);

            var success = await task.ExecuteAsync(context);
            return success ? ExitCodes.Success : ExitCodes.Error;
        });
    }
}