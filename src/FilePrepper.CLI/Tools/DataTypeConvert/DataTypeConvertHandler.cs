using FilePrepper.Tasks.DataTypeConvert;
using FilePrepper.Tasks;
using Microsoft.Extensions.Logging;
using System.Globalization;
using FilePrepper.CLI.Tools;

namespace FilePrepper.CLI.Tools.DataTypeConvert;

public class DataTypeConvertHandler : ICommandHandler
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<DataTypeConvertHandler> _logger;

    public DataTypeConvertHandler(
        ILoggerFactory loggerFactory,
        ILogger<DataTypeConvertHandler> logger)
    {
        _loggerFactory = loggerFactory;
        _logger = logger;
    }

    public async Task<int> ExecuteAsync(ICommandParameters parameters)
    {
        var opts = (DataTypeConvertParameters)parameters;

        try
        {
            var culture = CultureInfo.GetCultureInfo(opts.Culture);
            var conversions = new List<ColumnTypeConversion>();

            foreach (var convStr in opts.Conversions)
            {
                var parts = convStr.Split(':');
                if (parts.Length < 2 || parts.Length > 3)
                {
                    _logger.LogError("Invalid conversion format: {Conversion}. Expected format: column:type[:format]", convStr);
                    return 1;
                }

                if (!Enum.TryParse<DataType>(parts[1], true, out var dataType))
                {
                    _logger.LogError("Invalid data type: {Type}. Valid values are: {ValidValues}",
                        parts[1], string.Join(", ", Enum.GetNames<DataType>()));
                    return 1;
                }

                // DateTime 타입은 포맷이 필요
                if (dataType == DataType.DateTime && parts.Length != 3)
                {
                    _logger.LogError("DateTime format must be specified for DateTime type: {Conversion}", convStr);
                    return 1;
                }

                conversions.Add(new ColumnTypeConversion
                {
                    ColumnName = parts[0],
                    TargetType = dataType,
                    DateTimeFormat = dataType == DataType.DateTime ? parts[2] : null,
                    Culture = culture
                });
            }

            var options = new DataTypeConvertOption
            {
                Conversions = conversions,
                Common = opts.GetCommonOptions()
            };

            var taskLogger = _loggerFactory.CreateLogger<DataTypeConvertTask>();
            var task = new DataTypeConvertTask(taskLogger);
            var context = new TaskContext(options)
            {

                InputPath = opts.InputPath,
                OutputPath = opts.OutputPath
            };

            return await task.ExecuteAsync(context) ? 0 : 1;
        }
        catch (CultureNotFoundException)
        {
            _logger.LogError("Invalid culture: {Culture}", opts.Culture);
            return 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing convert-type command");
            return 1;
        }
    }

    public string? GetExample() =>
    "convert-type -i input.csv -o output.csv -c \"Date:DateTime:yyyy-MM-dd,Age:Integer\" --culture en-US";

}