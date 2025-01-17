namespace FilePrepper.Tasks.Aggregate;

public enum AggregateFunction
{
    Sum,
    Average,
    Count,
    Min,
    Max
}

public class AggregateColumn
{
    public string ColumnName { get; set; } = string.Empty;
    public AggregateFunction Function { get; set; }
    public string OutputColumnName { get; set; } = string.Empty;
}

public class AggregateOption : SingleInputOption, IAppendableOption
{
    public string[] GroupByColumns { get; set; } = Array.Empty<string>();
    public List<AggregateColumn> AggregateColumns { get; set; } = new();

    // IAppendableOption implementation
    public bool AppendToSource { get; set; }
    public string? OutputColumnTemplate { get; set; }

    protected override string[] ValidateInternal()
    {
        var errors = new List<string>();

        if (GroupByColumns == null || GroupByColumns.Length == 0)
        {
            errors.Add("At least one group by column must be specified");
        }
        else if (GroupByColumns.Any(string.IsNullOrWhiteSpace))
        {
            errors.Add("Group by column name cannot be empty or whitespace");
        }

        if (AggregateColumns == null || AggregateColumns.Count == 0)
        {
            errors.Add("At least one aggregate column must be specified");
        }
        else
        {
            foreach (var col in AggregateColumns)
            {
                if (string.IsNullOrWhiteSpace(col.ColumnName))
                {
                    errors.Add("Aggregate column name cannot be empty");
                }
                if (string.IsNullOrWhiteSpace(col.OutputColumnName) && !AppendToSource)
                {
                    errors.Add("Output column name cannot be empty when not appending to source");
                }
            }
        }

        if (string.IsNullOrWhiteSpace(OutputColumnTemplate) && AppendToSource)
        {
            errors.Add("Column template is required when appending to source");
        }

        return [.. errors];
    }
}