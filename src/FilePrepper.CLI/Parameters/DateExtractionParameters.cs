using CommandLine;
using FilePrepper.CLI.Handlers;

namespace FilePrepper.CLI.Parameters;

[Verb("extract-date", HelpText = "Extract components from date columns")]
public class DateExtractionParameters : SingleInputParameters
{
    [Option('e', "extractions", Required = true, Separator = ',',
        HelpText = "Date extractions in format column:component1,component2[:format] (e.g. Date:Year,Month,Day:yyyy-MM-dd)")]
    public IEnumerable<string> Extractions { get; set; } = Array.Empty<string>();

    [Option("culture", Default = "en-US",
        HelpText = "Culture to use for parsing dates (e.g. en-US, ko-KR)")]
    public string Culture { get; set; } = "en-US";

    [Option("template", Default = "{column}_{component}",
        HelpText = "Template for output column names")]
    public string OutputColumnTemplate { get; set; } = "{column}_{component}";

    public override Type GetHandlerType() => typeof(DateExtractionHandler);
}
