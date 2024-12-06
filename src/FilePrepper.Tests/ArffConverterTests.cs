using FilePrepper.Converters;
using FilePrepper.Core;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using Xunit.Abstractions;

namespace FilePrepper.Tests;

public class ArffConverterTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly string _testDirectory;

    public ArffConverterTests(ITestOutputHelper output)
    {
        _output = output;
        _testDirectory = Path.Combine(Path.GetTempPath(), "FilePrepper_Tests_" + Guid.NewGuid());
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        // 테스트 종료 후 테스트 디렉토리 정리
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task ArffConverter_Should_Create_Csv_File_From_Arff_File()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ArffConverter>>();
        var converter = new ArffConverter(loggerMock.Object, []);

        var arffContent = """
        @relation weather

        @attribute outlook {sunny, rainy, cloudy}
        @attribute temperature numeric
        @attribute humidity numeric
        @attribute windy {true, false}
        @attribute play {yes, no}

        @data
        sunny,85,85,false,no
        rainy,70,96,false,yes
        """;

        var arffPath = Path.Combine(_testDirectory, "test.arff");
        var csvPath = Path.Combine(_testDirectory, "test.csv");

        await File.WriteAllTextAsync(arffPath, arffContent);

        // Act
        using (var inputStream = File.OpenRead(arffPath))
        {
            var options = new ConversionOptions
            {
                Delimiter = ",",
                IncludeHeaders = true,
                OutputEncoding = Encoding.UTF8
            };

            var result = await converter.ConvertToCsvAsync(inputStream, options);

            // 변환 결과를 파일로 저장
            Assert.True(result.Success);
            Assert.NotNull(result.ResultStream);

            using var fileStream = File.Create(csvPath);
            await result.ResultStream.CopyToAsync(fileStream);
        }

        // Assert
        Assert.True(File.Exists(csvPath));
        var csvContent = await File.ReadAllTextAsync(csvPath);
        var lines = csvContent.Split('\n')
            .Select(line => line.TrimEnd('\r'))
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();

        _output.WriteLine("Generated CSV file content:");
        _output.WriteLine(csvContent);
        _output.WriteLine($"File location: {csvPath}");

        Assert.Equal(3, lines.Length);
        Assert.Equal("outlook,temperature,humidity,windy,play", lines[0]);
        Assert.Equal("sunny,85,85,false,no", lines[1]);
        Assert.Equal("rainy,70,96,false,yes", lines[2]);
    }

    [Fact]
    public async Task ArffConverter_Should_Create_Csv_File_With_Special_Characters()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ArffConverter>>();
        var converter = new ArffConverter(loggerMock.Object, []);

        var arffContent = """
        @relation employees

        @attribute name string
        @attribute description string

        @data
        "John Doe","Software, Engineer"
        "Jane Smith","Project, Manager"
        """;

        var arffPath = Path.Combine(_testDirectory, "employees.arff");
        var csvPath = Path.Combine(_testDirectory, "employees.csv");

        await File.WriteAllTextAsync(arffPath, arffContent);

        // Act
        using (var inputStream = File.OpenRead(arffPath))
        {
            var options = new ConversionOptions
            {
                Delimiter = ",",
                IncludeHeaders = true,
                OutputEncoding = Encoding.UTF8
            };

            var result = await converter.ConvertToCsvAsync(inputStream, options);

            // 변환 결과를 파일로 저장
            Assert.True(result.Success);
            Assert.NotNull(result.ResultStream);

            using var fileStream = File.Create(csvPath);
            await result.ResultStream.CopyToAsync(fileStream);
        }

        // Assert
        Assert.True(File.Exists(csvPath));
        var csvContent = await File.ReadAllTextAsync(csvPath);
        var lines = csvContent.Split('\n')
            .Select(line => line.TrimEnd('\r'))
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();

        _output.WriteLine("Generated CSV file content:");
        _output.WriteLine(csvContent);
        _output.WriteLine($"File location: {csvPath}");

        Assert.Equal(3, lines.Length);
        Assert.Equal("name,description", lines[0]);
        Assert.Equal("\"John Doe\",\"Software, Engineer\"", lines[1]);
        Assert.Equal("\"Jane Smith\",\"Project, Manager\"", lines[2]);
    }

    [Theory]
    [InlineData("test1.arff", "test1.csv")]
    [InlineData("path/to/test2.arff", "path/to/test2.csv")]
    public async Task ArffConverter_Should_Handle_Different_File_Paths(string inputPath, string outputPath)
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ArffConverter>>();
        var converter = new ArffConverter(loggerMock.Object, []);

        var fullInputPath = Path.Combine(_testDirectory, inputPath.Replace("/", Path.DirectorySeparatorChar.ToString()));
        var fullOutputPath = Path.Combine(_testDirectory, outputPath.Replace("/", Path.DirectorySeparatorChar.ToString()));

        // 입력 파일의 디렉토리가 없으면 생성
        Directory.CreateDirectory(Path.GetDirectoryName(fullInputPath)!);

        var arffContent = """
        @relation test
        @attribute name string
        @data
        "Test Data"
        """;

        await File.WriteAllTextAsync(fullInputPath, arffContent);

        // Act
        using (var inputStream = File.OpenRead(fullInputPath))
        {
            var options = new ConversionOptions
            {
                Delimiter = ",",
                IncludeHeaders = true,
                OutputEncoding = Encoding.UTF8
            };

            var result = await converter.ConvertToCsvAsync(inputStream, options);

            // 출력 파일의 디렉토리가 없으면 생성
            Directory.CreateDirectory(Path.GetDirectoryName(fullOutputPath)!);

            // 변환 결과를 파일로 저장
            Assert.True(result.Success);
            Assert.NotNull(result.ResultStream);

            using var fileStream = File.Create(fullOutputPath);
            await result.ResultStream.CopyToAsync(fileStream);
        }

        // Assert
        Assert.True(File.Exists(fullOutputPath));
        var content = await File.ReadAllTextAsync(fullOutputPath);
        Assert.Contains("name", content);
        Assert.Contains("Test Data", content);
    }

    [Fact]
    public async Task ArffConverter_Should_Convert_Simple_Arff_To_Csv()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ArffConverter>>();
        var converter = new ArffConverter(loggerMock.Object, []);

        var arff = """
        @relation weather

        @attribute outlook {sunny, rainy, cloudy}
        @attribute temperature numeric
        @attribute humidity numeric
        @attribute windy {true, false}
        @attribute play {yes, no}

        @data
        sunny,85,85,false,no
        rainy,70,96,false,yes
        """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(arff));
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

        _output.WriteLine("Raw CSV content:");
        _output.WriteLine(csvContent);

        var lines = csvContent.Split('\n')
            .Select(line => line.TrimEnd('\r'))
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();

        Assert.Equal(3, lines.Length);
        Assert.Equal("outlook,temperature,humidity,windy,play", lines[0]);
        Assert.Equal("sunny,85,85,false,no", lines[1]);
        Assert.Equal("rainy,70,96,false,yes", lines[2]);
    }

    [Fact]
    public async Task ArffConverter_Should_Handle_Quoted_And_Special_Characters()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ArffConverter>>();
        var converter = new ArffConverter(loggerMock.Object, []);

        var arff = """
        @relation test

        @attribute name string
        @attribute description string

        @data
        "John Doe","Software, Engineer"
        "Jane Smith","Project, Manager"
        """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(arff));
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
        var lines = csvContent.Split('\n')
            .Select(line => line.TrimEnd('\r'))
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();

        Assert.Equal(3, lines.Length);
        Assert.Equal("name,description", lines[0]);
        Assert.Equal("\"John Doe\",\"Software, Engineer\"", lines[1]);
        Assert.Equal("\"Jane Smith\",\"Project, Manager\"", lines[2]);
    }

    [Fact]
    public async Task ArffConverter_Should_Skip_Comments_And_Empty_Lines()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ArffConverter>>();
        var converter = new ArffConverter(loggerMock.Object, []);

        var arff = """
        % This is a comment
        @relation test

        % Another comment
        @attribute name string
        @attribute age numeric

        % Data section starts here
        @data
        
        John,30
        % Comment in data section
        Jane,25
        """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(arff));
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
        var lines = csvContent.Split('\n')
            .Select(line => line.TrimEnd('\r'))
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();

        Assert.Equal(3, lines.Length);
        Assert.Equal("name,age", lines[0]);
        Assert.Equal("John,30", lines[1]);
        Assert.Equal("Jane,25", lines[2]);
    }
}