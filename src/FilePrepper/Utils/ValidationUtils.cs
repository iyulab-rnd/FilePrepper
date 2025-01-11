namespace FilePrepper.Utils;

public static class ValidationUtils
{
    public static string[] ValidateColumns(string[] columns, string purpose = "columns")
    {
        var errors = new List<string>();

        if (columns == null || columns.Length == 0)
        {
            errors.Add($"At least one {purpose} must be specified");
        }
        else if (columns.Any(string.IsNullOrWhiteSpace))
        {
            errors.Add($"{purpose} name cannot be empty or whitespace");
        }

        return [.. errors];
    }

    public static List<string> ValidateColumns(IEnumerable<string> columns)
    {
        return ValidateColumns(columns.ToArray()).ToList();
    }

    public static string[] ValidateNumericValue(
        double value,
        string name,
        double? min = null,
        double? max = null)
    {
        var errors = new List<string>();

        if (min.HasValue && value < min.Value)
        {
            errors.Add($"{name} must be greater than {min.Value}");
        }
        if (max.HasValue && value > max.Value)
        {
            errors.Add($"{name} must be less than {max.Value}");
        }

        return [.. errors];
    }

    public static string[] ValidateRequiredOption<T>(T option, string optionName)
        where T : class
    {
        if (option == null)
        {
            return new[] { $"{optionName} cannot be null" };
        }
        return Array.Empty<string>();
    }

    public static bool ValidateAndLogErrors(IEnumerable<string> errors, ILogger logger)
    {
        var errorList = errors.ToList();
        if (errorList.Count != 0)
        {
            foreach (var error in errorList)
            {
                logger.LogError("Validation error: {Error}", error);
            }
            return false;
        }
        return true;
    }
}