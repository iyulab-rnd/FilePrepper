using FilePrepper.Tasks;
using FilePrepper.Tasks.AddColumns;
using Microsoft.Extensions.Logging;

namespace FilePrepper.CLI.Tools.AddColumns;

/// <summary>
/// CLI의 add-columns 명령어 핸들러
/// </summary>
public class AddColumnsHandler : BaseCommandHandler<AddColumnsParameters>
{
    public AddColumnsHandler(
        ILoggerFactory loggerFactory,
        ILogger<AddColumnsHandler> logger)
        : base(loggerFactory, logger)
    {
    }

    public override async Task<int> ExecuteAsync(ICommandParameters parameters)
    {
        var opts = (AddColumnsParameters)parameters;
        if (!ValidateParameters(opts))
        {
            return ExitCodes.InvalidArguments;
        }

        return await HandleExceptionAsync(async () =>
        {
            var columns = ParseColumnDefinitions(opts.Columns);
            if (columns == null)
            {
                return ExitCodes.InvalidArguments;
            }

            var options = new AddColumnsOption
            {
                InputPath = opts.InputPath,
                OutputPath = opts.OutputPath,
                NewColumns = columns,
                HasHeader = opts.HasHeader,
                IgnoreErrors = opts.IgnoreErrors
            };

            var taskLogger = _loggerFactory.CreateLogger<AddColumnsTask>();
            var task = new AddColumnsTask(taskLogger);
            var context = new TaskContext(options);

            _logger.LogInformation("Adding {Count} new columns to {InputPath}",
                columns.Count, opts.InputPath);

            var success = await task.ExecuteAsync(context);
            return success ? ExitCodes.Success : ExitCodes.Error;
        });
    }

    /// <summary>
    /// 컬럼 정의 문자열을 파싱하여 Dictionary로 변환
    /// </summary>
    private Dictionary<string, string>? ParseColumnDefinitions(IEnumerable<string> columnDefs)
    {
        try
        {
            var columns = new Dictionary<string, string>();

            foreach (var col in columnDefs)
            {
                var parts = col.Split('=', 2);
                if (parts.Length != 2)
                {
                    _logger.LogError("Invalid column format: {Column}. Expected format: name=value", col);
                    return null;
                }

                var columnName = parts[0].Trim();
                var columnValue = parts[1].Trim();

                if (string.IsNullOrWhiteSpace(columnName))
                {
                    _logger.LogError("Column name cannot be empty");
                    return null;
                }

                if (columns.ContainsKey(columnName))
                {
                    _logger.LogError("Duplicate column name: {Column}", columnName);
                    return null;
                }

                columns[columnName] = columnValue;
            }

            return columns;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing column definitions");
            return null;
        }
    }
}