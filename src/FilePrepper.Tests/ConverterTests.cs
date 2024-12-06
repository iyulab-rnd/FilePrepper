using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using OfficeOpenXml;
using FilePrepper.Converters;
using FilePrepper.Core;
using Xunit.Abstractions;

namespace FilePrepper.Tests;

public class ConverterTests
{
    private readonly ITestOutputHelper _output;

    public ConverterTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task ExcelConverter_Should_Convert_Excel_To_Csv()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ExcelConverter>>();
        var converter = new ExcelConverter(loggerMock.Object, Array.Empty<IConversionPipeline>());

        var excelBytes = CreateExcelFile();
        using var stream = new MemoryStream(excelBytes);

        var options = new ConversionOptions
        {
            Delimiter = ",",
            IncludeHeaders = true,
            OutputEncoding = Encoding.UTF8
        };

        // Act
        var result = await converter.ConvertToCsvAsync(stream, options);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.ResultStream);

        using var reader = new StreamReader(result.ResultStream);
        var csvContent = await reader.ReadToEndAsync();

        _output.WriteLine("=== Excel Converter Test Debug ===");
        _output.WriteLine("Raw CSV content (showing line breaks as \\n):");
        _output.WriteLine(csvContent.Replace("\n", "\\n"));
        _output.WriteLine("\nBytes:");
        _output.WriteLine(BitConverter.ToString(Encoding.UTF8.GetBytes(csvContent)));

        var lines = csvContent.Split('\n', StringSplitOptions.None)
                        .Select((line, index) => {
                            _output.WriteLine($"Line {index} (length: {line.Length}): '{line}'");
                            return line.TrimEnd('\r');
                        })
                        .ToArray();

        _output.WriteLine($"\nTotal lines (before filtering): {lines.Length}");
        lines = lines.Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
        _output.WriteLine($"Total lines (after filtering): {lines.Length}");

        Assert.Equal(2, lines.Length);
        Assert.Equal("Name,Age", lines[0]);
        Assert.Equal("John,30", lines[1]);
    }

    [Fact]
    public async Task XmlConverter_Should_Convert_Xml_To_Csv()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<XmlConverter>>();
        var converter = new XmlConverter(loggerMock.Object, Array.Empty<IConversionPipeline>());

        var xml = """
        <?xml version="1.0" encoding="UTF-8"?>
        <root>
            <person>
                <name>John</name>
                <age>30</age>
                <details type="employee">
                    <department>IT</department>
                </details>
            </person>
            <person>
                <name>Jane</name>
                <age>25</age>
                <details type="contractor">
                    <department>HR</department>
                </details>
            </person>
        </root>
        """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        var options = new ConversionOptions
        {
            Delimiter = ",",
            IncludeHeaders = true
        };

        // Act
        var result = await converter.ConvertToCsvAsync(stream, options);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.ResultStream);

        using var reader = new StreamReader(result.ResultStream);
        var csvContent = await reader.ReadToEndAsync();

        _output.WriteLine("=== XML Converter Test Debug ===");
        _output.WriteLine("Raw CSV content (showing line breaks as \\n):");
        _output.WriteLine(csvContent.Replace("\n", "\\n"));
        _output.WriteLine("\nBytes:");
        _output.WriteLine(BitConverter.ToString(Encoding.UTF8.GetBytes(csvContent)));

        var lines = csvContent.Split('\n', StringSplitOptions.None)
                        .Select((line, index) => {
                            _output.WriteLine($"Line {index} (length: {line.Length}): '{line}'");
                            return line.TrimEnd('\r');
                        })
                        .ToArray();

        _output.WriteLine($"\nTotal lines (before filtering): {lines.Length}");
        lines = lines.Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
        _output.WriteLine($"Total lines (after filtering): {lines.Length}");

        if (lines.Length > 0)
        {
            var headers = lines[0].Split(',');
            _output.WriteLine("\nHeader columns:");
            for (int i = 0; i < headers.Length; i++)
            {
                _output.WriteLine($"{i}: '{headers[i]}'");
            }

            if (lines.Length > 1)
            {
                var columns = lines[1].Split(',');
                _output.WriteLine("\nData columns:");
                for (int i = 0; i < columns.Length; i++)
                {
                    _output.WriteLine($"{i}: '{columns[i]}'");
                }
            }
        }

        var headerColumns = lines[0].Split(',');
        var dataColumns = lines[1].Split(',');

        Assert.Contains("person.name", headerColumns);
        Assert.Contains("person.age", headerColumns);
        Assert.Contains("person.details.@type", headerColumns);
        Assert.Contains("person.details.department", headerColumns);

        Assert.Contains("John", dataColumns);
        Assert.Contains("30", dataColumns);
        Assert.Contains("employee", dataColumns);
        Assert.Contains("IT", dataColumns);
    }

    private byte[] CreateExcelFile()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Sheet1");

        // 값 직접 설정
        worksheet.Cells[1, 1].Value = "Name";
        worksheet.Cells[1, 2].Value = "Age";
        worksheet.Cells[2, 1].Value = "John";
        worksheet.Cells[2, 2].Value = "30";

        return package.GetAsByteArray();
    }

    [Fact]
    public async Task JsonConverter_Should_Convert_Json_To_Csv()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<JsonConverter>>();
        var converter = new JsonConverter(loggerMock.Object, Array.Empty<IConversionPipeline>());

        var json = """
        [
            {"name": "John", "age": 30},
            {"name": "Jane", "age": 25}
        ]
        """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var options = new ConversionOptions
        {
            Delimiter = ",",
            IncludeHeaders = true
        };

        // Act
        var result = await converter.ConvertToCsvAsync(stream, options);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.ResultStream);

        using var reader = new StreamReader(result.ResultStream);
        var csvContent = await reader.ReadToEndAsync();
        var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                            .Select(line => line.TrimEnd('\r'))
                            .ToArray();

        Assert.Equal(3, lines.Length);
        Assert.Equal("name,age", lines[0]);
        Assert.Equal("John,30", lines[1]);
        Assert.Equal("Jane,25", lines[2]);
    }

}