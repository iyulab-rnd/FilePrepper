using CommandLine;
using FilePrepper;
using FilePrepper.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;

class Program
{
    public class Options
    {
        [Value(0, Required = true, HelpText = "Input file to convert.")]
        public string InputFile { get; set; } = string.Empty;

        [Option('o', "output", Required = false, HelpText = "Output path (directory or file path).")]
        public string? Output { get; set; }

        [Option('d', "delimiter", Required = false, Default = ",", HelpText = "CSV delimiter.")]
        public string Delimiter { get; set; } = ",";

        [Option("no-header", Required = false, HelpText = "Exclude headers in output CSV.")]
        public bool NoHeader { get; set; }

        [Option('e', "encoding", Required = false, Default = "utf-8",
            HelpText = "Output file encoding (utf-8, ascii, utf-16, utf-32).")]
        public string Encoding { get; set; } = "utf-8";

        [Option('v', "verbose", Required = false, HelpText = "Enable verbose logging.")]
        public bool Verbose { get; set; }
    }

    static async Task<int> Main(string[] args)
    {
#if DEBUG
        args = [@"D:\data\ML-Research\Ford\dataset\FordA_TRAIN.arff", "-o", @"D:\data\ML-Research\Ford\dataset", "-v"];
#endif

        return await Parser.Default.ParseArguments<Options>(args)
            .MapResult(async (Options opts) =>
            {
                try
                {
                    if (!File.Exists(opts.InputFile))
                    {
                        Console.Error.WriteLine($"Error: Input file '{opts.InputFile}' does not exist.");
                        return 1;
                    }

                    // 출력 경로 설정
                    string outputFilePath;
                    if (string.IsNullOrEmpty(opts.Output))
                    {
                        // 출력 경로가 지정되지 않은 경우, 입력 파일과 같은 위치에 생성
                        var inputDir = Path.GetDirectoryName(opts.InputFile) ?? Environment.CurrentDirectory;
                        outputFilePath = Path.Combine(inputDir, $"{Path.GetFileNameWithoutExtension(opts.InputFile)}.csv");
                    }
                    else
                    {
                        if (Path.HasExtension(opts.Output))
                        {
                            // 확장자가 있는 경우 파일 경로로 처리
                            outputFilePath = opts.Output;
                        }
                        else
                        {
                            // 확장자가 없는 경우 디렉토리로 처리
                            outputFilePath = Path.Combine(opts.Output, $"{Path.GetFileNameWithoutExtension(opts.InputFile)}.csv");
                        }
                    }

                    // 출력 디렉토리 생성
                    var outputDir = Path.GetDirectoryName(outputFilePath);
                    if (!string.IsNullOrEmpty(outputDir))
                    {
                        Directory.CreateDirectory(outputDir);
                    }

                    // 서비스 설정
                    var services = new ServiceCollection()
                        .AddLogging(builder =>
                        {
                            builder.AddConsole();
                            builder.SetMinimumLevel(opts.Verbose ? LogLevel.Debug : LogLevel.Information);
                        })
                        .AddFilePrepper(options =>
                        {
                            options.EnableLogging = true;
                            options.MaxFileSizeInMb = 100;
                        });

                    using var serviceProvider = services.BuildServiceProvider();
                    var converterFactory = serviceProvider.GetRequiredService<IFileConverterFactory>();

                    // 파일 확장자 확인
                    var extension = Path.GetExtension(opts.InputFile).TrimStart('.').ToLower();
                    var converter = converterFactory.GetConverter(extension);

                    // 인코딩 설정
                    var outputEncoding = opts.Encoding.ToLower() switch
                    {
                        "ascii" => System.Text.Encoding.ASCII,
                        "utf-16" => System.Text.Encoding.Unicode,
                        "utf-32" => System.Text.Encoding.UTF32,
                        _ => System.Text.Encoding.UTF8
                    };

                    if (opts.Verbose)
                    {
                        Console.WriteLine($"Input file: {opts.InputFile}");
                        Console.WriteLine($"Output path: {outputFilePath}");
                        Console.WriteLine($"Delimiter: {opts.Delimiter}");
                        Console.WriteLine($"Include headers: {!opts.NoHeader}");
                        Console.WriteLine($"Encoding: {opts.Encoding}");
                    }

                    // 파일 변환
                    Console.WriteLine($"Converting {Path.GetFileName(opts.InputFile)} to CSV...");
                    using var inputStream = File.OpenRead(opts.InputFile);
                    var options = new ConversionOptions
                    {
                        Delimiter = opts.Delimiter,
                        IncludeHeaders = !opts.NoHeader,
                        OutputEncoding = outputEncoding
                    };

                    var result = await converter.ConvertToCsvAsync(inputStream, options);

                    if (!result.Success)
                    {
                        Console.Error.WriteLine($"Error during conversion: {result.ErrorMessage}");
                        return 1;
                    }

                    // 결과 저장
                    using var outputStream = File.Create(outputFilePath);
                    await result.ResultStream!.CopyToAsync(outputStream);

                    Console.WriteLine($"Conversion completed successfully.");
                    Console.WriteLine($"Output file: {outputFilePath}");

                    if (result.Metadata.Count > 0)
                    {
                        Console.WriteLine("\nConversion metadata:");
                        foreach (var (key, value) in result.Metadata)
                        {
                            Console.WriteLine($"{key}: {value}");
                        }
                    }

                    return 0;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error: {ex.Message}");
                    if (opts.Verbose)
                    {
                        Console.Error.WriteLine(ex.StackTrace);
                    }
                    return 1;
                }
            },
            errs => Task.FromResult(1));
    }
}