using CommandLine;
using FilePrepper.Tasks.DropDuplicates;
using Microsoft.Extensions.Logging;

namespace FilePrepper.CLI.Tools.DropDuplicates;

[Verb("drop-duplicates", HelpText = "Remove duplicate rows")]
public class DropDuplicatesParameters : SingleInputParameters
{
    [Option("keep-first", Default = true,
        HelpText = "Keep first occurrence of duplicates instead of last")]
    public bool KeepFirst { get; set; } = true;

    [Option("subset-only", Default = false,
        HelpText = "Check duplicates only on specified columns")]
    public bool SubsetColumnsOnly { get; set; }

    [Option('c', "columns", Separator = ',',
        HelpText = "Columns to check for duplicates when using subset-only")]
    public IEnumerable<string> TargetColumns { get; set; } = Array.Empty<string>();

    public override Type GetHandlerType() => typeof(DropDuplicatesHandler);

    protected override bool ValidateInternal(ILogger logger)
    {
        if (!base.ValidateInternal(logger))
            return false;

        // 서브셋 모드일 때 컬럼 지정 필수
        if (SubsetColumnsOnly && !TargetColumns.Any())
        {
            logger.LogError("Target columns must be specified when using subset-only");
            return false;
        }

        // 지정된 컬럼들의 유효성 검사
        if (TargetColumns.Any())
        {
            foreach (var column in TargetColumns)
            {
                if (string.IsNullOrWhiteSpace(column))
                {
                    logger.LogError("Column name cannot be empty or whitespace");
                    return false;
                }
            }
        }

        return true;
    }

    public override string? GetExample() =>
    "drop-duplicates -i input.csv -o output.csv --subset-only -c \"Name,Department\" --keep-first";
}