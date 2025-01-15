using CommandLine;

namespace FilePrepper.CLI.Parameters;

/// <summary>
/// 다중 입력 파일을 처리하는 명령어를 위한 기본 클래스
/// </summary>
public abstract class MultipleInputParameters : SingleInputParameters
{
    [Option('o', "output", Required = true, HelpText = "Output file path")]
    public string OutputPath { get; set; } = string.Empty;
}
