using CommandLine;
using Microsoft.Extensions.Logging;

namespace FilePrepper.CLI.Tools.AddColumns;

/// <summary>
/// add-columns 명령어의 매개변수를 정의하는 클래스
/// </summary>
[Verb("add-columns", HelpText = "Add new columns to the CSV file with specified values")]
public class AddColumnsParameters : SingleInputParameters
{
    [Option('c', "columns", Required = true, Separator = ',',
        HelpText = "Columns to add in format name=value (e.g. Age=30,City=Seoul)")]
    public IEnumerable<string> Columns { get; set; } = Array.Empty<string>();

    public override Type GetHandlerType() => typeof(AddColumnsHandler);

    protected override bool ValidateInternal(ILogger logger)
    {
        if (!base.ValidateInternal(logger))
        {
            return false;
        }

        if (!Columns.Any())
        {
            logger.LogError("At least one column must be specified");
            return false;
        }

        // 각 컬럼 정의의 형식 검증
        foreach (var col in Columns)
        {
            var parts = col.Split('=', 2);
            if (parts.Length != 2)
            {
                logger.LogError("Invalid column format: {Column}. Expected format: name=value", col);
                return false;
            }

            if (string.IsNullOrWhiteSpace(parts[0]))
            {
                logger.LogError("Column name cannot be empty: {Column}", col);
                return false;
            }
        }

        // 중복 컬럼 이름 검사
        var columnNames = Columns
            .Select(c => c.Split('=', 2)[0].Trim())
            .ToList();

        var duplicates = columnNames
            .GroupBy(x => x)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicates.Any())
        {
            logger.LogError("Duplicate column names found: {Columns}",
                string.Join(", ", duplicates));
            return false;
        }

        return true;
    }
}