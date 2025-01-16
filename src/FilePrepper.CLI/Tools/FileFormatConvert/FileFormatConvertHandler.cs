using FilePrepper.Tasks.FileFormatConvert;
using FilePrepper.Tasks;
using Microsoft.Extensions.Logging;
using System.Text;
using FilePrepper.CLI.Tools;

namespace FilePrepper.CLI.Tools.FileFormatConvert;

public class FileFormatConvertHandler : ICommandHandler
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<FileFormatConvertHandler> _logger;

    public FileFormatConvertHandler(
        ILoggerFactory loggerFactory,
        ILogger<FileFormatConvertHandler> logger)
    {
        _loggerFactory = loggerFactory;
        _logger = logger;
    }

    public async Task<int> ExecuteAsync(ICommandParameters parameters)
    {
        var opts = (FileFormatConvertParameters)parameters;

        try
        {
            // Validate and parse format
            if (!Enum.TryParse<FileFormat>(opts.TargetFormat, true, out var format))
            {
                _logger.LogError("Invalid target format: {Format}. Valid values are: {ValidValues}",
                    opts.TargetFormat, string.Join(", ", Enum.GetNames<FileFormat>()));
                return 1;
            }

            // Validate and get encoding
            Encoding encoding;
            try
            {
                encoding = Encoding.GetEncoding(opts.Encoding);
            }
            catch (ArgumentException)
            {
                _logger.LogError("Invalid encoding: {Encoding}", opts.Encoding);
                return 1;
            }

            // XML format specific validations
            if (format == FileFormat.XML)
            {
                if (string.IsNullOrWhiteSpace(opts.RootElementName))
                {
                    _logger.LogError("Root element name cannot be empty for XML format");
                    return 1;
                }
                if (string.IsNullOrWhiteSpace(opts.ItemElementName))
                {
                    _logger.LogError("Item element name cannot be empty for XML format");
                    return 1;
                }
            }

            var options = new FileFormatConvertOption
            {
                TargetFormat = format,
                Encoding = encoding,
                HasHeader = true, // CSV 파일은 항상 헤더가 있다고 가정
                PrettyPrint = opts.PrettyPrint,
                RootElementName = opts.RootElementName,
                ItemElementName = opts.ItemElementName,
                Common = opts.GetCommonOptions()
            };

            var taskLogger = _loggerFactory.CreateLogger<FileFormatConvertTask>();
            var task = new FileFormatConvertTask(taskLogger);
            var context = new TaskContext(options)
            {
                InputPath = opts.InputPath,
                OutputPath = opts.OutputPath
            };

            return await task.ExecuteAsync(context) ? 0 : 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing convert-format command");
            return 1;
        }
    }

    public string? GetExample() =>
    "convert-format -i input.csv -o output.json -t JSON --pretty";
}