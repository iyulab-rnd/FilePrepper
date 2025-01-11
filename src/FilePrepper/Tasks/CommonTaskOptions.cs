namespace FilePrepper.Tasks;

public class CommonTaskOptions
{
    public ErrorHandlingOptions ErrorHandling { get; set; } = new();
    public OutputOptions Output { get; set; } = new();
}

public class ErrorHandlingOptions
{
    public bool IgnoreErrors { get; set; }
    public string? DefaultValue { get; set; }
}

public class OutputOptions
{
    public bool AppendToSource { get; set; }
    public string? OutputColumnTemplate { get; set; }
}