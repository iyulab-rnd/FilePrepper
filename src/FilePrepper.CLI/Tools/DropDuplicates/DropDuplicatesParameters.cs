using CommandLine;
using FilePrepper.CLI.Tools;

namespace FilePrepper.CLI.Tools.DropDuplicates;

[Verb("drop-duplicates", HelpText = "Remove duplicate rows")]
public class DropDuplicatesParameters : SingleInputParameters
{
    [Option("keep-first", Default = true,
        HelpText = "Keep first occurrence of duplicates instead of last")]
    public bool KeepFirst { get; set; }

    [Option("subset-only", Default = false,
        HelpText = "Check duplicates only on specified columns")]
    public bool SubsetColumnsOnly { get; set; }

    [Option('c', "columns", Separator = ',',
        HelpText = "Columns to check for duplicates when using subset-only")]
    public IEnumerable<string> TargetColumns { get; set; } = Array.Empty<string>();

    public override Type GetHandlerType() => typeof(DropDuplicatesHandler);
}