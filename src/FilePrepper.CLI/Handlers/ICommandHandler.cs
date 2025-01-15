using FilePrepper.CLI.Parameters;

namespace FilePrepper.CLI.Handlers;

public interface ICommandHandler
{
    Task<int> ExecuteAsync(ICommandParameters parameters);
    string? GetExample();
}
