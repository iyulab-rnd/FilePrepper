using CommandLine;
using FilePrepper.Tasks;
using Microsoft.Extensions.Logging;

namespace FilePrepper.CLI.Tools;

/// <summary>
/// 에러 처리와 기타 공통 옵션을 포함하는 기본 클래스
/// </summary>
public abstract class BaseParameters : ICommandParameters
{
    [Option("ignore-errors", Required = false, Default = false,
        HelpText = "Whether to ignore errors during processing")]
    public bool IgnoreErrors { get; set; }

    [Option("default-value", Required = false,
        HelpText = "Default value to use when encountering errors")]
    public string? DefaultValue { get; set; }

    [Option("has-header", Default = true,
        HelpText = "Whether input files have headers")]
    public bool HasHeader { get; set; } = true;

    public abstract Type GetHandlerType();

    public virtual CommonTaskOptions GetCommonOptions() => new()
    {
        ErrorHandling = new()
        {
            IgnoreErrors = IgnoreErrors,
            DefaultValue = DefaultValue
        }
    };

    public virtual bool Validate(ILogger logger)
    {
        try
        {
            // 기본 클래스에서는 항상 true를 반환
            // 파생 클래스에서 필요한 검증을 구현
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError($"Parameter validation error: {ex.Message}");
            return false;
        }
    }

    protected bool ValidateOutputPath(string outputPath, ILogger logger)
    {
        if (string.IsNullOrEmpty(outputPath))
        {
            logger.LogError("Output path is not specified");
            return false;
        }

        var outputDir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
        {
            logger.LogError($"Output directory does not exist: {outputDir}");
            return false;
        }

        return true;
    }

    protected bool ValidateInputPath(string inputPath, ILogger logger)
    {
        if (string.IsNullOrEmpty(inputPath))
        {
            logger.LogError("Input path is not specified");
            return false;
        }

        if (inputPath.Contains("..") || inputPath.Contains("~"))
        {
            logger.LogError($"Suspicious input path detected: {inputPath}");
            return false;
        }

        if (!File.Exists(inputPath))
        {
            logger.LogError($"Input file does not exist: {inputPath}");
            return false;
        }

        return true;
    }
}