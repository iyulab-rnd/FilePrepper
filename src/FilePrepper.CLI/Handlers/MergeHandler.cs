using FilePrepper.CLI.Parameters;
using FilePrepper.Tasks;
using FilePrepper.Tasks.Merge;
using Microsoft.Extensions.Logging;

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
            // 입력 파일 검증
            if (!opts.InputFiles.Any())
            {
                _logger.LogError("No input files specified");
                return ExitCodes.ValidationError;
            }

            if (opts.InputFiles.Count() < 2)
            {
                _logger.LogError("At least two input files are required for merge operation");
                return ExitCodes.ValidationError;
            }

            foreach (var path in opts.InputFiles)
            {
                if (!File.Exists(path))
                {
                    _logger.LogError("Input file not found: {Path}", path);
                    return ExitCodes.ValidationError;
                }
            }

            // Merge 타입 검증
            if (!Enum.TryParse<MergeType>(opts.MergeType, true, out var mergeType))
            {
                _logger.LogError("Invalid merge type: {Type}. Valid values are: {ValidValues}",
                    opts.MergeType, string.Join(", ", Enum.GetNames<MergeType>()));
                return ExitCodes.ValidationError;
            }

            // Join 타입 검증 (Horizontal merge인 경우)
            JoinType joinType = JoinType.Inner;
            if (mergeType == MergeType.Horizontal)
            {
                if (!Enum.TryParse(opts.JoinType, true, out joinType))
                {
                    _logger.LogError("Invalid join type: {Type}. Valid values are: {ValidValues}",
                        opts.JoinType, string.Join(", ", Enum.GetNames<JoinType>()));
                    return ExitCodes.ValidationError;
                }

                // Horizontal merge는 key columns가 필수
                if (!opts.JoinKeyColumns.Any())
                {
                    _logger.LogError("Key columns must be specified for horizontal merge");
                    return ExitCodes.ValidationError;
                }
            }

            var options = new MergeOption
            {
                InputPaths = opts.InputFiles.ToList(),
                MergeType = mergeType,
                JoinType = joinType,
                JoinKeyColumns = opts.JoinKeyColumns.ToList(),
                Common = opts.GetCommonOptions()
            };

            var taskLogger = _loggerFactory.CreateLogger<MergeTask>();
            var task = new MergeTask(options, taskLogger);

            var context = new TaskContext
            {
                InputPath = opts.InputFiles.First(), // 첫 번째 파일을 primary로 사용
                OutputPath = opts.OutputPath
            };

            _logger.LogInformation("Starting merge operation with {Count} files using {Type} merge",
                opts.InputFiles.Count(), mergeType);

            var result = await task.ExecuteAsync(context);

            if (result)
            {
                _logger.LogInformation("Merge operation completed successfully");
                return ExitCodes.Success;
            }
            else
            {
                _logger.LogError("Merge operation failed");
                return ExitCodes.Error;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing merge command");
            return ExitCodes.Error;
        }
    }

    public string? GetExample() =>
    "merge file1.csv file2.csv file3.csv -t Vertical -o merged.csv\n" +
    "  merge customers1.csv customers2.csv -t Horizontal -k CustomerID -j Left -o merged_customers.csv";
}