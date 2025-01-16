using FilePrepper.Tasks.DateExtraction;
using FilePrepper.Tasks;
using Microsoft.Extensions.Logging;
using System.Globalization;
using FilePrepper.CLI.Tools;

namespace FilePrepper.CLI.Tools.DateExtraction;

public class DateExtractionHandler : ICommandHandler
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<DateExtractionHandler> _logger;

    public DateExtractionHandler(
        ILoggerFactory loggerFactory,
        ILogger<DateExtractionHandler> logger)
    {
        _loggerFactory = loggerFactory;
        _logger = logger;
    }

    public async Task<int> ExecuteAsync(ICommandParameters parameters)
    {
        var opts = (DateExtractionParameters)parameters;

        try
        {
            var culture = CultureInfo.GetCultureInfo(opts.Culture);
            var extractions = new List<DateColumnExtraction>();

            foreach (var extractStr in opts.Extractions)
            {
                var parts = extractStr.Split(':');
                if (parts.Length < 2 || parts.Length > 3)
                {
                    _logger.LogError("Invalid extraction format: {Extraction}. Expected format: column:component1,component2[:format]", extractStr);
                    return 1;
                }

                var components = new List<DateComponent>();
                foreach (var comp in parts[1].Split(','))
                {
                    if (!Enum.TryParse<DateComponent>(comp, true, out var component))
                    {
                        _logger.LogError("Invalid date component: {Component}. Valid values are: {ValidValues}",
                            comp, string.Join(", ", Enum.GetNames<DateComponent>()));
                        return 1;
                    }
                    components.Add(component);
                }

                extractions.Add(new DateColumnExtraction
                {
                    SourceColumn = parts[0],
                    Components = components,
                    DateFormat = parts.Length > 2 ? parts[2] : null,
                    Culture = culture,
                    OutputColumnTemplate = opts.OutputColumnTemplate
                });
            }

            var options = new DateExtractionOption
            {
                Extractions = extractions,
                Common = opts.GetCommonOptions()
            };

            if (!options.Common.Output.AppendToSource)
            {
                options.Common.Output.OutputColumnTemplate = opts.OutputColumnTemplate;
            }

            var taskLogger = _loggerFactory.CreateLogger<DateExtractionTask>();
            var task = new DateExtractionTask(options, taskLogger);
            var context = new TaskContext
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
            _logger.LogError(ex, "Error executing extract-date command");
            return 1;
        }
    }

    public string? GetExample() =>
    "extract-date -i input.csv -o output.csv -e \"OrderDate:Year,Month,Day:yyyy-MM-dd\"";
}