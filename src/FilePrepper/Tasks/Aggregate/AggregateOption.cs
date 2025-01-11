﻿using FilePrepper.Utils;
using Microsoft.Extensions.Logging;

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

public class AggregateOption : BaseOption
{
    public string[] GroupByColumns { get; set; } = Array.Empty<string>();
    public List<AggregateColumn> AggregateColumns { get; set; } = new();

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
                if (string.IsNullOrWhiteSpace(col.OutputColumnName) && !Common.AppendToSource)
                {
                    errors.Add("Output column name cannot be empty when not appending to source");
                }
            }
        }

        if (string.IsNullOrWhiteSpace(Common.OutputColumnTemplate) && Common.AppendToSource)
        {
            errors.Add("Column template is required when appending to source");
        }

        return errors.ToArray();
    }
}