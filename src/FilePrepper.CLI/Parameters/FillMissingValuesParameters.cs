using CommandLine;
using FilePrepper.CLI.Handlers;

namespace FilePrepper.CLI.Parameters;

[Verb("fill-missing", HelpText = "Fill missing values in columns")]
public class FillMissingValuesParameters : SingleInputParameters
{
    [Option('m', "methods", Required = true, Separator = ',',
        HelpText = "Fill methods in format column:method[:value] (e.g. Age:Mean or Score:FixedValue:0)")]
    public IEnumerable<string> FillMethods { get; set; } = Array.Empty<string>();

    public override Type GetHandlerType() => typeof(FillMissingValuesHandler);
}
