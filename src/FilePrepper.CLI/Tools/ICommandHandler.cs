namespace FilePrepper.CLI.Tools;

public interface ICommandHandler
{
    Task<int> ExecuteAsync(ICommandParameters parameters);
    string? GetExample();
}
