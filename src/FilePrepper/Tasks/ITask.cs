namespace FilePrepper.Tasks;

public interface ITask
{
    string Name { get; }
    bool Execute(TaskContext context);
    ITaskOption Options { get; }
}