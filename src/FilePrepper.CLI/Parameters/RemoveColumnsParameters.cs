using CommandLine;
using FilePrepper.CLI.Handlers;

namespace FilePrepper.CLI.Parameters;

[Verb("remove-columns", HelpText = "Remove specified columns from the input file")]
public class RemoveColumnsParameters : SingleInputParameters
{
    [Option('c', "columns", Required = true, Separator = ',',
        HelpText = "Columns to remove from the input file")]
    public IEnumerable<string> Columns { get; set; } = Array.Empty<string>();

    public override Type GetHandlerType() => typeof(RemoveColumnsHandler);
}