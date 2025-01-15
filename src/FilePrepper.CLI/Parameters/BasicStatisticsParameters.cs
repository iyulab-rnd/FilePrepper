using CommandLine;
using FilePrepper.CLI.Handlers;

namespace FilePrepper.CLI.Parameters;

[Verb("stats", HelpText = "Calculate basic statistics on columns")]
public class BasicStatisticsParameters : SingleInputParameters
{
    [Option('c', "columns", Required = true, Separator = ',',
        HelpText = "Columns to calculate statistics for")]
    public IEnumerable<string> TargetColumns { get; set; } = Array.Empty<string>();

    [Option('s', "stats", Required = true, Separator = ',',
        HelpText = "Statistics to calculate (Mean/StandardDeviation/Min/Max/Median/Q1/Q3/ZScore/RobustZScore/PercentRank/MAD)")]
    public IEnumerable<string> Statistics { get; set; } = Array.Empty<string>();

    [Option("suffix", Default = "_stat",
        HelpText = "Suffix for output column names")]
    public string Suffix { get; set; } = "_stat";

    public override Type GetHandlerType() => typeof(BasicStatisticsHandler);
}
