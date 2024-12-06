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

        // CSV 포맷을 위한 특수문자 및 구분자 처리
        var reader = new StreamReader(inputStream, options.OutputEncoding);
        var outputStream = new MemoryStream();
        var writer = new StreamWriter(outputStream, options.OutputEncoding, leaveOpen: true);

        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            // 데이터 정제 작업
            line = SanitizeLine(line);
            await writer.WriteLineAsync(line);
        }

        await writer.FlushAsync();
        outputStream.Position = 0;
        return outputStream;
    }

    private string SanitizeLine(string line)
    {
        // 제어 문자 제거
        line = new string(line.Where(c => !char.IsControl(c) || c == '\t').ToArray());

        // 앞뒤 공백 제거
        line = line.Trim();

        // NULL 문자열 처리
        line = line.Replace("NULL", "");

        return line;
    }
}