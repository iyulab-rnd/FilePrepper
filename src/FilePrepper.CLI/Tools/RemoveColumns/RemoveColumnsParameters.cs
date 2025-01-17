using CommandLine;
using FilePrepper.Tasks.RemoveColumns;
using Microsoft.Extensions.Logging;

namespace FilePrepper.CLI.Tools.RemoveColumns;

[Verb("remove-columns", HelpText = "Remove specified columns from the input file")]
public class RemoveColumnsParameters : SingleInputParameters
{
    [Option('c', "columns", Required = true, Separator = ',',
        HelpText = "Columns to remove from the input file")]
    public IEnumerable<string> Columns { get; set; } = Array.Empty<string>();

    public override Type GetHandlerType() => typeof(RemoveColumnsHandler);

    protected override bool ValidateInternal(ILogger logger)
    {
        if (!base.ValidateInternal(logger))
            return false;

        if (!Columns.Any())
        {
            logger.LogError("At least one column must be specified to remove");
            return false;
        }

        foreach (var column in Columns)
        {
            if (string.IsNullOrWhiteSpace(column))
            {
                logger.LogError("Column name cannot be empty or whitespace");
                return false;
            }
        }

        // 중복된 컬럼 이름 검사
        var duplicates = Columns.GroupBy(x => x)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicates.Any())
        {
            logger.LogError("Duplicate column names found: {Columns}", string.Join(", ", duplicates));
            return false;
        }

        return true;
    }
}