using System.Data;

namespace FilePrepper.Tasks.ColumnInteraction;

public class ColumnInteractionTask : BaseTask<ColumnInteractionOption>
{
    public ColumnInteractionTask(
        ColumnInteractionOption options,
        ILogger<ColumnInteractionTask> logger,
        ILogger<ColumnInteractionValidator> validatorLogger)
        : base(options, logger, new ColumnInteractionValidator(validatorLogger))
    {
    }

    protected override async Task<List<Dictionary<string, string>>> ProcessRecordsAsync(
        List<Dictionary<string, string>> records)
    {
        // 헤더 검증
        var headers = records.FirstOrDefault()?.Keys.ToList() ?? new List<string>();
        if (headers.Contains(Options.OutputColumn))
        {
            throw new InvalidOperationException($"Output column already exists: {Options.OutputColumn}");
        }

        // 컬럼 연산 수행
        await Task.Run(() =>
        {
            foreach (var record in records)
            {
                try
                {
                    record[Options.OutputColumn] = ProcessRecord(record);
                }
                catch (Exception ex) when (Options.IgnoreErrors)
                {
                    _logger.LogWarning("Error processing row: {Error}", ex.Message);
                    record[Options.OutputColumn] = Options.DefaultValue ?? string.Empty;
                }
            }
        });

        return records;
    }

    private string ProcessRecord(Dictionary<string, string> record)
    {
        var values = Options.SourceColumns
            .Select(col => record.GetValueOrDefault(col, string.Empty))
            .ToArray();

        return Options.Operation switch
        {
            OperationType.Concat => string.Join(string.Empty, values),
            OperationType.Custom => ProcessCustomOperation(values),
            _ => ProcessNumericOperation(values)
        };
    }

    private string ProcessNumericOperation(string[] values)
    {
        var numbers = GetNumericValues(values);
        if (!numbers.Any()) return string.Empty;

        double result = Options.Operation switch
        {
            OperationType.Add => numbers.Sum(),
            OperationType.Subtract => numbers.Aggregate((a, b) => a - b),
            OperationType.Multiply => numbers.Aggregate((a, b) => a * b),
            OperationType.Divide => numbers.Aggregate((a, b) =>
                b != 0 ? a / b : throw new DivideByZeroException("Division by zero")),
            _ => throw new ArgumentException($"Unsupported operation type: {Options.Operation}")
        };

        return result.ToString(CultureInfo.InvariantCulture);
    }

    private string ProcessCustomOperation(string[] values)
    {
        if (string.IsNullOrWhiteSpace(Options.CustomExpression))
        {
            throw new ArgumentException("Custom expression is required for Custom operation type");
        }

        string expression = Options.CustomExpression;
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = GetValidNumericValue(values[i]);
            expression = expression.Replace($"${i + 1}", values[i]);
        }

        try
        {
            var dt = new DataTable();
            var result = dt.Compute(expression, "");
            return Convert.ToString(result, CultureInfo.InvariantCulture) ?? string.Empty;
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Error evaluating custom expression: {ex.Message}");
        }
    }

    private List<double> GetNumericValues(string[] values)
    {
        var numbers = new List<double>();

        foreach (var value in values)
        {
            if (CsvUtils.TryParseNumeric(value, out var number))
            {
                numbers.Add(number);
            }
            else if (Options.IgnoreErrors && !string.IsNullOrWhiteSpace(Options.DefaultValue))
            {
                if (double.TryParse(Options.DefaultValue, out var defaultNum))
                {
                    numbers.Add(defaultNum);
                }
            }
            else
            {
                throw new ArgumentException($"Invalid numeric value: {value}");
            }
        }

        return numbers;
    }

    private string GetValidNumericValue(string value)
    {
        if (CsvUtils.TryParseNumeric(value, out _))
        {
            return value;
        }

        if (Options.IgnoreErrors && !string.IsNullOrWhiteSpace(Options.DefaultValue))
        {
            return Options.DefaultValue;
        }

        throw new ArgumentException($"Invalid numeric value: {value}");
    }

    protected override IEnumerable<string> GetRequiredColumns()
    {
        return Options.SourceColumns;
    }
}