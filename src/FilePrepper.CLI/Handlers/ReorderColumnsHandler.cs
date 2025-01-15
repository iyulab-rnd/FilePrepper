using FilePrepper.Tasks.ReorderColumns;
using FilePrepper.Tasks;
using Microsoft.Extensions.Logging;
using FilePrepper.CLI.Parameters;

namespace FilePrepper.CLI.Handlers;

public class ReorderColumnsHandler : ICommandHandler
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<ReorderColumnsHandler> _logger;

    public ReorderColumnsHandler(
        ILoggerFactory loggerFactory,
        ILogger<ReorderColumnsHandler> logger)
    {
        _loggerFactory = loggerFactory;
        _logger = logger;
    }

    public async Task<int> ExecuteAsync(ICommandParameters parameters)
    {
        var opts = (ReorderColumnsParameters)parameters;

        try
        {
            var options = new ReorderColumnsOption
            {
                Order = opts.Order.ToList(),
                Common = opts.GetCommonOptions()
            };

            var taskLogger = _loggerFactory.CreateLogger<ReorderColumnsTask>();
            var task = new ReorderColumnsTask(options, taskLogger);
            var context = new TaskContext
            {
                InputPath = opts.InputPath,
                OutputPath = opts.OutputPath
            };

            return await task.ExecuteAsync(context) ? 0 : 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing reorder-columns command");
            return 1;
        }
    }

    public string? GetExample() =>
    "reorder-columns -i input.csv -o output.csv -o \"ID,Name,Age,Score\"";
}