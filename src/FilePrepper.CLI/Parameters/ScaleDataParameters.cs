using CommandLine;
using FilePrepper.CLI.Handlers;

namespace FilePrepper.CLI.Parameters;

[Verb("scale", HelpText = "Scale numeric columns")]
public class ScaleDataParameters : BaseParameters
{
    [Option('s', "scaling", Required = true, Separator = ',',
        HelpText = "Scaling methods in format column:method (e.g. Price:MinMax,Score:Standardization)")]
    public IEnumerable<string> ScaleColumns { get; set; } = Array.Empty<string>();

    public override Type GetHandlerType() => typeof(ScaleDataHandler);
}
