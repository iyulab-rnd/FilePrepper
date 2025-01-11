﻿using CommandLine;
using FilePrepper.CLI.Handlers;

namespace FilePrepper.CLI.Parameters;

[Verb("rename-columns", HelpText = "Rename columns in the input file")]
public class RenameColumnsParameters : BaseParameters
{
    [Option('m', "mappings", Required = true, Separator = ',',
        HelpText = "Column rename mappings in format oldName:newName (e.g. OldCol:NewCol)")]
    public IEnumerable<string> Mappings { get; set; } = Array.Empty<string>();

    public override Type GetHandlerType() => typeof(RenameColumnsHandler);
}