using FilePrepper.CLI.Options;
using FilePrepper.Core;
using FilePrepper.Pipelines;
using Microsoft.Extensions.DependencyInjection;

namespace FilePrepper.CLI.Handlers;

public static class PreprocessHandler
{
    public static async Task<int> HandleAsync(PreprocessOptions opts, IServiceProvider serviceProvider)
    {
        if (!File.Exists(opts.InputFile))
        {
            Console.Error.WriteLine($"Error: Input file '{opts.InputFile}' does not exist.");
            return 1;
        }

        string outputFilePath = Utils.GetOutputFilePath(opts.Output, opts.InputFile);
        var outputEncoding = Utils.GetEncoding(opts.Encoding);

        var preprocessingPipeline = serviceProvider.GetRequiredService<MlPreprocessingPipeline>();
        var options = new ConversionOptions
        {
            Delimiter = opts.Delimiter,
            IncludeHeaders = !opts.NoHeader,
            OutputEncoding = outputEncoding
        };

        Console.WriteLine($"Preprocessing {opts.InputFile}...");
        using var inputStream = File.OpenRead(opts.InputFile);
        var processedStream = await preprocessingPipeline.ProcessAsync(inputStream, options);

        using var outputStream = File.Create(outputFilePath);
        await processedStream.CopyToAsync(outputStream);

        Console.WriteLine($"Preprocessing completed successfully.");
        Console.WriteLine($"Output file: {outputFilePath}");
        return 0;
    }
}