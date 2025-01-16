using CommandLine;
using FilePrepper.CLI.Tools;

namespace FilePrepper.CLI.Tools.Merge;

[Verb("merge", HelpText = "Merge multiple CSV files")]
public class MergeParameters : MultipleInputParameters
{
    [Option('t', "type", Required = true,
        HelpText = "Merge type (Vertical/Horizontal)")]
    public string MergeType { get; set; } = string.Empty;

    [Option('j', "join-type", Default = "Inner",
        HelpText = "Join type for horizontal merge (Inner/Left/Right/Full)")]
    public string JoinType { get; set; } = "Inner";

    [Option('k', "key-columns", Separator = ',',
        HelpText = "Key columns for horizontal merge")]
    public IEnumerable<string> JoinKeyColumns { get; set; } = Array.Empty<string>();

    public override Type GetHandlerType() => typeof(MergeHandler);
}