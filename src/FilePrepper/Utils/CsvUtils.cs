using CsvHelper.Configuration;

namespace FilePrepper.Utils;

public static class CsvUtils
{
    public static CsvConfiguration GetDefaultConfiguration()
    {
        return new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null
        };
    }

    public static List<string> ValidateHeaders(IEnumerable<string> requiredColumns, IEnumerable<string> actualHeaders)
    {
        var errors = new List<string>();
        var headerSet = new HashSet<string>(actualHeaders);

        foreach (var column in requiredColumns)
        {
            if (!headerSet.Contains(column))
            {
                errors.Add($"Required column not found: {column}");
            }
        }

        return errors;
    }

    /// <summary>
    /// Parse string to double, but reject NaN/Infinity as invalid
    /// </summary>
    public static bool TryParseNumeric(string? input, out double value)
    {
        if (double.TryParse(input, out value))
        {
            // If parsed but is NaN or Infinity, treat as invalid
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                return false;
            }
            return true;
        }
        return false;
    }

    public static Dictionary<string, double> ParseNumericColumns(
        Dictionary<string, string> record,
        string[] columns,
        bool ignoreErrors = false,
        double? defaultValue = null)
    {
        var result = new Dictionary<string, double>();
        foreach (var col in columns)
        {
            if (!record.TryGetValue(col, out string? value))
            {
                if (ignoreErrors && defaultValue.HasValue)
                {
                    result[col] = defaultValue.Value;
                    continue;
                }
                throw new KeyNotFoundException($"Column not found: {col}");
            }

            if (TryParseNumeric(value, out var v))
            {
                result[col] = v;
            }
            else if (ignoreErrors && defaultValue.HasValue)
            {
                result[col] = defaultValue.Value;
            }
            else
            {
                throw new ArgumentException($"Invalid numeric value in column {col}: {record[col]}");
            }
        }
        return result;
    }

    public static bool ValidateNumericColumns(
        this Dictionary<string, string> record,
        IEnumerable<string> numericColumns,
        out Dictionary<string, double> numericValues,
        bool ignoreErrors = false,
        string? defaultValue = null)
    {
        numericValues = [];

        foreach (var column in numericColumns)
        {
            if (!record.TryGetValue(column, out string? value))
            {
                if (ignoreErrors && defaultValue != null)
                {
                    if (double.TryParse(defaultValue, out var defaultNum))
                    {
                        numericValues[column] = defaultNum;
                        continue;
                    }
                }
                return false;
            }

            if (!CsvUtils.TryParseNumeric(value, out var v2))
            {
                if (ignoreErrors && defaultValue != null)
                {
                    if (double.TryParse(defaultValue, out var defaultNum))
                    {
                        numericValues[column] = defaultNum;
                        continue;
                    }
                }
                return false;
            }

            numericValues[column] = v2;
        }

        return true;
    }
}