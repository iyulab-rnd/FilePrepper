using CommandLine;
using FilePrepper.CLI.Handlers;

namespace FilePrepper.CLI.Parameters;

[Verb("convert-type", HelpText = "Convert data types of columns")]
public class DataTypeConvertParameters : BaseParameters
{
    [Option('c', "conversions", Required = true, Separator = ',',
        HelpText = "Type conversions in format column:type[:format] (e.g. Date:DateTime:yyyy-MM-dd or Age:Integer)")]
    public IEnumerable<string> Conversions { get; set; } = Array.Empty<string>();

    [Option("culture", Default = "en-US",
        HelpText = "Culture to use for parsing (e.g. en-US, ko-KR)")]
    public string Culture { get; set; } = "en-US";

    public override Type GetHandlerType() => typeof(DataTypeConvertHandler);
}
