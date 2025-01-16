using FilePrepper.CLI.Tools;
using FilePrepper.Tasks;
using FilePrepper.Tasks.RemoveColumns;
using Microsoft.Extensions.Logging;

namespace FilePrepper.CLI.Tools.RemoveColumns;

public class RemoveColumnsHandler : ICommandHandler
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<RemoveColumnsHandler> _logger;

    public RemoveColumnsHandler(
        ILoggerFactory loggerFactory,
        ILogger<RemoveColumnsHandler> logger)
    {
        _loggerFactory = loggerFactory;
        _logger = logger;
    }

    public async Task<int> ExecuteAsync(ICommandParameters parameters)
    {
        var opts = (RemoveColumnsParameters)parameters;

        try
        {
            if (!opts.Columns.Any())
            {
                _logger.LogError("At least one column must be specified to remove");
                return 1;
            }

            // Check for empty or whitespace column names
            var invalidColumns = opts.Columns.Where(c => string.IsNullOrWhiteSpace(c)).ToList();
            if (invalidColumns.Any())
            {
                _logger.LogError("Column names cannot be empty or whitespace");
                return 1;
            }

            var options = new RemoveColumnsOption
            {
                RemoveColumns = opts.Columns.ToList(),
                Common = opts.GetCommonOptions()
            };

            var taskLogger = _loggerFactory.CreateLogger<RemoveColumnsTask>();
            var task = new RemoveColumnsTask(options, taskLogger);
            var context = new TaskContext
            {
                InputPath = opts.InputPath,
                OutputPath = opts.OutputPath
            };

            return await task.ExecuteAsync(context) ? 0 : 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing remove-columns command");
            return 1;
        }
    }

    public string? GetExample() =>
    "remove-columns -i input.csv -o output.csv -c \"TempColumn1,TempColumn2\"";
}
