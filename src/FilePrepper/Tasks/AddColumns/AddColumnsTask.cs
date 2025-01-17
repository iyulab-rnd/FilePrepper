namespace FilePrepper.Tasks.AddColumns;

public class AddColumnsTask : BaseTask<AddColumnsOption>
{
    public AddColumnsTask(ILogger<AddColumnsTask> logger) : base(logger)
    {
    }

    protected override Task<List<Dictionary<string, string>>> ProcessRecordsAsync(
        List<Dictionary<string, string>> records)
    {
        // 헤더 검증
        var headers = records.FirstOrDefault()?.Keys.ToList() ?? new List<string>();
        var duplicateColumns = Options.NewColumns.Keys.Where(headers.Contains).ToList();
        var validColumns = Options.NewColumns
            .Where(kvp => !headers.Contains(kvp.Key))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        if (duplicateColumns.Any())
        {
            var warning = $"Duplicate column names found: {string.Join(", ", duplicateColumns)}";
            _logger.LogWarning(warning);

            if (!Options.IgnoreErrors)
            {
                throw new ValidationException(warning, ValidationExceptionErrorCode.General);
            }

            // IgnoreErrors가 true인 경우, 중복되지 않은 컬럼만 추가
            _logger.LogWarning("Skipping duplicate columns and continuing with valid columns");
        }

        // 새 컬럼 추가 (IgnoreErrors=true인 경우 중복되지 않은 컬럼만)
        _logger.LogInformation("Processing data and adding new columns");
        foreach (var record in records)
        {
            foreach (var newColumn in validColumns)
            {
                record[newColumn.Key] = newColumn.Value;
            }
        }

        return Task.FromResult(records);
    }
}