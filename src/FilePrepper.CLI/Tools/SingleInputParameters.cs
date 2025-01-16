using CommandLine;
using Microsoft.Extensions.Logging;

namespace FilePrepper.CLI.Tools;

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
}