using CommandLine;
using FilePrepper.CLI.Handlers;

namespace FilePrepper.CLI.Parameters;

[Verb("column-interaction", HelpText = "Perform operations between columns")]
public class ColumnInteractionParameters : BaseParameters
{
    [Option('s', "source", Required = true, Separator = ',',
        HelpText = "Source columns to use in operation")]
    public IEnumerable<string> SourceColumns { get; set; } = Array.Empty<string>();

    [Option('t', "type", Required = true,
        HelpText = "Operation type (Add/Subtract/Multiply/Divide/Concat/Custom)")]
    public string Operation { get; set; } = string.Empty;

    [Option('o', "output", Required = true,
        HelpText = "Output column name")]
    public string OutputColumn { get; set; } = string.Empty;

    [Option('e', "expression", Required = false,
        HelpText = "Custom expression when using Custom operation type. Use $1, $2, etc. to reference columns")]
    public string? CustomExpression { get; set; }

    public override Type GetHandlerType() => typeof(ColumnInteractionHandler);
}
