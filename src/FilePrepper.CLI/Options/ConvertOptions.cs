using CommandLine;

namespace FilePrepper.CLI.Options;

[Verb("convert", HelpText = "Convert a single file to CSV.")]
public class ConvertOptions : CommonOptions
{
    [Value(0, Required = true, HelpText = "Input file to convert.")]
    public string InputFile { get; set; } = string.Empty;
}
