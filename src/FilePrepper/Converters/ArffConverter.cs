using System.Text;
using System.Text.RegularExpressions;

namespace FilePrepper.Converters;

public class ArffConverter(ILogger<ArffConverter> logger, IEnumerable<IConversionPipeline> pipelines)
    : BaseFileConverter(logger, pipelines)
{
    private static readonly string[] SupportedFormats = ["arff"];
    private static readonly Regex AttributeRegex = new(@"@attribute\s+([^\s]+)\s+(.+)", RegexOptions.IgnoreCase);

    public override bool CanHandle(string sourceFormat)
        => SupportedFormats.Contains(sourceFormat.ToLower());

    protected override async Task<Stream> ConvertToIntermediateFormatAsync(
        Stream inputStream,
        ConversionOptions options,
        CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(inputStream);
        var outputStream = new MemoryStream();
        await using var writer = new StreamWriter(outputStream, options.OutputEncoding, leaveOpen: true);

        var attributes = new List<string>();
        var isDataSection = false;
        string? line;

        while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            line = line.Trim();

            // Skip empty lines and comments
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('%'))
                continue;

            // Check if we've reached the data section
            if (line.StartsWith("@data", StringComparison.OrdinalIgnoreCase))
            {
                isDataSection = true;
                if (options.IncludeHeaders)
                {
                    await writer.WriteLineAsync(string.Join(options.Delimiter, attributes));
                }
                continue;
            }

            // Parse attribute definitions
            if (!isDataSection && line.StartsWith("@attribute", StringComparison.OrdinalIgnoreCase))
            {
                var match = AttributeRegex.Match(line);
                if (match.Success)
                {
                    attributes.Add(match.Groups[1].Value);
                }
                continue;
            }

            // Process data lines
            if (isDataSection && !string.IsNullOrWhiteSpace(line))
            {
                var values = ParseArffDataLine(line);
                await writer.WriteLineAsync(string.Join(options.Delimiter, values));
            }
        }

        await writer.FlushAsync(cancellationToken);
        outputStream.Position = 0;
        return outputStream;
    }

    private static List<string> ParseArffDataLine(string line)
    {
        var values = new List<string>();
        var currentValue = new StringBuilder();
        var inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                if (!inQuotes)
                {
                    // 새로운 따옴표 시작
                    if (currentValue.Length == 0)
                    {
                        currentValue.Append('"');
                        inQuotes = true;
                    }
                }
                else
                {
                    // 따옴표 종료 또는 이스케이프된 따옴표
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        // 이스케이프된 따옴표
                        currentValue.Append("\"\"");
                        i++; // 다음 따옴표 건너뛰기
                    }
                    else
                    {
                        // 따옴표 종료
                        currentValue.Append('"');
                        inQuotes = false;
                    }
                }
            }
            else if (c == ',' && !inQuotes)
            {
                // 필드 구분자를 만났을 때
                values.Add(currentValue.ToString());
                currentValue.Clear();
            }
            else
            {
                currentValue.Append(c);
            }
        }

        // 마지막 값 추가
        if (currentValue.Length > 0)
        {
            values.Add(currentValue.ToString());
        }

        return values;
    }
}