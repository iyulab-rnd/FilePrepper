using FilePrepper.Core;

namespace FilePrepper.Converters;

public abstract class BaseFileConverter : IFileConverter
{
    protected readonly ILogger<BaseFileConverter> _logger;
    protected readonly IEnumerable<IConversionPipeline> _pipelines;

    protected BaseFileConverter(
        ILogger<BaseFileConverter> logger,
        IEnumerable<IConversionPipeline> pipelines)
    {
        _logger = logger;
        _pipelines = pipelines;
    }

    public abstract bool CanHandle(string sourceFormat);

    protected abstract Task<Stream> ConvertToIntermediateFormatAsync(
        Stream inputStream,
        ConversionOptions options,
        CancellationToken cancellationToken);

    public async Task<ConversionResult> ConvertToCsvAsync(
        Stream inputStream,
        ConversionOptions options,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting conversion process");

            Stream currentStream = await ConvertToIntermediateFormatAsync(inputStream, options, cancellationToken);

            foreach (var pipeline in _pipelines)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return new ConversionResult
                    {
                        Success = false,
                        ErrorMessage = "Operation cancelled by user"
                    };
                }

                currentStream = await pipeline.ProcessAsync(currentStream, options);
            }

            return new ConversionResult
            {
                Success = true,
                ResultStream = currentStream
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during conversion");
            return new ConversionResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}