using System.Text.Json;
using System.Text.Json.Nodes;

namespace FilePrepper.Converters;

public class JsonConverter(ILogger<JsonConverter> logger, IEnumerable<IConversionPipeline> pipelines)
    : BaseFileConverter(logger, pipelines)
{
    private static readonly string[] SupportedFormats = ["json"];

    public override bool CanHandle(string sourceFormat)
        => SupportedFormats.Contains(sourceFormat.ToLower());

    protected override async Task<Stream> ConvertToIntermediateFormatAsync(
        Stream inputStream,
        ConversionOptions options,
        CancellationToken cancellationToken)
    {
        using var jsonDocument = await JsonDocument.ParseAsync(inputStream, cancellationToken: cancellationToken);

        // JSON 배열이 아닌 경우 단일 객체를 배열로 변환
        var elements = jsonDocument.RootElement.ValueKind == JsonValueKind.Array
            ? [.. jsonDocument.RootElement.EnumerateArray()]
            : new List<JsonElement> { jsonDocument.RootElement };

        if (elements.Count == 0)
        {
            throw new FilePreppingException("JSON document is empty");
        }

        // 모든 가능한 속성 키 수집
        var allKeys = new HashSet<string>();
        foreach (var element in elements)
        {
            if (element.ValueKind != JsonValueKind.Object)
            {
                throw new FilePreppingException("JSON elements must be objects");
            }

            foreach (var property in element.EnumerateObject())
            {
                allKeys.Add(property.Name);
            }
        }

        var outputStream = new MemoryStream();
        await using var writer = new StreamWriter(outputStream, options.OutputEncoding, leaveOpen: true);

        // 헤더 작성
        if (options.IncludeHeaders)
        {
            var headerLine = string.Join(options.Delimiter,
                allKeys.Select(key => EscapeCsvField(key)));
            await writer.WriteLineAsync(headerLine.AsMemory(), cancellationToken);
        }

        // 데이터 작성
        foreach (var element in elements)
        {
            if (cancellationToken.IsCancellationRequested) break;

            var values = allKeys.Select(key =>
            {
                if (element.TryGetProperty(key, out var property))
                {
                    return property.ValueKind == JsonValueKind.Null
                        ? ""
                        : EscapeCsvField(property.ToString());
                }
                return "";
            });

            var dataLine = string.Join(options.Delimiter, values);
            await writer.WriteLineAsync(dataLine.AsMemory(), cancellationToken);
        }

        await writer.FlushAsync(cancellationToken);
        outputStream.Position = 0;
        return outputStream;
    }

    private static string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field)) return "";

        if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
        {
            field = field.Replace("\"", "\"\"");
            field = $"\"{field}\"";
        }
        return field;
    }
}