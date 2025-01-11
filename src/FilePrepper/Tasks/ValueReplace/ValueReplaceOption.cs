namespace FilePrepper.Tasks.ValueReplace;

public class ColumnReplaceMethod
{
    public string ColumnName { get; set; } = string.Empty;
    public Dictionary<string, string> Replacements { get; set; } = new();
}

public class ValueReplaceOption : BaseColumnOption
{
    public List<ColumnReplaceMethod> ReplaceMethods { get; set; } = new();

    protected override string[] ValidateInternal()
    {
        var errors = new List<string>();

        if (ReplaceMethods.Count == 0)
        {
            errors.Add("At least one replace method must be specified.");
        }

        foreach (var method in ReplaceMethods)
        {
            if (string.IsNullOrWhiteSpace(method.ColumnName))
            {
                errors.Add("Column name cannot be empty or whitespace.");
            }

            if (method.Replacements == null || method.Replacements.Count == 0)
            {
                errors.Add($"Replacements must be specified for column {method.ColumnName}.");
            }
        }

        return [.. errors];
    }
}
