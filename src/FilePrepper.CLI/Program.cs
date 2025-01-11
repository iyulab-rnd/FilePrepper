using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FilePrepper.CLI.Parameters;
using FilePrepper.CLI.Handlers;

namespace FilePrepper.CLI;

public static class ExitCodes
{
    public const int Success = 0;
    public const int Error = 1;
    public const int InvalidHandler = 2;
    public const int ConfigurationError = 3;
    public const int ValidationError = 4;
}

public class Program
{
    private static readonly ILogger<Program> _logger;

    static Program()
    {
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddConsole()
                .SetMinimumLevel(LogLevel.Information);
        });

        _logger = loggerFactory.CreateLogger<Program>();
    }

    static async Task<int> Main(string[] args)
    {
        try
        {
            _logger.LogInformation("Application starting...");

            if (args == null || args.Length == 0)
            {
                _logger.LogError("No arguments provided");
                return ExitCodes.ValidationError;
            }

            var services = ConfigureServices();
            var types = LoadCommandTypes();

            _logger.LogInformation("Parsing command line arguments...");

            return await Parser.Default.ParseArguments(args, types)
                .MapResult(
                    async (ICommandParameters opts) =>
                    {
                        try
                        {
                            var handlerType = opts.GetHandlerType();
                            _logger.LogInformation($"Creating handler of type: {handlerType.Name}");

                            var handler = services.GetRequiredService(handlerType) as ICommandHandler;
                            if (handler == null)
                            {
                                _logger.LogError($"Could not create command handler for type: {handlerType.Name}");
                                return ExitCodes.InvalidHandler;
                            }

                            // 매개변수 유효성 검사
                            if (!ValidateParameters(opts))
                            {
                                _logger.LogError("Parameter validation failed");
                                return ExitCodes.ValidationError;
                            }

                            _logger.LogInformation($"Executing handler: {handler.GetType().Name}");
                            var result = await handler.ExecuteAsync(opts);

                            _logger.LogInformation($"Handler execution completed with result: {result}");
                            return result;
                        }
                        catch (InvalidOperationException ex)
                        {
                            _logger.LogError($"Operation error: {ex.Message}");
                            _logger.LogDebug(ex.StackTrace);
                            return ExitCodes.InvalidHandler;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Unexpected error during handler execution: {ex.Message}");
                            _logger.LogDebug(ex.StackTrace);
                            return ExitCodes.Error;
                        }
                    },
                    errors =>
                    {
                        foreach (var error in errors)
                        {
                            _logger.LogError($"Command line parsing error: {error}");
                        }
                        return Task.FromResult(ExitCodes.ValidationError);
                    });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError($"Configuration error: {ex.Message}");
            _logger.LogDebug(ex.StackTrace);
            return ExitCodes.ConfigurationError;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Fatal error: {ex.Message}");
            _logger.LogDebug(ex.StackTrace);
            return ExitCodes.Error;
        }
        finally
        {
            _logger.LogInformation("Application shutting down...");
        }
    }

    private static ServiceProvider ConfigureServices()
    {
        _logger.LogInformation("Configuring services...");

        try
        {
            var services = new ServiceCollection()
                .AddLogging(builder =>
                {
                    builder
                        .AddConsole()
                        .SetMinimumLevel(LogLevel.Information);
                })
                .AddSingleton<ILoggerFactory, LoggerFactory>();

            RegisterCommandHandlers(services);

            var serviceProvider = services.BuildServiceProvider();

            // 서비스 구성 유효성 검사
            ValidateServiceConfiguration(serviceProvider);

            return serviceProvider;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Service configuration failed: {ex.Message}");
            throw new InvalidOperationException("Failed to configure services", ex);
        }
    }

    private static void RegisterCommandHandlers(IServiceCollection services)
    {
        _logger.LogInformation("Registering command handlers...");

        var assembly = typeof(Program).Assembly;
        var handlerTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract &&
                       !t.IsInterface &&
                       typeof(ICommandHandler).IsAssignableFrom(t))
            .ToList();

        foreach (var handlerType in handlerTypes)
        {
            _logger.LogDebug($"Registering handler: {handlerType.Name}");
            services.AddTransient(handlerType);
        }

        _logger.LogInformation($"Registered {handlerTypes.Count} command handlers");
    }

    private static void ValidateServiceConfiguration(ServiceProvider serviceProvider)
    {
        _logger.LogInformation("Validating service configuration...");

        // 필수 서비스 존재 여부 확인
        var requiredServices = new[]
        {
            typeof(ILoggerFactory),
            // 다른 필수 서비스들을 여기에 추가
        };

        foreach (var serviceType in requiredServices)
        {
            if (serviceProvider.GetService(serviceType) == null)
            {
                throw new InvalidOperationException($"Required service {serviceType.Name} is not registered");
            }
        }
    }

    private static Type[] LoadCommandTypes()
    {
        _logger.LogInformation("Loading command types...");

        try
        {
            var assembly = typeof(Program).Assembly;
            var types = assembly.GetTypes()
                .Where(t => !t.IsAbstract &&
                           !t.IsInterface &&
                           typeof(ICommandParameters).IsAssignableFrom(t))
                .ToArray();

            _logger.LogInformation($"Loaded {types.Length} command types");
            return types;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to load command types: {ex.Message}");
            throw new InvalidOperationException("Failed to load command types", ex);
        }
    }

    private static bool ValidateParameters(ICommandParameters parameters)
    {
        if (parameters == null)
        {
            _logger.LogError("Parameters object is null");
            return false;
        }

        try
        {
            // 기본 매개변수 검증
            if (parameters is BaseParameters baseParams)
            {
                // 입력 파일 검증
                if (string.IsNullOrEmpty(baseParams.InputPath))
                {
                    _logger.LogError("Input path is not specified");
                    return false;
                }

                // 경로 주입 방지
                if (baseParams.InputPath.Contains("..") || baseParams.InputPath.Contains("~"))
                {
                    _logger.LogError("Suspicious input path detected");
                    return false;
                }

                // 입력 파일 존재 여부 확인
                if (!File.Exists(baseParams.InputPath))
                {
                    _logger.LogError($"Input file does not exist: {baseParams.InputPath}");
                    return false;
                }

                // 출력 경로 검증
                if (string.IsNullOrEmpty(baseParams.OutputPath))
                {
                    _logger.LogError("Output path is not specified");
                    return false;
                }

                // 출력 디렉토리 존재 여부 확인
                var outputDir = Path.GetDirectoryName(baseParams.OutputPath);
                if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                {
                    _logger.LogError($"Output directory does not exist: {outputDir}");
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Parameter validation error: {ex.Message}");
            return false;
        }
    }
}