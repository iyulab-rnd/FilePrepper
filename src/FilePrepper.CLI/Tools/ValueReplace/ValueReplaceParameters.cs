using CommandLine;
using FilePrepper.CLI.Tools;

namespace FilePrepper.CLI.Tools.ValueReplace;

[Verb("replace", HelpText = "Replace values in columns")]
public class ValueReplaceParameters : SingleInputParameters
{
    [Option('r', "replacements", Required = true, Separator = ',',
        HelpText = "Replacement rules in format column:oldValue=newValue[;oldValue2=newValue2] (e.g. Status:active=1;inactive=0)")]
    public IEnumerable<string> ReplaceMethods { get; set; } = Array.Empty<string>();

    public override Type GetHandlerType() => typeof(ValueReplaceHandler);
}