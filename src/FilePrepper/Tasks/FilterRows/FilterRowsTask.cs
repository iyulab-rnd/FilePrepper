namespace FilePrepper.Tasks.FilterRows;

public class FilterRowsTask : BaseTask<FilterRowsOption>
{
    public FilterRowsTask(
        FilterRowsOption options,
        ILogger<FilterRowsTask> logger,
        ILogger<FilterRowsValidator> validatorLogger)
        : base(options, logger, new FilterRowsValidator(validatorLogger))
    {
    }

    protected override async Task<List<Dictionary<string, string>>> ProcessRecordsAsync(
        List<Dictionary<string, string>> records)
    {
        var filteredRecords = records
            .Where(record => MatchesAllConditions(record))
            .ToList();

        return await Task.FromResult(filteredRecords);
    }

    private bool MatchesAllConditions(Dictionary<string, string> record)
    {
        foreach (var cond in Options.Conditions)
        {
            // 컬럼이 존재하지 않으면 자동 불일치
            if (!record.ContainsKey(cond.ColumnName)) return false;
            var cellValue = record[cond.ColumnName];

            if (!MatchesCondition(cellValue, cond))
                return false;
        }
        return true;
    }

    private bool MatchesCondition(string cellValue, FilterCondition cond)
    {
        switch (cond.Operator)
        {
            case FilterOperator.Equals:
                return cellValue == cond.Value;
            case FilterOperator.NotEquals:
                return cellValue != cond.Value;
            case FilterOperator.GreaterThan:
                if (double.TryParse(cellValue, out var cVal1) && double.TryParse(cond.Value, out var cVal2))
                {
                    return cVal1 > cVal2;
                }
                return false;
            case FilterOperator.GreaterOrEqual:
                if (double.TryParse(cellValue, out var cVal3) && double.TryParse(cond.Value, out var cVal4))
                {
                    return cVal3 >= cVal4;
                }
                return false;
            case FilterOperator.LessThan:
                if (double.TryParse(cellValue, out var cVal5) && double.TryParse(cond.Value, out var cVal6))
                {
                    return cVal5 < cVal6;
                }
                return false;
            case FilterOperator.LessOrEqual:
                if (double.TryParse(cellValue, out var cVal7) && double.TryParse(cond.Value, out var cVal8))
                {
                    return cVal7 <= cVal8;
                }
                return false;
            case FilterOperator.Contains:
                return cellValue.Contains(cond.Value, StringComparison.OrdinalIgnoreCase);
            case FilterOperator.NotContains:
                return !cellValue.Contains(cond.Value);
            case FilterOperator.StartsWith:
                return cellValue.StartsWith(cond.Value);
            case FilterOperator.EndsWith:
                return cellValue.EndsWith(cond.Value);
            default:
                return false;
        }
    }

    protected override IEnumerable<string> GetRequiredColumns()
    {
        // 조건에 사용되는 컬럼과 TargetColumns를 합침
        var conditionCols = Options.Conditions.Select(c => c.ColumnName);
        return Options.TargetColumns.Union(conditionCols);
    }
}
