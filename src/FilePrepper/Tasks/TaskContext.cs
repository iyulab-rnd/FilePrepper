namespace FilePrepper.Tasks;

public class TaskContext
{
    public required string InputPath { get; init; }
    public required string OutputPath { get; init; }
    public Dictionary<string, object> Parameters { get; } = [];
}