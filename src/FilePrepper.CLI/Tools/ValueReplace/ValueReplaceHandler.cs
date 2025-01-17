using FilePrepper.Tasks.ValueReplace;
using FilePrepper.Tasks;
using Microsoft.Extensions.Logging;

namespace FilePrepper.CLI.Tools.ValueReplace;

public class ValueReplaceHandler : BaseCommandHandler<ValueReplaceParameters>
{
    public ValueReplaceHandler(
        ILoggerFactory loggerFactory,
        ILogger<ValueReplaceHandler> logger)
        : base(loggerFactory, logger)
    {
    }

    public override async Task<int> ExecuteAsync(ICommandParameters parameters)
    {
        var opts = (ValueReplaceParameters)parameters;
        if (!ValidateParameters(opts))
        {
            return ExitCodes.InvalidArguments;
        }

        return await HandleExceptionAsync(async () =>
        {
            var replaceMethods = new List<ColumnReplaceMethod>();
            foreach (var replaceStr in opts.ReplaceMethods)
            {
                var parts = replaceStr.Split(':', 2);
                var columnName = parts[0];
                var replacementRules = parts[1].Split(';');
                var replacements = new Dictionary<string, string>();

                foreach (var rule in replacementRules)
                {
                    var valueParts = rule.Split('=', 2);
                    replacements[valueParts[0]] = valueParts[1];
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
                InputPath = opts.InputPath,
                OutputPath = opts.OutputPath,
                HasHeader = opts.HasHeader,
                IgnoreErrors = opts.IgnoreErrors
            };

            var taskLogger = _loggerFactory.CreateLogger<ValueReplaceTask>();
            var task = new ValueReplaceTask(taskLogger);
            var context = new TaskContext(options);

            _logger.LogInformation("Replacing values in {Input}. Rules: {Rules}",
                opts.InputPath, string.Join(", ", opts.ReplaceMethods));

            var success = await task.ExecuteAsync(context);
            return success ? ExitCodes.Success : ExitCodes.Error;
        });
    }

    public override string? GetExample() =>
        "replace -i input.csv -o output.csv -r \"Status:active=1;inactive=0,Gender:M=Male;F=Female\"";
}