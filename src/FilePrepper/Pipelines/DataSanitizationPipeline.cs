namespace FilePrepper.Pipelines;

public class DataSanitizationPipeline : IConversionPipeline
{
    private readonly ILogger<DataSanitizationPipeline> _logger;

    public DataSanitizationPipeline(ILogger<DataSanitizationPipeline> logger)
    {
        _logger = logger;
    }

    public async Task<Stream> ProcessAsync(Stream inputStream, ConversionOptions options)
    {
        _logger.LogInformation("Starting data sanitization pipeline");

        var reader = new StreamReader(inputStream, options.OutputEncoding);
        var outputStream = new MemoryStream();
        var writer = new StreamWriter(outputStream, options.OutputEncoding, leaveOpen: true);

        var uniqueRows = new HashSet<string>();
        string? line;
        int lineNumber = 0;

        while ((line = await reader.ReadLineAsync()) != null)
        {
            lineNumber++;
            line = SanitizeLine(line);

            // Skip empty lines
            if (string.IsNullOrWhiteSpace(line))
                continue;

            // Skip duplicate rows (if header row, allow it once)
            if (uniqueRows.Contains(line) && lineNumber > 1)
                continue;

            uniqueRows.Add(line);
            await writer.WriteLineAsync(line);
        }

        await writer.FlushAsync();
        outputStream.Position = 0;
        return outputStream;
    }

    private string SanitizeLine(string line)
    {
        // Remove control characters
        line = new string(line.Where(c => !char.IsControl(c) || c == '\t').ToArray());

        // Trim leading/trailing spaces
        line = line.Trim();

        // Replace NULL or NaN with an empty string
        line = line.Replace("NULL", "").Replace("NaN", "");

        return line;
    }
}
