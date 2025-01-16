using CommandLine;
using FilePrepper.CLI.Tools;

namespace FilePrepper.CLI.Tools.Aggregate;

[Verb("aggregate", HelpText = "Aggregate data based on group by columns")]
public class AggregateParameters : SingleInputParameters
{
    [Option('g', "group-by", Required = true, Separator = ',',
        HelpText = "Columns to group by")]
    public IEnumerable<string> GroupByColumns { get; set; } = Array.Empty<string>();

    [Option('a', "aggregates", Required = true, Separator = ',',
        HelpText = "Aggregate functions in format column:function:output (e.g. Sales:Sum:TotalSales,Price:Average:AvgPrice)")]
    public IEnumerable<string> AggregateColumns { get; set; } = Array.Empty<string>();

    [Option("append", Default = false,
        HelpText = "Append aggregated values to source rows")]
    public bool AppendToSource { get; set; } = false;

    [Option("template", Default = "{column}_{function}_{groupBy}",
        HelpText = "Column name template when appending to source")]
    public string? OutputColumnTemplate { get; set; }

    public override Type GetHandlerType() => typeof(AggregateHandler);
}