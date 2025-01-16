using CommandLine;
using FilePrepper.CLI.Tools;

namespace FilePrepper.CLI.Tools.DataSampling;

[Verb("data-sampling", HelpText = "Sample data from the input file")]
public class DataSamplingParameters : SingleInputParameters
{
    [Option('m', "method", Required = true,
        HelpText = "Sampling method (Random/Systematic/Stratified)")]
    public string Method { get; set; } = "Random";

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
}