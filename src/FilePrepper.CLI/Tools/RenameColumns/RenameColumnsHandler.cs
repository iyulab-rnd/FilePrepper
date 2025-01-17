using FilePrepper.Tasks;
using FilePrepper.Tasks.RenameColumns;
using Microsoft.Extensions.Logging;

namespace FilePrepper.CLI.Tools.RenameColumns;

public class RenameColumnsHandler : BaseCommandHandler<RenameColumnsParameters>
{
    public RenameColumnsHandler(
        ILoggerFactory loggerFactory,
        ILogger<RenameColumnsHandler> logger)
        : base(loggerFactory, logger)
    {
    }

    public override async Task<int> ExecuteAsync(ICommandParameters parameters)
    {
        var opts = (RenameColumnsParameters)parameters;
        if (!ValidateParameters(opts))
        {
            return ExitCodes.InvalidArguments;
        }

        return await HandleExceptionAsync(async () =>
        {
            var renameMap = new Dictionary<string, string>();
            foreach (var mapping in opts.Mappings)
            {
                var parts = mapping.Split(':');
                renameMap[parts[0].Trim()] = parts[1].Trim();
            }

            var options = new RenameColumnsOption
            {
                InputPath = opts.InputPath,
                OutputPath = opts.OutputPath,
                RenameMap = renameMap,
                HasHeader = opts.HasHeader,
                IgnoreErrors = opts.IgnoreErrors
            };

            var taskLogger = _loggerFactory.CreateLogger<RenameColumnsTask>();
            var task = new RenameColumnsTask(taskLogger);
            var context = new TaskContext(options);

            _logger.LogInformation("Renaming columns in {Input}. Mappings: {Mappings}",
                opts.InputPath, string.Join(", ", opts.Mappings));

            var success = await task.ExecuteAsync(context);
            return success ? ExitCodes.Success : ExitCodes.Error;
        });
    }

    public override string? GetExample() =>
        "rename-columns -i input.csv -o output.csv -m \"OldName:NewName,Price:Cost\"";
}