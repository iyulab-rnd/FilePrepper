using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Text;

namespace FilePrepper.Pipelines;

public class CsvMergePipeline : IConversionPipeline
{
    private readonly ILogger<CsvMergePipeline> _logger;

    public CsvMergePipeline(ILogger<CsvMergePipeline> logger)
    {
        _logger = logger;
    }

    public Task<Stream> ProcessAsync(Stream inputStream, ConversionOptions options)
    {
        throw new NotImplementedException("This method is not implemented for CsvMergePipeline. Use the overloaded method for merging two streams.");
    }

    public async Task<Stream> ProcessAsync(Stream inputStream1, Stream inputStream2, ConversionOptions options)
    {
        _logger.LogInformation("Starting CSV merge pipeline");

        var outputStream = new MemoryStream();
        await using var writer = new StreamWriter(outputStream, options.OutputEncoding, leaveOpen: true);

        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = options.Delimiter,
            Encoding = options.OutputEncoding,
            HasHeaderRecord = options.IncludeHeaders
        };

        using var reader1 = new StreamReader(inputStream1, options.OutputEncoding);
        using var reader2 = new StreamReader(inputStream2, options.OutputEncoding);

        // 헤더 읽기
        string[]? headers1 = null;
        string[]? headers2 = null;

        if (options.IncludeHeaders)
        {
            headers1 = (await reader1.ReadLineAsync())?.Split(options.Delimiter);
            headers2 = (await reader2.ReadLineAsync())?.Split(options.Delimiter);

            if (headers1 != null && headers2 != null)
            {
                await writer.WriteLineAsync(string.Join(options.Delimiter, headers1.Concat(headers2)));
            }
        }

        // 데이터 병합
        string? line1, line2;
        while ((line1 = await reader1.ReadLineAsync()) != null &&
               (line2 = await reader2.ReadLineAsync()) != null)
        {
            var values1 = ParseCsvLine(line1);
            var values2 = ParseCsvLine(line2);
            var mergedLine = string.Join(options.Delimiter, values1.Concat(values2));
            await writer.WriteLineAsync(mergedLine);
        }

        await writer.FlushAsync();
        outputStream.Position = 0;
        return outputStream;
    }

    private static List<string> ParseCsvLine(string line)
    {
        var values = new List<string>();
        var currentValue = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var currentChar = line[i];

            if (currentChar == '"')
            {
                if (!inQuotes)
                {
                    inQuotes = true;
                }
                else if (i + 1 < line.Length && line[i + 1] == '"')
                {
                    // 이스케이프된 따옴표
                    currentValue.Append('"');
                    i++; // Skip next quote
                }
                else
                {
                    inQuotes = false;
                }
            }
            else if (currentChar == ',' && !inQuotes)
            {
                values.Add(currentValue.ToString());
                currentValue.Clear();
            }
            else
            {
                currentValue.Append(currentChar);
            }
        }

        values.Add(currentValue.ToString());
        return values;
    }
}