using CommandLine;
using FilePrepper.CLI.Tools;

namespace FilePrepper.CLI.Tools.OneHotEncoding;

[Verb("one-hot-encoding", HelpText = "Perform one-hot encoding on categorical columns")]
public class OneHotEncodingParameters : SingleInputParameters
{
    [Option('c', "columns", Required = true, Separator = ',',
        HelpText = "Columns to encode")]
    public IEnumerable<string> Columns { get; set; } = Array.Empty<string>();

    [Option("drop-first", Default = false,
        HelpText = "Drop first category to avoid dummy variable trap")]
    public bool DropFirst { get; set; }

    [Option("keep-original", Default = false,
        HelpText = "Keep original columns alongside encoded ones")]
    public bool KeepOriginalColumns { get; set; }

    public override Type GetHandlerType() => typeof(OneHotEncodingHandler);
}