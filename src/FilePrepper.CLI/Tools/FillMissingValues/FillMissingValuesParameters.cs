using CommandLine;
using FilePrepper.CLI.Tools;

namespace FilePrepper.CLI.Tools.FillMissingValues;

[Verb("fill-missing", HelpText = "Fill missing values in columns")]
public class FillMissingValuesParameters : SingleInputParameters
{
    [Option('m', "methods", Required = true, Separator = ',',
        HelpText = "Fill methods in format column:method[:value] (e.g. Age:Mean or Score:FixedValue:0)")]
    public IEnumerable<string> FillMethods { get; set; } = Array.Empty<string>();

    public override Type GetHandlerType() => typeof(FillMissingValuesHandler);
}
