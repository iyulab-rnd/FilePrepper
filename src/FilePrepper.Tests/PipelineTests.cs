using FilePrepper.Core;
using FilePrepper.Pipelines;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Text;

namespace FilePrepper.Tests;

public class PipelineTests
{
    [Fact]
    public async Task ValidationPipeline_Should_Validate_File_Size()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ValidationPipeline>>();
        var optionsMock = new Mock<IOptions<FilePrepperOptions>>();
        optionsMock.Setup(x => x.Value).Returns(new FilePrepperOptions { MaxFileSizeInMb = 1 });

        var pipeline = new ValidationPipeline(loggerMock.Object, optionsMock.Object);

        // 2MB 크기의 스트림 생성
        var largeStream = new MemoryStream(new byte[2 * 1024 * 1024]);

        // Act & Assert
        await Assert.ThrowsAsync<FilePreppingException>(() =>
            pipeline.ProcessAsync(largeStream, new ConversionOptions()));
    }

    [Fact]
    public async Task ValidationPipeline_Should_Pass_Valid_Stream()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ValidationPipeline>>();
        var optionsMock = new Mock<IOptions<FilePrepperOptions>>();
        optionsMock.Setup(x => x.Value).Returns(new FilePrepperOptions { MaxFileSizeInMb = 1 });

        var pipeline = new ValidationPipeline(loggerMock.Object, optionsMock.Object);

        // 0.5MB 크기의 스트림 생성
        var validStream = new MemoryStream(new byte[512 * 1024]);

        // Act
        var result = await pipeline.ProcessAsync(validStream, new ConversionOptions());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(validStream, result);
    }

    [Fact]
    public async Task DataSanitizationPipeline_Should_Clean_Data()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<DataSanitizationPipeline>>();
        var pipeline = new DataSanitizationPipeline(loggerMock.Object);

        var dirtyData = "Column1,Column2\r\nNULL,  Data  \r\n";
        using var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(dirtyData));
        var options = new ConversionOptions();

        // Act
        var resultStream = await pipeline.ProcessAsync(inputStream, options);

        // Assert
        Assert.NotNull(resultStream);
        using var reader = new StreamReader(resultStream);
        var cleanedData = await reader.ReadToEndAsync();

        Assert.DoesNotContain("NULL", cleanedData);
        Assert.Contains("Data", cleanedData);
        Assert.DoesNotContain("  Data  ", cleanedData); // 앞뒤 공백이 제거되어야 함
    }
}
