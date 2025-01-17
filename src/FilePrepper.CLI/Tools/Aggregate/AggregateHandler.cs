using CommandLine;
using FilePrepper.Tasks;
using FilePrepper.Tasks.Aggregate;
using Microsoft.Extensions.Logging;

namespace FilePrepper.CLI.Tools.Aggregate;

/// <summary>
/// CLI의 aggregate 명령어 핸들러
/// </summary>
public class AggregateHandler : BaseCommandHandler<AggregateParameters>
{
    public AggregateHandler(
        ILoggerFactory loggerFactory,
        ILogger<AggregateHandler> logger)
        : base(loggerFactory, logger)
    {
    }

    public override async Task<int> ExecuteAsync(ICommandParameters parameters)
    {
        var opts = (AggregateParameters)parameters;
        if (!ValidateParameters(opts))
        {
            return ExitCodes.InvalidArguments;
        }

        return await HandleExceptionAsync(async () =>
        {
            var aggregateColumns = ParseAggregateColumns(opts.AggregateColumns);
            if (aggregateColumns == null)
            {
                return ExitCodes.InvalidArguments;
            }

            var options = new AggregateOption
            {
                InputPath = opts.InputPath,
                OutputPath = opts.OutputPath,
                GroupByColumns = opts.GroupByColumns.ToArray(),
                AggregateColumns = aggregateColumns,
                HasHeader = opts.HasHeader,
                IgnoreErrors = opts.IgnoreErrors,
                AppendToSource = opts.AppendToSource,
                OutputColumnTemplate = opts.OutputColumnTemplate
            };

            var taskLogger = _loggerFactory.CreateLogger<AggregateTask>();
            var task = new AggregateTask(taskLogger);
            var context = new TaskContext(options);

            _logger.LogInformation("Aggregating data by {Columns} with {Count} aggregate functions",
                string.Join(", ", opts.GroupByColumns), aggregateColumns.Count);

            var success = await task.ExecuteAsync(context);
            return success ? ExitCodes.Success : ExitCodes.Error;
        });
    }

    private List<AggregateColumn>? ParseAggregateColumns(IEnumerable<string> aggregateDefs)
    {
        try
        {
            var columns = new List<AggregateColumn>();

            foreach (var agg in aggregateDefs)
            {
                var parts = agg.Split(':');
                if (parts.Length != 3)
                {
                    _logger.LogError("Invalid aggregate format: {Aggregate}. Expected format: column:function:output", agg);
                    return null;
                }

                if (!Enum.TryParse<AggregateFunction>(parts[1], true, out var function))
                {
                    _logger.LogError("Invalid aggregate function: {Function}. Valid values are: {ValidValues}",
                        parts[1], string.Join(", ", Enum.GetNames<AggregateFunction>()));
                    return null;
                }

                columns.Add(new AggregateColumn
                {
                    ColumnName = parts[0],
                    Function = function,
                    OutputColumnName = parts[2]
                });
            }

            return columns;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing aggregate column definitions");
            return null;
        }
    }

    public override string? GetExample() =>
        "aggregate -i input.csv -o output.csv -g \"Region,Category\" -a \"Sales:Sum:TotalSales,Price:Average:AvgPrice\"";
}
