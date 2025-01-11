namespace FilePrepper.Tasks;

public class TaskContext
{
    public string InputPath { get; set; } = null!;
    public string? OutputPath { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = [];
}
