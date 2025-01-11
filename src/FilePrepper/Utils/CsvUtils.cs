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
            if (!record.ContainsKey(col))
            {
                if (ignoreErrors && defaultValue.HasValue)
                {
                    result[col] = defaultValue.Value;
                    continue;
                }
                throw new KeyNotFoundException($"Column not found: {col}");
            }

            if (TryParseNumeric(record[col], out var value))
            {
                result[col] = value;
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


}