using FilePrepper.Utils;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace FilePrepper.Tasks.Aggregate;

public class AggregateTask : BaseTask<AggregateOption>
{
    public AggregateTask(
        AggregateOption options,
        ILogger<AggregateTask> logger,
        ILogger<AggregateValidator> validatorLogger)
        : base(options, logger, new AggregateValidator(validatorLogger))
    {
    }

    protected override async Task<List<Dictionary<string, string>>> ProcessRecordsAsync(
        List<Dictionary<string, string>> records)
    {
        // 그룹화
        var groups = await GroupRecordsAsync(records);

        // 집계 계산
        var groupResults = await CalculateAggregatesAsync(groups);

        // 결과 포맷팅
        return Options.Common.AppendToSource
            ? await WriteAppendedResultsAsync(records, groupResults)
            : await WriteAggregateResultsAsync(groupResults);
    }

    private async Task<Dictionary<string, List<Dictionary<string, string>>>> GroupRecordsAsync(
        List<Dictionary<string, string>> records)
    {
        return await Task.Run(() =>
            records.GroupBy(record =>
                string.Join("|", Options.GroupByColumns.Select(col => record[col])))
                .ToDictionary(g => g.Key, g => g.ToList()));
    }

    private async Task<Dictionary<string, Dictionary<string, string>>> CalculateAggregatesAsync(
        Dictionary<string, List<Dictionary<string, string>>> groups)
    {
        return await Task.Run(() =>
        {
            var results = new Dictionary<string, Dictionary<string, string>>();

            foreach (var group in groups)
            {
                var result = new Dictionary<string, string>();
                var keyValues = group.Key.Split('|');

                // 그룹 키 설정
                for (int i = 0; i < Options.GroupByColumns.Length; i++)
                {
                    result[Options.GroupByColumns[i]] = keyValues[i];
                }

                // 집계 계산
                foreach (var aggCol in Options.AggregateColumns)
                {
                    var values = new List<double>();
                    foreach (var record in group.Value)
                    {
                        if (CsvUtils.TryParseNumeric(record[aggCol.ColumnName], out var value))
                        {
                            values.Add(value);
                        }
                    }

                    if (values.Any())
                    {
                        var aggregateValue = CalculateAggregate(values, aggCol.Function);
                        var outputColumnName = GetAggregateColumnName(aggCol);
                        result[outputColumnName] = aggregateValue.ToString(CultureInfo.InvariantCulture);
                    }
                }

                results[group.Key] = result;
            }

            return results;
        });
    }

    private async Task<List<Dictionary<string, string>>> WriteAppendedResultsAsync(
        List<Dictionary<string, string>> records,
        Dictionary<string, Dictionary<string, string>> groupResults)
    {
        return await Task.Run(() =>
        {
            foreach (var record in records)
            {
                var groupKey = string.Join("|", Options.GroupByColumns.Select(col => record[col]));
                if (groupResults.TryGetValue(groupKey, out var groupResult))
                {
                    foreach (var (key, value) in groupResult.Where(kvp =>
                        !Options.GroupByColumns.Contains(kvp.Key)))
                    {
                        record[key] = value;
                    }
                }
            }
            return records;
        });
    }

    private async Task<List<Dictionary<string, string>>> WriteAggregateResultsAsync(
        Dictionary<string, Dictionary<string, string>> groupResults)
    {
        return await Task.Run(() => groupResults.Values.ToList());
    }

    private string GetAggregateColumnName(AggregateColumn aggCol)
    {
        if (!Options.Common.AppendToSource)
        {
            return aggCol.OutputColumnName;
        }

        return Options.Common.OutputColumnTemplate!
            .Replace("{column}", aggCol.ColumnName)
            .Replace("{function}", aggCol.Function.ToString())
            .Replace("{groupBy}", string.Join("_", Options.GroupByColumns));
    }

    private double CalculateAggregate(List<double> values, AggregateFunction function)
    {
        return function switch
        {
            AggregateFunction.Sum => values.Sum(),
            AggregateFunction.Average => MathUtils.CalculateMean(values),
            AggregateFunction.Count => values.Count,
            AggregateFunction.Min => values.Min(),
            AggregateFunction.Max => values.Max(),
            _ => throw new ArgumentException($"Unsupported function: {function}")
        };
    }

    protected override IEnumerable<string> GetRequiredColumns()
    {
        return Options.GroupByColumns.Concat(
            Options.AggregateColumns.Select(x => x.ColumnName));
    }
}
