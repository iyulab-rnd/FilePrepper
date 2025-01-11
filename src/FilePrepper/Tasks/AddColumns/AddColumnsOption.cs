namespace FilePrepper.Tasks.AddColumns;

public class AddColumnsOption : BaseOption
{
    public Dictionary<string, string> NewColumns { get; set; } = [];

    protected override string[] ValidateInternal()
    {
        var errors = new List<string>();

        if (NewColumns == null || NewColumns.Count == 0)
        {
            errors.Add("At least one new column must be specified");
            return [.. errors];
        }

        foreach (var columnName in NewColumns.Keys)
        {
            if (string.IsNullOrWhiteSpace(columnName))
            {
                errors.Add("Column name cannot be empty or whitespace");
            }
        }

        return [.. errors];
    }
}