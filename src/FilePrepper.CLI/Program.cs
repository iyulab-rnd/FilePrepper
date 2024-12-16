using CommandLine;
using FilePrepper.CLI.Handlers;
using FilePrepper.CLI.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FilePrepper.CLI;

class Program
{
    static async Task<int> Main(string[] args)
    {
#if DEBUG
        var input1 = @"C:\Users\achun\Downloads\03. Dataset_CNC\dataset\CNC 학습통합데이터_1209\X_train.csv";
        var input2 = @"C:\Users\achun\Downloads\03. Dataset_CNC\dataset\CNC 학습통합데이터_1209\Y_train.csv";
        var output = @"C:\Users\achun\Downloads\03. Dataset_CNC\dataset\CNC 학습통합데이터_1209\merged.csv";

        args = ["merge", "-i", $"{input1},{input2}", "-o", output, "--no-header", "--verbose"];
#endif

        return await Parser.Default.ParseArguments<ConvertOptions, MergeOptions, PreprocessOptions>(args)
            .MapResult(
                async (ConvertOptions opts) => await HandleCommand(opts, ConvertHandler.HandleAsync),
                async (MergeOptions opts) => await HandleCommand(opts, MergeHandler.HandleAsync),
                async (PreprocessOptions opts) => await HandleCommand(opts, PreprocessHandler.HandleAsync),
                errs => Task.FromResult(1));
    }

    private static async Task<int> HandleCommand<T>(T opts, Func<T, IServiceProvider, Task<int>> handler)
        where T : CommonOptions
    {
        var services = ConfigureServices(opts.Verbose);
        var serviceProvider = services.BuildServiceProvider();
        return await handler(opts, serviceProvider);
    }

    private static IServiceCollection ConfigureServices(bool verbose)
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(verbose ? LogLevel.Debug : LogLevel.Information);
        });

        services.AddFilePrepper(options =>
        {
            options.EnableLogging = true;
            options.MaxFileSizeInMb = 100;
        });

        return services;
    }
}