using CsvHelper.Configuration;
using System.Globalization;

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

    public static bool TryParseNumeric(string value, out double result)
    {
        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
        {
            return true;
        }
        result = 0;
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