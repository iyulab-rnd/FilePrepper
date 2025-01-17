using FilePrepper.Tasks.AddColumns;
using FilePrepper.Tasks;
using Microsoft.Extensions.Logging;
using FilePrepper.CLI.Tools;

namespace FilePrepper.CLI.Tools.AddColumns;

public class AddColumnsHandler : ICommandHandler
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<AddColumnsHandler> _logger;

    public AddColumnsHandler(
        ILoggerFactory loggerFactory,
        ILogger<AddColumnsHandler> logger)
    {
        _loggerFactory = loggerFactory;
        _logger = logger;
    }

    public async Task<int> ExecuteAsync(ICommandParameters parameters)
    {
        var opts = (AddColumnsParameters)parameters;

        try
        {
            var columns = new Dictionary<string, string>();
            foreach (var col in opts.Columns)
            {
                var parts = col.Split('=', 2);
                if (parts.Length != 2)
                {
                    _logger.LogError("Invalid column format: {Column}. Expected format: name=value", col);
                    return 1;
                }
                columns[parts[0]] = parts[1];
            }

            var options = new AddColumnsOption
            {
                InputPath = opts.InputPath,
                OutputPath = opts.OutputPath,
                NewColumns = columns,
                Common = opts.GetCommonOptions()
            };

            var taskLogger = _loggerFactory.CreateLogger<AddColumnsTask>();
            var task = new AddColumnsTask(taskLogger);
            var context = new TaskContext(options);

            return await task.ExecuteAsync(context) ? 0 : 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing add-columns command");
            return 1;
        }
    }

    public string? GetExample() => "add-columns -i input.csv -o output.csv -c \"Age=30,City=Seoul\"";
}