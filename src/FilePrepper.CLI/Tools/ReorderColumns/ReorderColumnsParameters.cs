using CommandLine;
using Microsoft.Extensions.Logging;

namespace FilePrepper.CLI.Tools.ReorderColumns;

public class ReorderColumnsParameters : SingleInputParameters
{
    [Option('o', "order", Required = true, Separator = ',',
        HelpText = "Desired column order (comma-separated)")]
    public IEnumerable<string> Order { get; set; } = Array.Empty<string>();

    public override Type GetHandlerType() => typeof(ReorderColumnsHandler);

    protected override bool ValidateInternal(ILogger logger)
    {
        if (!base.ValidateInternal(logger))
            return false;

        if (!Order.Any())
        {
            logger.LogError("At least one column must be specified in order");
            return false;
        }

        foreach (var col in Order)
        {
            if (string.IsNullOrWhiteSpace(col))
            {
                logger.LogError("Column name in order cannot be empty or whitespace");
                return false;
            }
        }

        return true;
    }
}