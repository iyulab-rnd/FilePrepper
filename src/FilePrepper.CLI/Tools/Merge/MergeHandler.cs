using FilePrepper.Tasks;
using FilePrepper.Tasks.Merge;
using Microsoft.Extensions.Logging;

namespace FilePrepper.CLI.Tools.Merge;

public class MergeHandler : BaseCommandHandler<MergeParameters>
{
    public MergeHandler(
        ILoggerFactory loggerFactory,
        ILogger<MergeHandler> logger)
        : base(loggerFactory, logger)
    {
    }

    public override async Task<int> ExecuteAsync(ICommandParameters parameters)
    {
        var opts = (MergeParameters)parameters;
        if (!ValidateParameters(opts))
        {
            return ExitCodes.InvalidArguments;
        }

        return await HandleExceptionAsync(async () =>
        {
            if (!Enum.TryParse<MergeType>(opts.MergeType, true, out var mergeType))
            {
                _logger.LogError("Invalid merge type: {Type}", opts.MergeType);
                return ExitCodes.InvalidArguments;
            }

            if (!Enum.TryParse<JoinType>(opts.JoinType, true, out var joinType))
            {
                _logger.LogError("Invalid join type: {Type}", opts.JoinType);
                return ExitCodes.InvalidArguments;
            }

            var options = new MergeOption
            {
                InputPaths = opts.InputFiles.ToList(),
                OutputPath = opts.OutputPath,
                MergeType = mergeType,
                JoinType = joinType,
                JoinKeyColumns = opts.JoinKeyColumns.Select(column =>
                {
                    if (int.TryParse(column, out int index))
                    {
                        return ColumnIdentifier.ByIndex(index);
                    }
                    return ColumnIdentifier.ByName(column);
                }).ToList(),
                HasHeader = opts.HasHeader,
                IgnoreErrors = opts.IgnoreErrors
            };

            var taskLogger = _loggerFactory.CreateLogger<MergeTask>();
            var task = new MergeTask(taskLogger);
            var context = new TaskContext(options);

            _logger.LogInformation("Merging {Count} files using {Type} merge type",
                opts.InputFiles.Count(), mergeType);

            var success = await task.ExecuteAsync(context);
            return success ? ExitCodes.Success : ExitCodes.Error;
        });
    }
}