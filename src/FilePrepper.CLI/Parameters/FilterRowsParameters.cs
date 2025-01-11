using CommandLine;
using FilePrepper.CLI.Handlers;

namespace FilePrepper.CLI.Parameters;

[Verb("filter-rows", HelpText = "Filter rows based on conditions")]
public class FilterRowsParameters : BaseParameters
{
    [Option('c', "conditions", Required = true, Separator = ',',
        HelpText = "Filter conditions in format column:operator:value (e.g. Age:GreaterThan:30)")]
    public IEnumerable<string> Conditions { get; set; } = Array.Empty<string>();

    public override Type GetHandlerType() => typeof(FilterRowsHandler);
}
