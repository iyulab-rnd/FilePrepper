using FilePrepper.Tasks;
using FilePrepper.Tasks.FillMissingValues;
using Microsoft.Extensions.Logging;

namespace FilePrepper.CLI.Tools.FillMissingValues;

public class FillMissingValuesHandler : BaseCommandHandler<FillMissingValuesParameters>
{
    public FillMissingValuesHandler(
        ILoggerFactory loggerFactory,
        ILogger<FillMissingValuesHandler> logger)
        : base(loggerFactory, logger)
    {
    }

    public override async Task<int> ExecuteAsync(ICommandParameters parameters)
    {
        var opts = (FillMissingValuesParameters)parameters;
        if (!ValidateParameters(opts))
        {
            return ExitCodes.InvalidArguments;
        }

        return await HandleExceptionAsync(async () =>
        {
            var fillMethods = new List<ColumnFillMethod>();

            foreach (var methodStr in opts.FillMethods)
            {
                var parts = methodStr.Split(':');
                if (!Enum.TryParse<FillMethod>(parts[1], true, out var method))
                {
                    _logger.LogError("Invalid fill method: {Method}", parts[1]);
                    return ExitCodes.InvalidArguments;
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
                InputPath = opts.InputPath,
                OutputPath = opts.OutputPath,
                FillMethods = fillMethods,
                HasHeader = opts.HasHeader,
                IgnoreErrors = opts.IgnoreErrors,
                DefaultValue = opts.DefaultValue,
                AppendToSource = opts.AppendToSource,
                OutputColumnTemplate = opts.OutputColumnTemplate
            };

            var taskLogger = _loggerFactory.CreateLogger<FillMissingValuesTask>();
            var task = new FillMissingValuesTask(taskLogger);
            var context = new TaskContext(options);

            _logger.LogInformation("Filling missing values in {Input} using {Count} methods",
                opts.InputPath, fillMethods.Count);

            var success = await task.ExecuteAsync(context);
            return success ? ExitCodes.Success : ExitCodes.Error;
        });
    }

    public override string? GetExample() =>
        "fill-missing -i input.csv -o output.csv -m \"Age:Mean,Name:FixedValue:Unknown,Score:Median\"";
}