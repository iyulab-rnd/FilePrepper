using CommandLine;
using FilePrepper.CLI.Tools;

namespace FilePrepper.CLI.Tools.ColumnInteraction;

[Verb("column-interaction", HelpText = "Perform operations between columns")]
public class ColumnInteractionParameters : SingleInputParameters
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
}