using CommandLine;
using FilePrepper.Tasks.NormalizeData;
using Microsoft.Extensions.Logging;

namespace FilePrepper.CLI.Tools.NormalizeData;

[Verb("normalize", HelpText = "Normalize numeric columns")]
public class NormalizeDataParameters : BaseColumnParameters
{
    [Option('m', "method", Required = true,
        HelpText = "Normalization method (MinMax/ZScore)")]
    public string Method { get; set; } = string.Empty;

    [Option("min", Default = 0.0,
        HelpText = "Min value for MinMax scaling")]
    public double MinValue { get; set; }

    [Option("max", Default = 1.0,
        HelpText = "Max value for MinMax scaling")]
    public double MaxValue { get; set; }

    public override Type GetHandlerType() => typeof(NormalizeDataHandler);

    protected override bool ValidateInternal(ILogger logger)
    {
        if (!base.ValidateInternal(logger))
            return false;

        if (!Enum.TryParse<NormalizationMethod>(Method, true, out var method))
        {
            logger.LogError("Invalid normalization method: {Method}. Valid values are: {ValidValues}",
                Method, string.Join(", ", Enum.GetNames<NormalizationMethod>()));
            return false;
        }

        if (method == NormalizationMethod.MinMax)
        {
            if (MinValue >= MaxValue)
            {
                logger.LogError("Min value must be less than max value for MinMax normalization");
                return false;
            }
        }

        return true;
    }

    public override string? GetExample() =>
        "normalize -i input.csv -o output.csv -c \"Price,Quantity\" -m MinMax --min 0 --max 1";
}