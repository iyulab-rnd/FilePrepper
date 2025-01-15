using FilePrepper.Tasks;

namespace FilePrepper.CLI.Parameters;

/// <summary>
/// 모든 명령어 매개변수의 기본 인터페이스
/// </summary>
public interface ICommandParameters
{
    Type GetHandlerType();
    CommonTaskOptions GetCommonOptions();
}