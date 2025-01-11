using FilePrepper.Tasks.Merge;
using FilePrepper.Tasks;
using Microsoft.Extensions.Logging;
using FilePrepper.CLI.Parameters;

namespace FilePrepper.CLI.Handlers;

public class MergeHandler : ICommandHandler
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<MergeHandler> _logger;

    public MergeHandler(
        ILoggerFactory loggerFactory,
        ILogger<MergeHandler> logger)
    {
        _loggerFactory = loggerFactory;
        _logger = logger;
    }

    public async Task<int> ExecuteAsync(ICommandParameters parameters)
    {
        var opts = (MergeParameters)parameters;

        try
        {
            // Validate input files
            if (!opts.InputPaths.Any())
            {
                _logger.LogError("At least one additional input file must be specified");
                return 1;
            }

            foreach (var path in opts.InputPaths)
            {
                if (!File.Exists(path))
                {
                    _logger.LogError("Input file not found: {Path}", path);
                    return 1;
                }
            }

            // Validate merge type
            if (!Enum.TryParse<MergeType>(opts.MergeType, true, out var mergeType))
            {
                _logger.LogError("Invalid merge type: {Type}. Valid values are: {ValidValues}",
                    opts.MergeType, string.Join(", ", Enum.GetNames<MergeType>()));
                return 1;
            }

            // Validate join type for horizontal merge
            JoinType joinType = JoinType.Inner;
            if (mergeType == MergeType.Horizontal)
            {
                if (!Enum.TryParse(opts.JoinType, true, out joinType))
                {
                    _logger.LogError("Invalid join type: {Type}. Valid values are: {ValidValues}",
                        opts.JoinType, string.Join(", ", Enum.GetNames<JoinType>()));
                    return 1;
                }

                // Key columns are required for horizontal merge
                if (!opts.JoinKeyColumns.Any())
                {
                    _logger.LogError("Key columns must be specified for horizontal merge");
                    return 1;
                }
            }

            // Create input paths list including the main input file
            var allInputPaths = new List<string> { opts.InputPath };
            allInputPaths.AddRange(opts.InputPaths);

            var options = new MergeOption
            {
                InputPaths = allInputPaths,
                MergeType = mergeType,
                JoinType = joinType,
                JoinKeyColumns = opts.JoinKeyColumns.ToList(),
                Common = opts.GetCommonOptions()
            };

            var taskLogger = _loggerFactory.CreateLogger<MergeTask>();
            var task = new MergeTask(options, taskLogger);
            var context = new TaskContext
            {
                InputPath = opts.InputPath,
                OutputPath = opts.OutputPath
            };

            _logger.LogInformation("Merging {Count} files using {Type} merge",
                allInputPaths.Count, mergeType);

            return await task.ExecuteAsync(context) ? 0 : 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing merge command");
            return 1;
        }
    }
}