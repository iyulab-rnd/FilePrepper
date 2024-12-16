using FilePrepper.Pipelines;
using Microsoft.Extensions.DependencyInjection;

namespace FilePrepper;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFilePrepper(
    this IServiceCollection services,
    Action<FilePrepperOptions>? configureOptions = null)
    {
        var options = new FilePrepperOptions();
        configureOptions?.Invoke(options);

        services.AddSingleton<IFileConverterFactory, FileConverterFactory>();

        // Register all converters
        services.Scan(scan => scan
            .FromAssemblyOf<IFileConverter>()
            .AddClasses(classes => classes.AssignableTo<IFileConverter>())
            .AsImplementedInterfaces()
            .WithTransientLifetime());

        // Register all pipelines
        services.Scan(scan => scan
            .FromAssemblyOf<IConversionPipeline>()
            .AddClasses(classes => classes.AssignableTo<IConversionPipeline>())
            .AsImplementedInterfaces()
            .WithTransientLifetime());

        // CsvMergePipeline을 명시적으로 등록
        services.AddTransient<CsvMergePipeline>();

        return services;
    }


}
