﻿using CommandLine;
using FilePrepper.CLI.Tools;

namespace FilePrepper.CLI.Tools.ReorderColumns;

[Verb("reorder-columns", HelpText = "Reorder columns in the CSV file")]
public class ReorderColumnsParameters : SingleInputParameters
{
    [Option('o', "order", Required = true, Separator = ',',
        HelpText = "Desired column order (comma-separated)")]
    public IEnumerable<string> Order { get; set; } = Array.Empty<string>();

    public override Type GetHandlerType() => typeof(ReorderColumnsHandler);
}