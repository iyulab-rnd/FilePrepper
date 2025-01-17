using CommandLine;
using FilePrepper.Tasks.DataTypeConvert;
using Microsoft.Extensions.Logging;

namespace FilePrepper.CLI.Tools.DataTypeConvert;

[Verb("convert-type", HelpText = "Convert data types of columns")]
public class DataTypeConvertParameters : SingleInputParameters
{
    [Option('c', "conversions", Required = true, Separator = ',',
        HelpText = "Type conversions in format column:type[:format] (e.g. Date:DateTime:yyyy-MM-dd or Age:Integer)")]
    public IEnumerable<string> Conversions { get; set; } = Array.Empty<string>();

    [Option("culture", Default = "en-US",
        HelpText = "Culture to use for parsing (e.g. en-US, ko-KR)")]
    public string Culture { get; set; } = "en-US";

    public override Type GetHandlerType() => typeof(DataTypeConvertHandler);

    protected override bool ValidateInternal(ILogger logger)
    {
        if (!base.ValidateInternal(logger))
            return false;

        if (!Conversions.Any())
        {
            logger.LogError("At least one conversion must be specified");
            return false;
        }

        foreach (var conv in Conversions)
        {
            var parts = conv.Split(':');
            if (parts.Length < 2 || parts.Length > 3)
            {
                logger.LogError("Invalid conversion format: {Conversion}. Expected format: column:type[:format]", conv);
                return false;
            }

            if (!Enum.TryParse<DataType>(parts[1], true, out var dataType))
            {
                logger.LogError("Invalid data type: {Type}. Valid values are: {ValidValues}",
                    parts[1], string.Join(", ", Enum.GetNames<DataType>()));
                return false;
            }

            if (dataType == DataType.DateTime && parts.Length != 3)
            {
                logger.LogError("DateTime format must be specified for DateTime type: {Conversion}", conv);
                return false;
            }
        }

        try
        {
            _ = System.Globalization.CultureInfo.GetCultureInfo(Culture);
        }
        catch (System.Globalization.CultureNotFoundException)
        {
            logger.LogError("Invalid culture: {Culture}", Culture);
            return false;
        }

        return true;
    }
}