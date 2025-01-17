using FilePrepper.Tasks;
using FilePrepper.Tasks.FilterRows;
using Microsoft.Extensions.Logging;

namespace FilePrepper.CLI.Tools.FilterRows;

public class FilterRowsHandler : BaseCommandHandler<FilterRowsParameters>
{
    public FilterRowsHandler(
        ILoggerFactory loggerFactory,
        ILogger<FilterRowsHandler> logger)
        : base(loggerFactory, logger)
    {
    }

    public override async Task<int> ExecuteAsync(ICommandParameters parameters)
    {
        var opts = (FilterRowsParameters)parameters;
        if (!ValidateParameters(opts))
        {
            return ExitCodes.InvalidArguments;
        }

        return await HandleExceptionAsync(async () =>
        {
            var conditions = new List<FilterCondition>();

            foreach (var condStr in opts.Conditions)
            {
                var parts = condStr.Split(':');
                if (!Enum.TryParse<FilterOperator>(parts[1], true, out var filterOperator))
                {
                    _logger.LogError("Invalid filter operator: {Operator}", parts[1]);
                    return ExitCodes.InvalidArguments;
                }

                conditions.Add(new FilterCondition
                {
                    ColumnName = parts[0],
                    Operator = filterOperator,
                    Value = parts[2]
                });
            }

            var options = new FilterRowsOption
            {
                InputPath = opts.InputPath,
                OutputPath = opts.OutputPath,
                Conditions = conditions,
                HasHeader = opts.HasHeader,
                IgnoreErrors = opts.IgnoreErrors
            };

            var taskLogger = _loggerFactory.CreateLogger<FilterRowsTask>();
            var task = new FilterRowsTask(taskLogger);
            var context = new TaskContext(options);

            _logger.LogInformation("Filtering rows in {Input} using {Count} conditions",
                opts.InputPath, conditions.Count);

            var success = await task.ExecuteAsync(context);
            return success ? ExitCodes.Success : ExitCodes.Error;
        });
    }
}