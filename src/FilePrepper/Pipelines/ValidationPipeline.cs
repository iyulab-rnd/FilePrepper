namespace FilePrepper.Pipelines;

public class ValidationPipeline : IConversionPipeline
{
    private readonly ILogger<ValidationPipeline> _logger;
    private readonly int _maxFileSizeInMb;

    public ValidationPipeline(
        ILogger<ValidationPipeline> logger,
        IOptions<FilePrepperOptions> options)
    {
        _logger = logger;
        _maxFileSizeInMb = options.Value.MaxFileSizeInMb;
    }

    public Task<Stream> ProcessAsync(Stream inputStream, ConversionOptions options)
    {
        _logger.LogInformation("Starting validation pipeline");

        if (inputStream == null || !inputStream.CanRead)
        {
            throw new FilePreppingException("Input stream is null or cannot be read");
        }

        var fileSizeInMb = inputStream.Length / (1024 * 1024);
        if (fileSizeInMb > _maxFileSizeInMb)
        {
            throw new FilePreppingException(
                $"File size ({fileSizeInMb}MB) exceeds maximum allowed size ({_maxFileSizeInMb}MB)");
        }

        return Task.FromResult(inputStream);
    }
}
