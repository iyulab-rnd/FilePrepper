using CommandLine;
using FilePrepper.Tasks;

namespace FilePrepper.CLI.Parameters;

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

    public abstract Type GetHandlerType();

    public virtual CommonTaskOptions GetCommonOptions() => new()
    {
        ErrorHandling = new()
        {
            IgnoreErrors = IgnoreErrors,
            DefaultValue = DefaultValue
        }
    };
}
