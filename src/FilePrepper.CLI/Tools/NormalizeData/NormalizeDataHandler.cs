using FilePrepper.Tasks;
using FilePrepper.Tasks.NormalizeData;
using Microsoft.Extensions.Logging;

namespace FilePrepper.CLI.Tools.NormalizeData;

public class NormalizeDataHandler : BaseCommandHandler<NormalizeDataParameters>
{
    public NormalizeDataHandler(
        ILoggerFactory loggerFactory,
        ILogger<NormalizeDataHandler> logger)
        : base(loggerFactory, logger)
    {
    }

    public override async Task<int> ExecuteAsync(ICommandParameters parameters)
    {
        var opts = (NormalizeDataParameters)parameters;
        if (!ValidateParameters(opts))
        {
            return ExitCodes.InvalidArguments;
        }

        return await HandleExceptionAsync(async () =>
        {
            if (!Enum.TryParse<NormalizationMethod>(opts.Method, true, out var method))
            {
                _logger.LogError("Invalid normalization method: {Method}", opts.Method);
                return ExitCodes.InvalidArguments;
            }

            var options = new NormalizeDataOption
            {
                InputPath = opts.InputPath,
                OutputPath = opts.OutputPath,
                Method = method,
                MinValue = opts.MinValue,
                MaxValue = opts.MaxValue,
                TargetColumns = opts.TargetColumns.ToArray(),
                HasHeader = opts.HasHeader,
                IgnoreErrors = opts.IgnoreErrors
            };

            var taskLogger = _loggerFactory.CreateLogger<NormalizeDataTask>();
            var task = new NormalizeDataTask(taskLogger);
            var context = new TaskContext(options);

            _logger.LogInformation("Normalizing columns {Columns} in {Input} using {Method} method",
                string.Join(", ", opts.TargetColumns), opts.InputPath, method);

            var success = await task.ExecuteAsync(context);
            return success ? ExitCodes.Success : ExitCodes.Error;
        });
    }

    public override string? GetExample() =>
        "normalize -i input.csv -o output.csv -c \"Price,Quantity\" -m MinMax --min 0 --max 1";
}