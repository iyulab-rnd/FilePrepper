using CommandLine;
using FilePrepper.Tasks.ScaleData;
using Microsoft.Extensions.Logging;

namespace FilePrepper.CLI.Tools.ScaleData;

public class ScaleDataParameters : SingleInputParameters
{
    [Option('s', "scaling", Required = true, Separator = ',',
        HelpText = "Scaling methods in format column:method (e.g. Price:MinMax,Score:Standardization)")]
    public IEnumerable<string> ScaleColumns { get; set; } = Array.Empty<string>();

    public override Type GetHandlerType() => typeof(ScaleDataHandler);

    protected override bool ValidateInternal(ILogger logger)
    {
        if (!base.ValidateInternal(logger))
            return false;

        if (!ScaleColumns.Any())
        {
            logger.LogError("At least one scaling method must be specified");
            return false;
        }

        foreach (var scaleStr in ScaleColumns)
        {
            var parts = scaleStr.Split(':');
            if (parts.Length != 2)
            {
                logger.LogError("Invalid scaling format: {Scale}. Expected format: column:method", scaleStr);
                return false;
            }

            if (!Enum.TryParse<ScaleMethod>(parts[1], true, out _))
            {
                logger.LogError("Invalid scale method: {Method}. Valid values are: {ValidValues}",
                    parts[1], string.Join(", ", Enum.GetNames<ScaleMethod>()));
                return false;
            }
        }

        return true;
    }

    public override string? GetExample() =>
        "scale -i input.csv -o output.csv -s \"Price:MinMax,Score:Standardization\"";
}