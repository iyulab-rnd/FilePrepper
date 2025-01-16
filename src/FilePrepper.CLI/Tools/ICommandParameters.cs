using FilePrepper.Tasks;
using Microsoft.Extensions.Logging;

namespace FilePrepper.CLI.Tools;

/// <summary>
/// 모든 명령어 매개변수의 기본 인터페이스
/// </summary>
public interface ICommandParameters
{
    Type GetHandlerType();
    CommonTaskOptions GetCommonOptions();

    /// <summary>
    /// 매개변수의 유효성을 검사합니다.
    /// </summary>
    /// <param name="logger">로깅을 위한 ILogger 인스턴스</param>
    /// <returns>유효성 검사 결과</returns>
    bool Validate(ILogger logger);
}