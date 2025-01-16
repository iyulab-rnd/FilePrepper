using CommandLine;
using FilePrepper.Tasks;
using Microsoft.Extensions.Logging;

namespace FilePrepper.CLI.Tools;

/// <summary>
/// 다중 입력 파일을 처리하는 명령어를 위한 기본 클래스
/// </summary>
public abstract class MultipleInputParameters : BaseParameters
{
    [Value(0, Required = true, Min = 2, MetaName = "inputs",
        HelpText = "Input CSV files to merge (minimum 2 files required)")]
    public IEnumerable<string> InputFiles { get; set; } = [];

    [Option('o', "output", Required = true, HelpText = "Output file path")]
    public string OutputPath { get; set; } = string.Empty;

    public override bool Validate(ILogger logger)
    {
        if (!base.Validate(logger)) return false;

        if (!InputFiles.Any())
        {
            logger.LogError("No input files specified");
            return false;
        }

        foreach (var inputPath in InputFiles)
        {
            if (!ValidateInputPath(inputPath, logger)) return false;
        }

        if (!ValidateOutputPath(OutputPath, logger)) return false;

        return true;
    }
}