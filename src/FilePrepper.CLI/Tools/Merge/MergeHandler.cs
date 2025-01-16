using FilePrepper.CLI.Tools;
using FilePrepper.Tasks;
using FilePrepper.Tasks.Merge;
using Microsoft.Extensions.Logging;

namespace FilePrepper.CLI.Tools.Merge;

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
            // 옵션 객체 생성 
            var options = new MergeOption
            {
                InputPaths = opts.InputFiles.ToList(),
                MergeType = Enum.Parse<MergeType>(opts.MergeType, true),
                JoinType = Enum.Parse<JoinType>(opts.JoinType, true),
                HasHeader = opts.HasHeader,
                JoinKeyColumns = opts.JoinKeyColumns.Select(column =>
                {
                    if (int.TryParse(column, out int index))
                    {
                        return ColumnIdentifier.ByIndex(index);
                    }
                    return ColumnIdentifier.ByName(column);
                }).ToList(),
                Common = opts.GetCommonOptions()
            };

            var taskLogger = _loggerFactory.CreateLogger<MergeTask>();
            var task = new MergeTask(taskLogger);

            var context = new TaskContext(options)
            {
                InputPath = opts.InputFiles.First(),
                OutputPath = opts.OutputPath
            };

            _logger.LogInformation("Starting merge operation with {Count} files using {Type} merge",
                opts.InputFiles.Count(), options.MergeType);

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
        "fileprepper merge file1.csv file2.csv file3.csv -t Vertical -o merged.csv\n" +
        "fileprepper merge customers1.csv customers2.csv -t Horizontal -k CustomerID -j Left -o merged_customers.csv";
}