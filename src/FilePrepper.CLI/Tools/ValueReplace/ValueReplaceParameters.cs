using CommandLine;
using Microsoft.Extensions.Logging;

namespace FilePrepper.CLI.Tools.ValueReplace;

public class ValueReplaceParameters : SingleInputParameters
{
    [Option('r', "replacements", Required = true, Separator = ',',
        HelpText = "Replacement rules in format column:oldValue=newValue[;oldValue2=newValue2] (e.g. Status:active=1;inactive=0)")]
    public IEnumerable<string> ReplaceMethods { get; set; } = Array.Empty<string>();

    public override Type GetHandlerType() => typeof(ValueReplaceHandler);

    protected override bool ValidateInternal(ILogger logger)
    {
        if (!base.ValidateInternal(logger))
            return false;

        if (!ReplaceMethods.Any())
        {
            logger.LogError("At least one replacement method must be specified");
            return false;
        }

        foreach (var replaceStr in ReplaceMethods)
        {
            var parts = replaceStr.Split(':', 2);
            if (parts.Length != 2)
            {
                logger.LogError("Invalid replacement format: {Replace}. Expected format: column:oldValue=newValue[;oldValue2=newValue2]", replaceStr);
                return false;
            }

            var replacementRules = parts[1].Split(';');
            foreach (var rule in replacementRules)
            {
                var valueParts = rule.Split('=', 2);
                if (valueParts.Length != 2)
                {
                    logger.LogError("Invalid replacement rule: {Rule}. Expected format: oldValue=newValue", rule);
                    return false;
                }
            }
        }

        return true;
    }

    public override string? GetExample() =>
        "replace -i input.csv -o output.csv -r \"Status:active=1;inactive=0,Gender:M=Male;F=Female\"";
}