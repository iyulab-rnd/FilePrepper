using CommandLine;
using FilePrepper.Tasks.DataSampling;
using Microsoft.Extensions.Logging;

namespace FilePrepper.CLI.Tools.DataSampling;

[Verb("data-sampling", HelpText = "Sample data from the input file")]
public class DataSamplingParameters : SingleInputParameters
{
    [Option('m', "method", Required = true,
        HelpText = "Sampling method (Random/Systematic/Stratified)")]
    public string Method { get; set; } = string.Empty;

    [Option('s', "size", Required = true,
        HelpText = "Sample size (absolute number if > 1, ratio if between 0 and 1)")]
    public double SampleSize { get; set; }

    [Option("seed", Required = false,
        HelpText = "Random seed for reproducibility")]
    public int? Seed { get; set; }

    [Option("stratify", Required = false,
        HelpText = "Column to use for stratified sampling")]
    public string? StratifyColumn { get; set; }

    [Option("interval", Required = false,
        HelpText = "Interval for systematic sampling")]
    public int? SystematicInterval { get; set; }

    public override Type GetHandlerType() => typeof(DataSamplingHandler);

    protected override bool ValidateInternal(ILogger logger)
    {
        if (!base.ValidateInternal(logger))
            return false;

        if (!Enum.TryParse<SamplingMethod>(Method, true, out var method))
        {
            logger.LogError("Invalid sampling method: {Method}. Valid values are: {ValidValues}",
                Method, string.Join(", ", Enum.GetNames<SamplingMethod>()));
            return false;
        }

        if (SampleSize <= 0)
        {
            logger.LogError("Sample size must be greater than 0");
            return false;
        }

        if (method == SamplingMethod.Stratified && string.IsNullOrWhiteSpace(StratifyColumn))
        {
            logger.LogError("Stratify column is required for stratified sampling");
            return false;
        }

        if (method == SamplingMethod.Systematic && (!SystematicInterval.HasValue || SystematicInterval.Value <= 0))
        {
            logger.LogError("Valid systematic interval is required for systematic sampling");
            return false;
        }

        return true;
    }

    public override string? GetExample() =>
    "data-sampling -i input.csv -o output.csv -m Random -s 0.3 --seed 42";
}