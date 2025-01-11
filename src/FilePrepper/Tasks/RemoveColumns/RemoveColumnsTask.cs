﻿namespace FilePrepper.Tasks.RemoveColumns;

public class RemoveColumnsTask : BaseTask<RemoveColumnsOption>
{
    public RemoveColumnsTask(
        RemoveColumnsOption options,
        ILogger<RemoveColumnsTask> logger)
        : base(options, logger)
    {
    }

    protected override Task<List<Dictionary<string, string>>> ProcessRecordsAsync(
        List<Dictionary<string, string>> records)
    {
        _logger.LogInformation("Removing specified columns from records");

        // 레코드가 없는 경우에도 헤더에서 지정된 열을 제거
        if (records.Count == 0)
        {
            foreach (var col in Options.RemoveColumns)
            {
                _originalHeaders.Remove(col);
            }
            return Task.FromResult(records);
        }

        // 레코드가 있는 경우 각 레코드에서 열 제거
        foreach (var record in records)
        {
            foreach (var colToRemove in Options.RemoveColumns)
            {
                record.Remove(colToRemove);
            }
        }

        return Task.FromResult(records);
    }
}
