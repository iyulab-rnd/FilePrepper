using FilePrepper.CLI.Options;
using FilePrepper.Core;
using Microsoft.Extensions.DependencyInjection;

namespace FilePrepper.CLI.Handlers;

public static class ConvertHandler
{
    public static async Task<int> HandleAsync(ConvertOptions opts, IServiceProvider serviceProvider)
    {
        if (!File.Exists(opts.InputFile))
        {
            Console.Error.WriteLine($"Error: Input file '{opts.InputFile}' does not exist.");
            return 1;
        }

        string outputFilePath = Utils.GetOutputFilePath(opts.Output, opts.InputFile);
        var outputEncoding = Utils.GetEncoding(opts.Encoding);

        var converterFactory = serviceProvider.GetRequiredService<IFileConverterFactory>();
        var extension = Path.GetExtension(opts.InputFile).TrimStart('.').ToLower();
        var converter = converterFactory.GetConverter(extension);

        var options = new ConversionOptions
        {
            Delimiter = opts.Delimiter,
            IncludeHeaders = !opts.NoHeader,
            OutputEncoding = outputEncoding
        };

        Console.WriteLine($"Converting {opts.InputFile} to {outputFilePath}...");
        using var inputStream = File.OpenRead(opts.InputFile);
        var result = await converter.ConvertToCsvAsync(inputStream, options);

        if (!result.Success)
        {
            Console.Error.WriteLine($"Error during conversion: {result.ErrorMessage}");
            return 1;
        }

        using var outputStream = File.Create(outputFilePath);
        await result.ResultStream!.CopyToAsync(outputStream);

        Console.WriteLine($"Conversion completed successfully.");
        Console.WriteLine($"Output file: {outputFilePath}");
        return 0;
    }
}
