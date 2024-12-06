using Microsoft.Extensions.DependencyInjection;

namespace FilePrepper;

public interface IFileConverterFactory
{
    IFileConverter GetConverter(string sourceFormat);
}

public class FileConverterFactory : IFileConverterFactory
{
    private readonly IServiceProvider _serviceProvider;

    public FileConverterFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IFileConverter GetConverter(string sourceFormat)
    {
        var converters = _serviceProvider.GetServices<IFileConverter>();

        var converter = converters.FirstOrDefault(c => c.CanHandle(sourceFormat))
            ?? throw new FilePreppingException($"No converter found for format: {sourceFormat}");

        return converter;
    }
}