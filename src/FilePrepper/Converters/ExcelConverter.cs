using OfficeOpenXml;
using System.Xml.Linq;

namespace FilePrepper.Converters;

public class ExcelConverter(ILogger<ExcelConverter> logger, IEnumerable<IConversionPipeline> pipelines)
    : BaseFileConverter(logger, pipelines)
{
    private static readonly string[] SupportedFormats = ["xlsx", "xls"];

    public override bool CanHandle(string sourceFormat)
        => SupportedFormats.Contains(sourceFormat.ToLower());

    // ExcelConverter.cs의 ConvertToIntermediateFormatAsync 메서드 수정
    protected override async Task<Stream> ConvertToIntermediateFormatAsync(
        Stream inputStream,
        ConversionOptions options,
        CancellationToken cancellationToken)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage(inputStream);
        var worksheet = package.Workbook.Worksheets[0];
        var usedRange = worksheet.Dimension;

        if (usedRange == null)
        {
            throw new FilePreppingException("Excel worksheet is empty");
        }

        var outputStream = new MemoryStream();
        await using var writer = new StreamWriter(outputStream, options.OutputEncoding, leaveOpen: true);

        var isFirstLine = true;

        // Write headers if needed (한 번만 작성)
        if (options.IncludeHeaders)
        {
            var headerValues = new List<string>();
            for (int col = 1; col <= usedRange.End.Column; col++)
            {
                var value = worksheet.Cells[1, col].Text;
                headerValues.Add(EscapeCsvField(value));
            }
            await writer.WriteLineAsync(string.Join(options.Delimiter, headerValues));
        }

        // Write data starting from row 2
        for (int row = 2; row <= usedRange.End.Row; row++)
        {
            if (cancellationToken.IsCancellationRequested) break;

            var rowValues = new List<string>();
            for (int col = 1; col <= usedRange.End.Column; col++)
            {
                var value = worksheet.Cells[row, col].Text;
                rowValues.Add(EscapeCsvField(value));
            }
            await writer.WriteLineAsync(string.Join(options.Delimiter, rowValues));
        }

        await writer.FlushAsync();
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