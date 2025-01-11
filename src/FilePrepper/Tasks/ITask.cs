namespace FilePrepper.Tasks;

public interface ITask
{
    string Name { get; }
    Task<bool> ExecuteAsync(TaskContext context);
    ITaskOption Options { get; }
}