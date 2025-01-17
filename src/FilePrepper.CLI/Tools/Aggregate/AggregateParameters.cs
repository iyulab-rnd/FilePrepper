using CommandLine;
using Microsoft.Extensions.Logging;


namespace FilePrepper.CLI.Tools.Aggregate;

/// <summary>
/// aggregate 명령어의 매개변수를 정의하는 클래스
/// </summary>
[Verb("aggregate", HelpText = "Aggregate data based on group by columns")]
public class AggregateParameters : SingleInputParameters, IAppendableParameters
{
    [Option('g', "group-by", Required = true, Separator = ',',
        HelpText = "Columns to group by")]
    public IEnumerable<string> GroupByColumns { get; set; } = Array.Empty<string>();

    [Option('a', "aggregates", Required = true, Separator = ',',
        HelpText = "Aggregate functions in format column:function:output (e.g. Sales:Sum:TotalSales,Price:Average:AvgPrice)")]
    public IEnumerable<string> AggregateColumns { get; set; } = Array.Empty<string>();

    public override Type GetHandlerType() => typeof(AggregateHandler);

    protected override bool ValidateInternal(ILogger logger)
    {
        if (!base.ValidateInternal(logger))
        {
            return false;
        }

        if (!GroupByColumns.Any())
        {
            logger.LogError("At least one group by column must be specified");
            return false;
        }

        if (!AggregateColumns.Any())
        {
            logger.LogError("At least one aggregate function must be specified");
            return false;
        }

        foreach (var col in GroupByColumns)
        {
            if (string.IsNullOrWhiteSpace(col))
            {
                logger.LogError("Group by column name cannot be empty");
                return false;
            }
        }

        // Validate aggregate column definitions
        foreach (var agg in AggregateColumns)
        {
            var parts = agg.Split(':');
            if (parts.Length != 3)
            {
                logger.LogError("Invalid aggregate format: {Aggregate}. Expected format: column:function:output", agg);
                return false;
            }

            if (string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[2]))
            {
                logger.LogError("Column names cannot be empty in aggregate definition: {Aggregate}", agg);
                return false;
            }
        }

        // Validate template when appending
        if (AppendToSource && string.IsNullOrWhiteSpace(OutputColumnTemplate))
        {
            logger.LogError("Output column template is required when appending to source");
            return false;
        }

        return true;
    }

    public override string? GetExample() =>
    "aggregate -i input.csv -o output.csv -g \"Region,Category\" -a \"Sales:Sum:TotalSales,Price:Average:AvgPrice\"";
}