using FilePrepper.Tasks.ValueReplace;
using FilePrepper.Tasks;
using Microsoft.Extensions.Logging;
using FilePrepper.CLI.Parameters;

namespace FilePrepper.CLI.Handlers;

public class ValueReplaceHandler : ICommandHandler
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<ValueReplaceHandler> _logger;

    public ValueReplaceHandler(
        ILoggerFactory loggerFactory,
        ILogger<ValueReplaceHandler> logger)
    {
        _loggerFactory = loggerFactory;
        _logger = logger;
    }

    public async Task<int> ExecuteAsync(ICommandParameters parameters)
    {
        var opts = (ValueReplaceParameters)parameters;

        try
        {
            var replaceMethods = new List<ColumnReplaceMethod>();

            foreach (var replaceStr in opts.ReplaceMethods)
            {
                var parts = replaceStr.Split(':', 2);
                if (parts.Length != 2)
                {
                    _logger.LogError("Invalid replacement format: {Replace}. Expected format: column:oldValue=newValue[;oldValue2=newValue2]", replaceStr);
                    return 1;
                }

                var columnName = parts[0];
                var replacementRules = parts[1].Split(';');
                var replacements = new Dictionary<string, string>();

                foreach (var rule in replacementRules)
                {
                    var valueParts = rule.Split('=', 2);
                    if (valueParts.Length != 2)
                    {
                        _logger.LogError("Invalid replacement rule: {Rule}. Expected format: oldValue=newValue", rule);
                        return 1;
                    }

                    replacements[valueParts[0]] = valueParts[1];
                }

                if (!replacements.Any())
                {
                    _logger.LogError("No valid replacement rules found for column: {Column}", columnName);
                    return 1;
                }

                replaceMethods.Add(new ColumnReplaceMethod
                {
                    ColumnName = columnName,
                    Replacements = replacements
                });
            }

            var options = new ValueReplaceOption
            {
                ReplaceMethods = replaceMethods,
                Common = opts.GetCommonOptions()
            };

            var taskLogger = _loggerFactory.CreateLogger<ValueReplaceTask>();
            var task = new ValueReplaceTask(options, taskLogger);
            var context = new TaskContext
            {
                InputPath = opts.InputPath,
                OutputPath = opts.OutputPath
            };

            return await task.ExecuteAsync(context) ? 0 : 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing replace command");
            return 1;
        }
    }
}