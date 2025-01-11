namespace FilePrepper.Tasks.RenameColumns;

public class RenameColumnsOption : BaseOption
{
    /// <summary>
    /// Dictionary mapping original column names to new column names.
    /// </summary>
    public Dictionary<string, string> RenameMap { get; set; } = new();

    protected override string[] ValidateInternal()
    {
        var errors = new List<string>();

        if (RenameMap == null || RenameMap.Count == 0)
        {
            errors.Add("At least one column rename mapping must be specified.");
            return errors.ToArray();
        }

        foreach (var kv in RenameMap)
        {
            if (string.IsNullOrWhiteSpace(kv.Key))
            {
                errors.Add("Original column name cannot be empty or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(kv.Value))
            {
                errors.Add("New column name cannot be empty or whitespace.");
            }
        }

        return errors.ToArray();
    }
}
