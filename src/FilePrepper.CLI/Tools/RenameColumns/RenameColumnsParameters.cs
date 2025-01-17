using CommandLine;
using FilePrepper.Tasks.RenameColumns;
using Microsoft.Extensions.Logging;

namespace FilePrepper.CLI.Tools.RenameColumns;

[Verb("rename-columns", HelpText = "Rename columns in the input file")]
public class RenameColumnsParameters : SingleInputParameters
{
    [Option('m', "mappings", Required = true, Separator = ',',
        HelpText = "Column rename mappings in format oldName:newName (e.g. OldCol:NewCol)")]
    public IEnumerable<string> Mappings { get; set; } = Array.Empty<string>();

    public override Type GetHandlerType() => typeof(RenameColumnsHandler);

    protected override bool ValidateInternal(ILogger logger)
    {
        if (!base.ValidateInternal(logger))
            return false;

        if (!Mappings.Any())
        {
            logger.LogError("At least one column mapping must be specified");
            return false;
        }

        var oldNames = new HashSet<string>();
        var newNames = new HashSet<string>();

        foreach (var mapping in Mappings)
        {
            var parts = mapping.Split(':');
            if (parts.Length != 2)
            {
                logger.LogError("Invalid mapping format: {Mapping}. Expected format: oldName:newName", mapping);
                return false;
            }

            var oldName = parts[0].Trim();
            var newName = parts[1].Trim();

            if (string.IsNullOrWhiteSpace(oldName) || string.IsNullOrWhiteSpace(newName))
            {
                logger.LogError("Column names cannot be empty in mapping: {Mapping}", mapping);
                return false;
            }

            if (!oldNames.Add(oldName))
            {
                logger.LogError("Duplicate source column name: {Column}", oldName);
                return false;
            }

            if (!newNames.Add(newName))
            {
                logger.LogError("Duplicate target column name: {Column}", newName);
                return false;
            }
        }

        return true;
    }
}