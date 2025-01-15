using CommandLine;
using FilePrepper.CLI.Handlers;

namespace FilePrepper.CLI.Parameters;

[Verb("normalize", HelpText = "Normalize numeric columns")]
public class NormalizeDataParameters : SingleInputParameters
{
    [Option('c', "columns", Required = true, Separator = ',',
        HelpText = "Columns to normalize")]
    public IEnumerable<string> TargetColumns { get; set; } = Array.Empty<string>();

    [Option('m', "method", Required = true,
        HelpText = "Normalization method (MinMax/ZScore)")]
    public string Method { get; set; } = string.Empty;

    [Option("min", Default = 0.0,
        HelpText = "Min value for MinMax scaling")]
    public double MinValue { get; set; }

    [Option("max", Default = 1.0,
        HelpText = "Max value for MinMax scaling")]
    public double MaxValue { get; set; }

    public override Type GetHandlerType() => typeof(NormalizeDataHandler);
}
