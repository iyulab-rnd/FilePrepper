using FilePrepper.Tasks;
using FilePrepper.Tasks.DateExtraction;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace FilePrepper.CLI.Tools.DateExtraction;

public class DateExtractionHandler : BaseCommandHandler<DateExtractionParameters>
{
    public DateExtractionHandler(
        ILoggerFactory loggerFactory,
        ILogger<DateExtractionHandler> logger)
        : base(loggerFactory, logger)
    {
    }

    public override async Task<int> ExecuteAsync(ICommandParameters parameters)
    {
        var opts = (DateExtractionParameters)parameters;
        if (!ValidateParameters(opts))
        {
            return ExitCodes.InvalidArguments;
        }

        return await HandleExceptionAsync(async () =>
        {
            var culture = CultureInfo.GetCultureInfo(opts.Culture);
            var extractions = new List<DateColumnExtraction>();

            foreach (var extractStr in opts.Extractions)
            {
                var parts = extractStr.Split(':');

                var components = new List<DateComponent>();
                foreach (var comp in parts[1].Split(','))
                {
                    if (Enum.TryParse<DateComponent>(comp, true, out var component))
                    {
                        components.Add(component);
                    }
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
                InputPath = opts.InputPath,
                OutputPath = opts.OutputPath,
                Extractions = extractions,
                HasHeader = opts.HasHeader,
                IgnoreErrors = opts.IgnoreErrors,
                AppendToSource = opts.AppendToSource,
                OutputColumnTemplate = opts.OutputColumnTemplate
            };

            var taskLogger = _loggerFactory.CreateLogger<DateExtractionTask>();
            var task = new DateExtractionTask(taskLogger);
            var context = new TaskContext(options);

            var success = await task.ExecuteAsync(context);
            return success ? ExitCodes.Success : ExitCodes.Error;
        });
    }
}