namespace FilePrepper.Tasks.RemoveColumns;

public class RemoveColumnsOption : BaseOption
{
    /// <summary>
    /// List of columns to remove
    /// </summary>
    public List<string> RemoveColumns { get; set; } = new();

    protected override string[] ValidateInternal()
    {
        var errors = new List<string>();

        if (RemoveColumns == null || RemoveColumns.Count == 0)
        {
            errors.Add("At least one column must be specified to remove.");
            return errors.ToArray();
        }

        foreach (var columnName in RemoveColumns)
        {
            if (string.IsNullOrWhiteSpace(columnName))
            {
                errors.Add("Column name to remove cannot be empty or whitespace.");
            }
        }

        return errors.ToArray();
    }
}
