namespace FilePrepper.Tasks.DropDuplicates;


public class DropDuplicatesTask : BaseTask<DropDuplicatesOption>
{
    public DropDuplicatesTask(
        DropDuplicatesOption options,
        ILogger<DropDuplicatesTask> logger,
        ILogger<DropDuplicatesValidator> validatorLogger)
        : base(options, logger, new DropDuplicatesValidator(validatorLogger))
    {
        if (options.Common == null)
        {
            options.Common = new();
        }
    }

    protected override Task<List<Dictionary<string, string>>> ProcessRecordsAsync(
        List<Dictionary<string, string>> records)
    {
        if (records.Count == 0)
        {
            return Task.FromResult(records);
        }

        // 중복 체크할 컬럼들 결정
        var columnsToCheck = GetColumnsToCheck(records[0].Keys.ToList());
        _logger.LogInformation("Checking duplicates using columns: {Columns}",
            string.Join(", ", columnsToCheck));

        var processedRecords = new List<Dictionary<string, string>>();
        var seen = new HashSet<string>();

        var recordsToProcess = Options.KeepFirst ? records : records.AsEnumerable().Reverse();

        foreach (var record in recordsToProcess)
        {
            // 체크할 컬럼들의 값을 조합하여 해시키 생성
            var key = string.Join("|", columnsToCheck.Select(col => record[col]));

            if (!seen.Contains(key))
            {
                seen.Add(key);
                processedRecords.Add(record);
            }
            else
            {
                _logger.LogDebug("Dropping duplicate record with key: {Key}", key);
            }
        }

        // 원래 순서 유지
        if (!Options.KeepFirst)
        {
            processedRecords.Reverse();
        }

        _logger.LogInformation("Removed {Count} duplicate records",
            records.Count - processedRecords.Count);

        return Task.FromResult(processedRecords);
    }

    private IEnumerable<string> GetColumnsToCheck(List<string> allColumns)
    {
        if (!Options.SubsetColumnsOnly)
        {
            return allColumns;
        }

        return Options.TargetColumns;
    }

    protected override IEnumerable<string> GetRequiredColumns()
    {
        return Options.SubsetColumnsOnly ? Options.TargetColumns : Array.Empty<string>();
    }
}
