using CommandLine;
using FilePrepper.CLI.Tools.ColumnInteraction;
using FilePrepper.CLI.Tools;
using FilePrepper.Tasks.ColumnInteraction;

using Microsoft.Extensions.Logging;

/// <summary>
/// column-interaction 명령어의 매개변수를 정의하는 클래스
/// </summary>
[Verb("column-interaction", HelpText = "Perform operations between columns")]
public class ColumnInteractionParameters : SingleInputParameters, IDefaultValueParameters
{
    [Option('s', "source", Required = true, Separator = ',',
        HelpText = "Source columns to use in operation (e.g., Price,Quantity)")]
    public IEnumerable<string> SourceColumns { get; set; } = Array.Empty<string>();

    [Option('t', "type", Required = true,
        HelpText = "Operation type (Add/Subtract/Multiply/Divide/Concat/Custom)")]
    public string Operation { get; set; } = string.Empty;

    [Option('c', "column", Required = true,
        HelpText = "Name of the column to store operation result")]
    public string OutputColumn { get; set; } = string.Empty;

    [Option('e', "expression", Required = false,
        HelpText = "Custom expression when using Custom operation type. Use $1, $2, etc. to reference columns in order")]
    public string? CustomExpression { get; set; }

    public override Type GetHandlerType() => typeof(ColumnInteractionHandler);

    protected override bool ValidateInternal(ILogger logger)
    {
        if (!base.ValidateInternal(logger))
        {
            return false;
        }

        // Validate source columns
        if (SourceColumns.Count() < 2)
        {
            logger.LogError("At least two source columns must be specified");
            return false;
        }

        foreach (var col in SourceColumns)
        {
            if (string.IsNullOrWhiteSpace(col))
            {
                logger.LogError("Source column name cannot be empty");
                return false;
            }
        }

        // Validate operation type
        if (!Enum.TryParse<OperationType>(Operation, true, out var operationType))
        {
            logger.LogError("Invalid operation type: {Operation}. Valid values are: {ValidValues}",
                Operation, string.Join(", ", Enum.GetNames<OperationType>()));
            return false;
        }

        // Validate output column
        if (string.IsNullOrWhiteSpace(OutputColumn))
        {
            logger.LogError("Output column name cannot be empty");
            return false;
        }

        // Validate custom expression
        if (operationType == OperationType.Custom && string.IsNullOrWhiteSpace(CustomExpression))
        {
            logger.LogError("Custom expression is required when using Custom operation type");
            return false;
        }

        return true;
    }

    public override string? GetExample() =>
    "column-interaction -i sales.csv -o output.csv -s \"Price,Quantity\" -t Multiply -c TotalAmount";
}