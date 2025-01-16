using CommandLine;
using FilePrepper.CLI.Tools;

namespace FilePrepper.CLI.Tools.RenameColumns;

[Verb("rename-columns", HelpText = "Rename columns in the input file")]
public class RenameColumnsParameters : SingleInputParameters
{
    [Option('m', "mappings", Required = true, Separator = ',',
        HelpText = "Column rename mappings in format oldName:newName (e.g. OldCol:NewCol)")]
    public IEnumerable<string> Mappings { get; set; } = Array.Empty<string>();

    public override Type GetHandlerType() => typeof(RenameColumnsHandler);
}