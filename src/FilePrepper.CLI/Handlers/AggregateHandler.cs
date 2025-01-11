using FilePrepper.Tasks.Aggregate;
using FilePrepper.Tasks;
using Microsoft.Extensions.Logging;
using FilePrepper.CLI.Parameters;

namespace FilePrepper.CLI.Handlers;

public class AggregateHandler : ICommandHandler
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<AggregateHandler> _logger;

    public AggregateHandler(
        ILoggerFactory loggerFactory,
        ILogger<AggregateHandler> logger)
    {
        _loggerFactory = loggerFactory;
        _logger = logger;
    }

    public async Task<int> ExecuteAsync(ICommandParameters parameters)
    {
        var opts = (AggregateParameters)parameters;

        try
        {
            if (!opts.GroupByColumns.Any())
            {
                _logger.LogError("At least one group by column must be specified");
                return 1;
            }

            var aggregateColumns = new List<AggregateColumn>();
            foreach (var aggStr in opts.AggregateColumns)
            {
                var parts = aggStr.Split(':');
                if (parts.Length != 3)
                {
                    _logger.LogError("Invalid aggregate format: {Aggregate}. Expected format: column:function:output", aggStr);
                    return 1;
                }

                if (!Enum.TryParse<AggregateFunction>(parts[1], true, out var function))
                {
                    _logger.LogError("Invalid aggregate function: {Function}. Valid values are: {ValidValues}",
                        parts[1], string.Join(", ", Enum.GetNames<AggregateFunction>()));
                    return 1;
                }

                aggregateColumns.Add(new AggregateColumn
                {
                    ColumnName = parts[0],
                    Function = function,
                    OutputColumnName = parts[2]
                });
            }

            var options = new AggregateOption
            {
                GroupByColumns = opts.GroupByColumns.ToArray(),
                AggregateColumns = aggregateColumns,
                Common = opts.GetCommonOptions()
            };

            // Handle output options
            options.Common.Output.AppendToSource = opts.AppendToSource;
            if (opts.AppendToSource)
            {
                if (string.IsNullOrWhiteSpace(opts.OutputColumnTemplate))
                {
                    _logger.LogError("Output column template is required when appending to source");
                    return 1;
                }
                options.Common.Output.OutputColumnTemplate = opts.OutputColumnTemplate;
            }

            var taskLogger = _loggerFactory.CreateLogger<AggregateTask>();
            var task = new AggregateTask(options, taskLogger);
            var context = new TaskContext
            {
                InputPath = opts.InputPath,
                OutputPath = opts.OutputPath
            };

            return await task.ExecuteAsync(context) ? 0 : 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing aggregate command");
            return 1;
        }
    }
}