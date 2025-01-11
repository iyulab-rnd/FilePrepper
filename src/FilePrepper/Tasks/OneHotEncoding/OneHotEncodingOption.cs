namespace FilePrepper.Tasks.OneHotEncoding;

/// <summary>
/// Configuration for One-Hot Encoding task
/// </summary>
public class OneHotEncodingOption : BaseColumnOption
{
    /// <summary>
    /// If true, remove the first category to avoid dummy variable trap
    /// </summary>
    public bool DropFirst { get; set; } = false;

    /// <summary>
    /// If true, retain the original column (e.g., Category) alongside the one-hot columns
    /// </summary>
    public bool KeepOriginalColumns { get; set; } = false;

    protected override string[] ValidateInternal()
    {
        var errors = new List<string>();

        if (TargetColumns == null || TargetColumns.Length == 0)
        {
            errors.Add("At least one target column must be specified for one-hot encoding.");
        }

        return errors.ToArray();
    }
}
