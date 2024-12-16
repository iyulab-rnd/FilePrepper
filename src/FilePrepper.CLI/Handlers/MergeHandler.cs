using FilePrepper.CLI.Options;
using FilePrepper.Core;
using FilePrepper.Pipelines;
using Microsoft.Extensions.DependencyInjection;

namespace FilePrepper.CLI.Handlers;

public static class MergeHandler
{
    public static async Task<int> HandleAsync(MergeOptions opts, IServiceProvider serviceProvider)
    {
        var inputFiles = opts.InputFiles.Split(',', StringSplitOptions.RemoveEmptyEntries);
        if (inputFiles.Length != 2)
        {
            Console.Error.WriteLine("Error: Merge operation requires exactly two input files.");
            return 1;
        }

        foreach (var file in inputFiles)
        {
            if (!File.Exists(file))
            {
                Console.Error.WriteLine($"Error: Input file '{file}' does not exist.");
                return 1;
            }
        }

        string outputFilePath = Utils.GetOutputFilePath(opts.Output, inputFiles[0]);
        var outputEncoding = Utils.GetEncoding(opts.Encoding);
        var mergePipeline = serviceProvider.GetRequiredService<CsvMergePipeline>();

        var options = new ConversionOptions
        {
            Delimiter = opts.Delimiter,
            IncludeHeaders = !opts.NoHeader,
            OutputEncoding = outputEncoding
        };

        Console.WriteLine($"Merging {inputFiles[0]} and {inputFiles[1]} to {outputFilePath}...");
        using var inputStream1 = File.OpenRead(inputFiles[0]);
        using var inputStream2 = File.OpenRead(inputFiles[1]);
        var mergedStream = await mergePipeline.ProcessAsync(inputStream1, inputStream2, options);

        using var outputStream = File.Create(outputFilePath);
        await mergedStream.CopyToAsync(outputStream);

        Console.WriteLine($"Merge completed successfully.");
        Console.WriteLine($"Output file: {outputFilePath}");
        return 0;
    }
}
