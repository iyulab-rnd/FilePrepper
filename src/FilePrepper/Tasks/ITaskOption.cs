namespace FilePrepper.Tasks;

public interface ITaskOption
{
    bool IsValid { get; }
    string[] Validate();
    CommonTaskOptions Common { get; set; }
}