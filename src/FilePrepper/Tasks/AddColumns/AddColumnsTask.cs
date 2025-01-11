using Microsoft.Extensions.Logging;

namespace FilePrepper.Tasks.AddColumns;

public class AddColumnsTask : BaseTask<AddColumnsOption>
{
    public AddColumnsTask(
        AddColumnsOption options,
        ILogger<AddColumnsTask> logger,
        ILogger<AddColumnsValidator> validatorLogger)
        : base(options, logger, new AddColumnsValidator(validatorLogger))
    {
    }

    protected override async Task<List<Dictionary<string, string>>> ProcessRecordsAsync(
        List<Dictionary<string, string>> records)
    {
        // 헤더 검증
        var headers = records.FirstOrDefault()?.Keys.ToList() ?? new List<string>();
        foreach (var newColumn in Options.NewColumns)
        {
            if (headers.Contains(newColumn.Key))
            {
                _logger.LogError("Duplicate column name found: {ColumnName}", newColumn.Key);
                throw new InvalidOperationException($"Duplicate column name found: {newColumn.Key}");
            }
        }

        // 새 컬럼 추가
        _logger.LogInformation("Processing data and adding new columns");
        foreach (var record in records)
        {
            foreach (var newColumn in Options.NewColumns)
            {
                record[newColumn.Key] = newColumn.Value;
            }
        }

        return records;
    }
}
