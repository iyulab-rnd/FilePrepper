using CommandLine;
using FilePrepper.CLI.Handlers;

namespace FilePrepper.CLI.Parameters;

[Verb("merge", HelpText = "Merge multiple CSV files")]
public class MergeParameters : MultipleInputParameters
{
    [Value(0, Required = true, Min = 2, MetaName = "inputs",
        HelpText = "Input CSV files to merge (minimum 2 files required)")]
    public IEnumerable<string> InputFiles { get; set; } = Array.Empty<string>();

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