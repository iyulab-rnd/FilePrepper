namespace FilePrepper.Tasks;

// Extension methods for common CSV operations
public static class CsvTaskExtensions
{
    public static bool ValidateNumericColumns(
        this Dictionary<string, string> record,
        IEnumerable<string> numericColumns,
        out Dictionary<string, double> numericValues,
        bool ignoreErrors = false,
        string? defaultValue = null)
    {
        numericValues = new Dictionary<string, double>();

        foreach (var column in numericColumns)
        {
            if (!record.ContainsKey(column))
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

            if (!CsvUtils.TryParseNumeric(record[column], out var value))
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

            numericValues[column] = value;
        }

        return true;
    }
}