namespace FilePrepper.Tasks.ScaleData;

public enum ScaleMethod
{
    MinMax,
    Standardization
}

public class ScaleColumnOption
{
    public string ColumnName { get; set; } = string.Empty;
    public ScaleMethod Method { get; set; }
}

public class ScaleDataOption : BaseColumnOption
{
    public List<ScaleColumnOption> ScaleColumns { get; set; } = new();

    protected override string[] ValidateInternal()
    {
        var errors = new List<string>();

        if (ScaleColumns.Count == 0)
        {
            errors.Add("At least one column must be specified for scaling.");
        }

        foreach (var col in ScaleColumns)
        {
            if (string.IsNullOrWhiteSpace(col.ColumnName))
            {
                errors.Add("Column name cannot be empty or whitespace.");
            }
        }

        return [.. errors];
    }
}