namespace FilePrepper.Tasks;

public class CommonTaskOptions
{
    public bool IgnoreErrors { get; set; }
    public string? DefaultValue { get; set; }
    public bool AppendToSource { get; set; }
    public string? OutputColumnTemplate { get; set; }
}
