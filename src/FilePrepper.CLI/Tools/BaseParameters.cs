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

/// <summary>
/// 단일 입력 파일을 처리하는 명령어를 위한 기본 클래스
/// </summary>
public abstract class SingleInputParameters : BaseParameters
{
    [Option('i', "input", Required = true, HelpText = "Input file path")]
    public string InputPath { get; set; } = string.Empty;

    [Option('o', "output", Required = true, HelpText = "Output file path")]
    public string OutputPath { get; set; } = string.Empty;

    public override bool Validate(ILogger logger)
    {
        if (!base.Validate(logger)) return false;

        if (!ValidateInputPath(InputPath, logger)) return false;
        if (!ValidateOutputPath(OutputPath, logger)) return false;

        return true;
    }

    public override CommonTaskOptions GetCommonOptions()
    {
        var options = base.GetCommonOptions();

        return options;
    }
}

/// <summary>
/// 다중 입력 파일을 처리하는 명령어를 위한 기본 클래스
/// </summary>
public abstract class MultipleInputParameters : BaseParameters
{
    // Option 대신 Value 어트리뷰트 사용하여 위치 기반 인자로 처리
    [Value(0, Required = true, Min = 2,
           HelpText = "Input CSV files to merge (minimum 2 files required)")]
    public IEnumerable<string> InputFiles { get; set; } = [];

    [Option('o', "output", Required = true, HelpText = "Output file path")]
    public string OutputPath { get; set; } = string.Empty;

    public override bool Validate(ILogger logger)
    {
        if (!base.Validate(logger)) return false;

        var inputList = InputFiles.Select(path => path.Trim('"')).ToList();
        if (inputList.Count < 2)
        {
            logger.LogError("At least two input files are required");
            return false;
        }

        foreach (var inputPath in inputList)
        {
            if (!ValidateInputPath(inputPath, logger)) return false;
        }

        if (!ValidateOutputPath(OutputPath, logger)) return false;

        return true;
    }
}