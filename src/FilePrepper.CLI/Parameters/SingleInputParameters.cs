using CommandLine;

namespace FilePrepper.CLI.Parameters;

/// <summary>
/// 단일 입력 파일을 처리하는 명령어를 위한 기본 클래스
/// </summary>
public abstract class SingleInputParameters : BaseParameters
{
    [Option('i', "input", Required = true, HelpText = "Input file path")]
    public string InputPath { get; set; } = string.Empty;

    [Option('o', "output", Required = true, HelpText = "Output file path")]
    public string OutputPath { get; set; } = string.Empty;
}
