using System.Xml.Linq;

namespace FilePrepper.Converters;

public class XmlConverter : BaseFileConverter
{
    private static readonly string[] SupportedFormats = ["xml"];

    public XmlConverter(ILogger<XmlConverter> logger, IEnumerable<IConversionPipeline> pipelines)
        : base(logger, pipelines) { }

    public override bool CanHandle(string sourceFormat)
        => SupportedFormats.Contains(sourceFormat.ToLower());

    protected override async Task<Stream> ConvertToIntermediateFormatAsync(
        Stream inputStream,
        ConversionOptions options,
        CancellationToken cancellationToken)
    {
        var doc = await XDocument.LoadAsync(inputStream, LoadOptions.None, cancellationToken);
        var rootElement = doc.Root ?? throw new FilePreppingException("XML document is empty");

        // 반복되는 요소 찾기 (루트의 직계 자식들)
        var repeatingElements = rootElement.Elements().ToList();
        if (!repeatingElements.Any())
        {
            throw new FilePreppingException("No repeating elements found in XML");
        }

        // 첫 번째 반복 요소의 이름을 기준으로 모든 같은 이름의 요소를 수집
        var elementName = repeatingElements[0].Name.LocalName;
        repeatingElements = rootElement.Elements(elementName).ToList();

        // 모든 가능한 컬럼 수집
        var columns = new HashSet<string>();
        foreach (var element in repeatingElements)
        {
            CollectColumns(element, elementName, columns);
        }

        var outputStream = new MemoryStream();
        await using var writer = new StreamWriter(outputStream, options.OutputEncoding, leaveOpen: true);

        // 컬럼을 정렬하여 일관된 순서 보장
        var orderedColumns = columns.OrderBy(x => x).ToList();

        // Write headers
        if (options.IncludeHeaders)
        {
            await writer.WriteLineAsync(string.Join(options.Delimiter, orderedColumns));
        }

        // Write data rows
        foreach (var element in repeatingElements)
        {
            var values = orderedColumns.Select(column => GetValue(element, column) ?? "")
                                     .Select(EscapeCsvField);

            await writer.WriteLineAsync(string.Join(options.Delimiter, values));
        }

        await writer.FlushAsync();
        outputStream.Position = 0;
        return outputStream;
    }

    private static void CollectColumns(XElement element, string prefix, HashSet<string> columns)
    {
        // 속성 처리
        foreach (var attribute in element.Attributes())
        {
            columns.Add($"{prefix}.@{attribute.Name.LocalName}");
        }

        // 하위 요소 처리
        foreach (var child in element.Elements())
        {
            var childPath = $"{prefix}.{child.Name.LocalName}";

            // 하위 요소가 더 있는지 확인
            var grandChildren = child.Elements().ToList();
            if (!grandChildren.Any())
            {
                // 말단 요소인 경우
                columns.Add(childPath);
            }
            else
            {
                // 중첩된 요소가 있는 경우 재귀적으로 처리
                CollectColumns(child, childPath, columns);
            }
        }
    }

    private static string? GetValue(XElement element, string path)
    {
        var parts = path.Split('.');

        // 첫 번째 부분은 요소 자체의 이름이므로 제외
        var current = element;

        // 경로의 나머지 부분을 순회
        for (int i = 1; i < parts.Length - 1; i++)
        {
            current = current.Element(parts[i]);
            if (current == null) return null;
        }

        // 마지막 부분이 속성인지 요소인지 확인
        var lastPart = parts[^1];
        if (lastPart.StartsWith('@'))
        {
            return current?.Attribute(lastPart[1..])?.Value;
        }
        else
        {
            return current?.Element(lastPart)?.Value;
        }
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