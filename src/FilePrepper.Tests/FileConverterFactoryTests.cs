using FilePrepper.Converters;
using FilePrepper.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace FilePrepper.Tests;

public class FileConverterFactoryTests
{
    [Fact]
    public void GetConverter_Should_Return_Correct_Converter()
    {
        // Arrange
        var services = new ServiceCollection();

        // Mock 로거 팩토리 등록
        var loggerFactory = new Mock<ILoggerFactory>();
        services.AddSingleton(loggerFactory.Object);

        // 변환기들 등록
        services.AddTransient(sp => Mock.Of<ILogger<ExcelConverter>>());
        services.AddTransient<IFileConverter, ExcelConverter>();
        services.AddTransient(sp => Mock.Of<ILogger<JsonConverter>>());
        services.AddTransient<IFileConverter, JsonConverter>();
        services.AddTransient(sp => Mock.Of<ILogger<XmlConverter>>());
        services.AddTransient<IFileConverter, XmlConverter>();

        var serviceProvider = services.BuildServiceProvider();
        var factory = new FileConverterFactory(serviceProvider);

        // Act & Assert
        var excelConverter = factory.GetConverter("xlsx");
        Assert.IsType<ExcelConverter>(excelConverter);

        var jsonConverter = factory.GetConverter("json");
        Assert.IsType<JsonConverter>(jsonConverter);

        var xmlConverter = factory.GetConverter("xml");
        Assert.IsType<XmlConverter>(xmlConverter);
    }

    [Fact]
    public void GetConverter_Should_Throw_Exception_For_Unsupported_Format()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var factory = new FileConverterFactory(serviceProvider);

        // Act & Assert
        Assert.Throws<FilePreppingException>(() => factory.GetConverter("unsupported"));
    }
}