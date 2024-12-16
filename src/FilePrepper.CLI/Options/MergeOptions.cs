using CommandLine;

namespace FilePrepper.CLI.Options;

[Verb("merge", HelpText = "Merge two CSV files into a single CSV.")]
public class MergeOptions : CommonOptions
{
    [Option('i', "inputs", Required = true,
        HelpText = "Input files to merge, separated by a comma (e.g., file1.csv,file2.csv).")]
    public string InputFiles { get; set; } = string.Empty;
}
