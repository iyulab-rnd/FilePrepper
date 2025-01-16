using CommandLine;
using FilePrepper.CLI.Tools;

namespace FilePrepper.CLI.Tools.FileFormatConvert;

[Verb("convert-format", HelpText = "Convert file format")]
public class FileFormatConvertParameters : SingleInputParameters
{
    [Option('t', "target", Required = true,
        HelpText = "Target format (CSV/TSV/PSV/JSON/XML)")]
    public string TargetFormat { get; set; } = string.Empty;

    [Option('e', "encoding", Default = "utf-8",
        HelpText = "File encoding")]
    public string Encoding { get; set; } = "utf-8";

    [Option("pretty", Default = false,
        HelpText = "Enable pretty printing for JSON/XML")]
    public bool PrettyPrint { get; set; }

    [Option("root", Default = "root",
        HelpText = "Root element name for XML format")]
    public string RootElementName { get; set; } = "root";

    [Option("item", Default = "item",
        HelpText = "Item element name for XML format")]
    public string ItemElementName { get; set; } = "item";

    public override Type GetHandlerType() => typeof(FileFormatConvertHandler);
}
