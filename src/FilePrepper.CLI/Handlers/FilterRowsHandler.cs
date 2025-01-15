using FilePrepper.Tasks.FilterRows;
using FilePrepper.Tasks;
using Microsoft.Extensions.Logging;
using FilePrepper.CLI.Parameters;

namespace FilePrepper.CLI.Handlers;

public class FilterRowsHandler : ICommandHandler
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<FilterRowsHandler> _logger;

    public FilterRowsHandler(
        ILoggerFactory loggerFactory,
        ILogger<FilterRowsHandler> logger)
    {
        _loggerFactory = loggerFactory;
        _logger = logger;
    }

    public async Task<int> ExecuteAsync(ICommandParameters parameters)
    {
        var opts = (FilterRowsParameters)parameters;

        try
        {
            var conditions = new List<FilterCondition>();
            foreach (var condition in opts.Conditions)
            {
                var parts = condition.Split(':', 3);
                if (parts.Length != 3)
                {
                    _logger.LogError("Invalid condition format: {Condition}. Expected format: column:operator:value", condition);
                    return 1;
                }

                if (!Enum.TryParse<FilterOperator>(parts[1], true, out var filterOperator))
                {
                    _logger.LogError("Invalid filter operator: {Operator}. Valid values are: {ValidValues}",
                        parts[1], string.Join(", ", Enum.GetNames<FilterOperator>()));
                    return 1;
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
                Conditions = conditions,
                Common = opts.GetCommonOptions()
            };

            var taskLogger = _loggerFactory.CreateLogger<FilterRowsTask>();
            var task = new FilterRowsTask(options, taskLogger);
            var context = new TaskContext
            {
                InputPath = opts.InputPath,
                OutputPath = opts.OutputPath
            };

            return await task.ExecuteAsync(context) ? 0 : 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing filter-rows command");
            return 1;
        }
    }

    public string? GetExample() =>
    "filter-rows -i input.csv -o output.csv -c \"Age:GreaterThan:30,Status:Equals:Active\"";
}