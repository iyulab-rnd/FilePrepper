namespace FilePrepper.Tasks;

public class TaskContext
{
    public TaskContext(ITaskOption options)
    {
        Options = options;
    }

    public required string InputPath { get; init; }
    public required string OutputPath { get; init; }
    public ITaskOption Options { get; }
    public Dictionary<string, object> Parameters { get; } = [];

    // 편의를 위한 제네릭 메서드 추가
    public T GetOptions<T>() where T : class, ITaskOption
    {
        if (Options is not T typedOptions)
        {
            throw new InvalidOperationException($"Options is not of type {typeof(T).Name}");
        }
        return typedOptions;
    }
}