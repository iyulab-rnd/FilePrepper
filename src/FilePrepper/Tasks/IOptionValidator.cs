namespace FilePrepper.Tasks;

public interface IOptionValidator
{
    bool Validate(ITaskOption option, out string[] errors);
}