using CommandLine;
using FilePrepper.CLI.Tools;

namespace FilePrepper.CLI.Tools.ScaleData;

[Verb("scale", HelpText = "Scale numeric columns")]
public class ScaleDataParameters : SingleInputParameters
{
    [Option('s', "scaling", Required = true, Separator = ',',
        HelpText = "Scaling methods in format column:method (e.g. Price:MinMax,Score:Standardization)")]
    public IEnumerable<string> ScaleColumns { get; set; } = Array.Empty<string>();

    public override Type GetHandlerType() => typeof(ScaleDataHandler);
}
