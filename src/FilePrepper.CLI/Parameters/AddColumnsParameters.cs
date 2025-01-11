using CommandLine;
using FilePrepper.CLI.Handlers;

namespace FilePrepper.CLI.Parameters;

[Verb("add-columns", HelpText = "Add new columns to the CSV file")]
public class AddColumnsParameters : BaseParameters
{
    [Option('c', "columns", Required = true, Separator = ',',
        HelpText = "Columns to add in format name=value,name2=value2")]
    public IEnumerable<string> Columns { get; set; } = Array.Empty<string>();

    public override Type GetHandlerType() => typeof(AddColumnsHandler);
}
