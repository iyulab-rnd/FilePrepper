using CommandLine;
using FilePrepper.Tasks.DateExtraction;
using Microsoft.Extensions.Logging;

namespace FilePrepper.CLI.Tools.DateExtraction;

[Verb("extract-date", HelpText = "Extract components from date columns")]
public class DateExtractionParameters : SingleInputParameters, IAppendableParameters
{
    [Option('e', "extractions", Required = true, Separator = ',',
        HelpText = "Date extractions in format column:component1,component2[:format] (e.g. Date:Year,Month,Day:yyyy-MM-dd)")]
    public IEnumerable<string> Extractions { get; set; } = Array.Empty<string>();

    [Option("culture", Default = "en-US",
        HelpText = "Culture to use for parsing dates (e.g. en-US, ko-KR)")]
    public string Culture { get; set; } = "en-US";

    public override Type GetHandlerType() => typeof(DateExtractionHandler);

    protected override bool ValidateInternal(ILogger logger)
    {
        if (!base.ValidateInternal(logger))
            return false;

        if (!Extractions.Any())
        {
            logger.LogError("At least one extraction must be specified");
            return false;
        }

        foreach (var extract in Extractions)
        {
            var parts = extract.Split(':');
            if (parts.Length < 2 || parts.Length > 3)
            {
                logger.LogError("Invalid extraction format: {Extraction}. Expected format: column:component1,component2[:format]", extract);
                return false;
            }

            var components = parts[1].Split(',');
            foreach (var comp in components)
            {
                if (!Enum.TryParse<DateComponent>(comp, true, out _))
                {
                    logger.LogError("Invalid date component: {Component}. Valid values are: {ValidValues}",
                        comp, string.Join(", ", Enum.GetNames<DateComponent>()));
                    return false;
                }
            }
        }

        // Culture 유효성 검사
        try
        {
            _ = System.Globalization.CultureInfo.GetCultureInfo(Culture);
        }
        catch (System.Globalization.CultureNotFoundException)
        {
            logger.LogError("Invalid culture: {Culture}", Culture);
            return false;
        }

        // AppendToSource가 true일 때 OutputColumnTemplate 필수
        if (AppendToSource && string.IsNullOrWhiteSpace(OutputColumnTemplate))
        {
            logger.LogError("Output column template is required when appending to source");
            return false;
        }

        return true;
    }
}