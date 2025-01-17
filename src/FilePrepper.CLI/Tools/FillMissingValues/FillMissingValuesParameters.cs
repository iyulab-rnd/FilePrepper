using CommandLine;
using FilePrepper.Tasks.FillMissingValues;
using Microsoft.Extensions.Logging;

namespace FilePrepper.CLI.Tools.FillMissingValues;

[Verb("fill-missing", HelpText = "Fill missing values in columns")]
public class FillMissingValuesParameters : SingleInputParameters, IAppendableParameters
{
    [Option('m', "methods", Required = true, Separator = ',',
        HelpText = "Fill methods in format column:method[:value] (e.g. Age:Mean or Score:FixedValue:0)")]
    public IEnumerable<string> FillMethods { get; set; } = Array.Empty<string>();

    public override Type GetHandlerType() => typeof(FillMissingValuesHandler);

    protected override bool ValidateInternal(ILogger logger)
    {
        if (!base.ValidateInternal(logger))
            return false;

        if (!FillMethods.Any())
        {
            logger.LogError("At least one fill method must be specified");
            return false;
        }

        foreach (var method in FillMethods)
        {
            var parts = method.Split(':');
            if (parts.Length < 2 || parts.Length > 3)
            {
                logger.LogError("Invalid fill method format: {Method}. Expected format: column:method[:value]", method);
                return false;
            }

            if (!Enum.TryParse<FillMethod>(parts[1], true, out var fillMethod))
            {
                logger.LogError("Invalid fill method: {Method}. Valid values are: {ValidValues}",
                    parts[1], string.Join(", ", Enum.GetNames<FillMethod>()));
                return false;
            }

            if (fillMethod == FillMethod.FixedValue && parts.Length != 3)
            {
                logger.LogError("Fixed value must be specified for FixedValue method: {Method}", method);
                return false;
            }
        }

        if (AppendToSource && string.IsNullOrWhiteSpace(OutputColumnTemplate))
        {
            logger.LogError("Output column template is required when appending to source");
            return false;
        }

        return true;
    }
}