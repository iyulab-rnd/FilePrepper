using FilePrepper.Tasks;
using FilePrepper.Tasks.DataTypeConvert;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace FilePrepper.CLI.Tools.DataTypeConvert;

public class DataTypeConvertHandler : BaseCommandHandler<DataTypeConvertParameters>
{
    public DataTypeConvertHandler(
        ILoggerFactory loggerFactory,
        ILogger<DataTypeConvertHandler> logger)
        : base(loggerFactory, logger)
    {
    }

    public override async Task<int> ExecuteAsync(ICommandParameters parameters)
    {
        var opts = (DataTypeConvertParameters)parameters;
        if (!ValidateParameters(opts))
        {
            return ExitCodes.InvalidArguments;
        }

        return await HandleExceptionAsync(async () =>
        {
            var culture = CultureInfo.GetCultureInfo(opts.Culture);
            var conversions = new List<ColumnTypeConversion>();

            foreach (var convStr in opts.Conversions)
            {
                var parts = convStr.Split(':');
                if (!Enum.TryParse<DataType>(parts[1], true, out var dataType))
                {
                    _logger.LogError("Invalid data type: {Type}. Valid values are: {ValidValues}",
                        parts[1], string.Join(", ", Enum.GetNames<DataType>()));
                    return ExitCodes.InvalidArguments;
                }

                conversions.Add(new ColumnTypeConversion
                {
                    ColumnName = parts[0],
                    TargetType = dataType,
                    DateTimeFormat = dataType == DataType.DateTime ? parts[2] : null,
                    Culture = culture,
                    TrimWhitespace = true,
                    IgnoreCase = true
                });
            }

            var options = new DataTypeConvertOption
            {
                InputPath = opts.InputPath,
                OutputPath = opts.OutputPath,
                Conversions = conversions,
                DefaultValue = opts.DefaultValue,
                HasHeader = opts.HasHeader,
                IgnoreErrors = opts.IgnoreErrors
            };

            var taskLogger = _loggerFactory.CreateLogger<DataTypeConvertTask>();
            var task = new DataTypeConvertTask(taskLogger);
            var context = new TaskContext(options);

            var success = await task.ExecuteAsync(context);
            return success ? ExitCodes.Success : ExitCodes.Error;
        });
    }

    public override string? GetExample() =>
        "convert-type -i input.csv -o output.csv -c \"Date:DateTime:yyyy-MM-dd,Age:Integer\" --culture en-US";
}