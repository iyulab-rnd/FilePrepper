using CommandLine;

namespace FilePrepper.CLI.Options;

public abstract class CommonOptions
{
    [Option('o', "output", Required = false, HelpText = "Output path (directory or file path).")]
    public string? Output { get; set; }

    [Option('d', "delimiter", Required = false, Default = ",", HelpText = "CSV delimiter.")]
    public string Delimiter { get; set; } = ",";

    [Option("no-header", Required = false, HelpText = "Exclude headers in output CSV.")]
    public bool NoHeader { get; set; }

    [Option('e', "encoding", Required = false, Default = "utf-8",
        HelpText = "Output file encoding (utf-8, ascii, utf-16, utf-32).")]
    public string Encoding { get; set; } = "utf-8";

    [Option('v', "verbose", Required = false, HelpText = "Enable verbose logging.")]
    public bool Verbose { get; set; }
}
