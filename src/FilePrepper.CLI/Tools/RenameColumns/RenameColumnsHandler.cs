using FilePrepper.CLI.Tools;
using FilePrepper.Tasks;
using FilePrepper.Tasks.RenameColumns;
using Microsoft.Extensions.Logging;

namespace FilePrepper.CLI.Tools.RenameColumns;

public class RenameColumnsHandler : ICommandHandler
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<RenameColumnsHandler> _logger;

    public RenameColumnsHandler(
        ILoggerFactory loggerFactory,
        ILogger<RenameColumnsHandler> logger)
    {
        _loggerFactory = loggerFactory;
        _logger = logger;
    }

    public async Task<int> ExecuteAsync(ICommandParameters parameters)
    {
        var opts = (RenameColumnsParameters)parameters;

        try
        {
            var renameMap = new Dictionary<string, string>();
            foreach (var mapping in opts.Mappings)
            {
                var parts = mapping.Split(':');
                if (parts.Length != 2)
                {
                    _logger.LogError("Invalid mapping format: {Mapping}. Expected format: oldName:newName", mapping);
                    return 1;
                }

                if (string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
                {
                    _logger.LogError("Column names cannot be empty: {Mapping}", mapping);
                    return 1;
                }

                renameMap[parts[0]] = parts[1];
            }

            var options = new RenameColumnsOption
            {
                RenameMap = renameMap,
                Common = opts.GetCommonOptions()
            };

            var taskLogger = _loggerFactory.CreateLogger<RenameColumnsTask>();
            var task = new RenameColumnsTask(taskLogger);
            var context = new TaskContext(options)
            {
                InputPath = opts.InputPath,
                OutputPath = opts.OutputPath
            };

            return await task.ExecuteAsync(context) ? 0 : 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing rename-columns command");
            return 1;
        }
    }

    public string? GetExample() =>
    "rename-columns -i input.csv -o output.csv -m \"OldName:NewName,Price:Cost\"";
}