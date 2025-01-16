using FilePrepper.CLI.Tools;
using FilePrepper.Tasks;
using FilePrepper.Tasks.DataSampling;
using Microsoft.Extensions.Logging;

namespace FilePrepper.CLI.Tools.DataSampling;

public class DataSamplingHandler : ICommandHandler
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<DataSamplingHandler> _logger;

    public DataSamplingHandler(
        ILoggerFactory loggerFactory,
        ILogger<DataSamplingHandler> logger)
    {
        _loggerFactory = loggerFactory;
        _logger = logger;
    }

    public async Task<int> ExecuteAsync(ICommandParameters parameters)
    {
        var opts = (DataSamplingParameters)parameters;

        try
        {
            // Validate sampling method
            if (!Enum.TryParse<SamplingMethod>(opts.Method, true, out var samplingMethod))
            {
                _logger.LogError("Invalid sampling method: {Method}. Valid values are: {ValidValues}",
                    opts.Method, string.Join(", ", Enum.GetNames<SamplingMethod>()));
                return 1;
            }

            // Validate stratify column when needed
            if (samplingMethod == SamplingMethod.Stratified && string.IsNullOrWhiteSpace(opts.StratifyColumn))
            {
                _logger.LogError("Stratify column is required when using Stratified sampling method");
                return 1;
            }

            // Validate systematic interval when needed
            if (samplingMethod == SamplingMethod.Systematic && (!opts.SystematicInterval.HasValue || opts.SystematicInterval.Value <= 0))
            {
                _logger.LogError("Valid systematic interval is required when using Systematic sampling method");
                return 1;
            }

            var options = new DataSamplingOption
            {
                Method = samplingMethod,
                SampleSize = opts.SampleSize,
                Seed = opts.Seed,
                StratifyColumn = opts.StratifyColumn,
                SystematicInterval = opts.SystematicInterval,
                Common = opts.GetCommonOptions()
            };

            var taskLogger = _loggerFactory.CreateLogger<DataSamplingTask>();
            var task = new DataSamplingTask(options, taskLogger);
            var context = new TaskContext
            {
                InputPath = opts.InputPath,
                OutputPath = opts.OutputPath
            };

            return await task.ExecuteAsync(context) ? 0 : 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing data-sampling command");
            return 1;
        }
    }

    public string? GetExample() =>
    "data-sampling -i input.csv -o output.csv -m Stratified --stratify Category -s 0.3 --seed 42";
}
