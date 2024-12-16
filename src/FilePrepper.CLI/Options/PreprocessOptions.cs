using CommandLine;

namespace FilePrepper.CLI.Options;

[Verb("preprocess", HelpText = "Preprocess CSV file for machine learning.")]
public class PreprocessOptions : CommonOptions
{
    [Value(0, Required = true, HelpText = "Input file to preprocess.")]
    public string InputFile { get; set; } = string.Empty;

    [Option("max-null", Required = false, Default = 0.5,
        HelpText = "Maximum allowed percentage of null values per column (0.0 to 1.0).")]
    public double MaxNullPercentage { get; set; } = 0.5;

    [Option("remove-null-rows", Required = false,
        HelpText = "Remove rows with missing values instead of imputing them.")]
    public bool RemoveNullRows { get; set; }

    [Option("remove-outliers", Required = false,
        HelpText = "Remove statistical outliers from numeric columns.")]
    public bool RemoveOutliers { get; set; }

    [Option("scale-features", Required = false,
        HelpText = "Apply feature scaling to numeric columns.")]
    public bool ScaleFeatures { get; set; }
}
