namespace FilePrepper.Tasks.DataTypeConvert;

public class DataTypeConvertTask : BaseTask<DataTypeConvertOption>
{
    private static readonly string[] TrueValues = ["true", "yes", "1", "y", "t"];
    private static readonly string[] FalseValues = ["false", "no", "0", "n", "f"];

    public DataTypeConvertTask(
        DataTypeConvertOption options,
        ILogger<DataTypeConvertTask> logger)
        : base(options, logger)
    {
    }

    protected override async Task<List<Dictionary<string, string>>> ProcessRecordsAsync(
        List<Dictionary<string, string>> records)
    {
        await Task.Run(() =>
        {
            foreach (var record in records)
            {
                foreach (var conversion in Options.Conversions)
                {
                    try
                    {
                        record[conversion.ColumnName] = ConvertValue(
                            record[conversion.ColumnName],
                            conversion);
                    }
                    catch (Exception) when (Options.Common.ErrorHandling.IgnoreErrors)
                    {
                        record[conversion.ColumnName] = conversion.DefaultValue ?? string.Empty;
                    }
                }
            }
        });

        return records;
    }

    private string ConvertValue(string value, ColumnTypeConversion conversion)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return conversion.DefaultValue ?? string.Empty;
        }

        var culture = conversion.Culture ?? CultureInfo.InvariantCulture;
        string processedValue = conversion.TrimWhitespace ? value.Trim() : value;

        try
        {
            return conversion.TargetType switch
            {
                DataType.Integer => ConvertToInteger(processedValue, culture).ToString(culture),
                DataType.Decimal => ConvertToDecimal(processedValue, culture),
                DataType.DateTime => ConvertToDateTime(processedValue, conversion, culture),
                DataType.Boolean => ConvertToBoolean(processedValue, conversion.IgnoreCase).ToString(),
                DataType.String => processedValue,
                _ => throw new ArgumentException($"Unsupported target type: {conversion.TargetType}")
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                "Error converting value '{Value}' to {Type}: {Error}",
                value,
                conversion.TargetType,
                ex.Message);
            throw;
        }
    }

    private int ConvertToInteger(string value, CultureInfo culture)
    {
        // 소수점이 있는 경우 반올림
        if (value.Contains(".") || value.Contains(","))
        {
            return (int)Math.Round(decimal.Parse(value, culture));
        }
        return int.Parse(value, culture);
    }

    private string ConvertToDecimal(string value, CultureInfo culture)
    {
        var styles = NumberStyles.Number | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands;
        var decimalValue = decimal.Parse(value, styles, culture);

        // 원본 소수점 자릿수 유지
        var originalDecimals = 1; // 최소 1자리
        foreach (var separator in new[] {
            culture.NumberFormat.NumberDecimalSeparator,
            ".",
            ","
        })
        {
            if (value.Contains(separator))
            {
                var parts = value.Split(separator);
                if (parts.Length > 1)
                {
                    originalDecimals = Math.Max(originalDecimals, parts[1].Trim().Length);
                    break;
                }
            }
        }

        return decimalValue.ToString($"F{originalDecimals}", CultureInfo.InvariantCulture);
    }

    private string ConvertToDateTime(
        string value,
        ColumnTypeConversion conversion,
        CultureInfo culture)
    {
        var dateTime = DateTime.Parse(value, culture);
        return dateTime.ToString(conversion.DateTimeFormat, culture);
    }

    private bool ConvertToBoolean(string value, bool ignoreCase)
    {
        if (TrueValues.Contains(value, StringComparer.Create(CultureInfo.InvariantCulture, ignoreCase)))
        {
            return true;
        }
        if (FalseValues.Contains(value, StringComparer.Create(CultureInfo.InvariantCulture, ignoreCase)))
        {
            return false;
        }

        return bool.Parse(value);
    }

    protected override IEnumerable<string> GetRequiredColumns()
    {
        return Options.Conversions.Select(c => c.ColumnName);
    }
}
