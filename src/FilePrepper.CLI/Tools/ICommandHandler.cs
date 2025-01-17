using Microsoft.Extensions.Logging;

namespace FilePrepper.CLI.Tools;

/// <summary>
/// CLI 명령어 핸들러의 기본 인터페이스
/// </summary>
public interface ICommandHandler
{
    /// <summary>
    /// 명령어를 비동기적으로 실행합니다.
    /// </summary>
    /// <param name="parameters">명령어 실행에 필요한 매개변수</param>
    /// <returns>실행 결과 코드. 0은 성공, 그 외는 실패</returns>
    Task<int> ExecuteAsync(ICommandParameters parameters);

    /// <summary>
    /// 명령어 사용 예시를 반환합니다.
    /// </summary>
    /// <returns>명령어 사용 예시 문자열. null이면 예시가 없음.</returns>
    string? GetExample();
}

/// <summary>
/// 명령어 실행 결과 코드
/// </summary>
public static class ExitCodes
{
    /// <summary>
    /// 성공적으로 실행됨
    /// </summary>
    public const int Success = 0;

    /// <summary>
    /// 일반적인 오류 발생
    /// </summary>
    public const int Error = 1;

    /// <summary>
    /// 잘못된 인수 또는 매개변수
    /// </summary>
    public const int InvalidArguments = 2;

    /// <summary>
    /// 파일 접근 또는 I/O 오류
    /// </summary>
    public const int FileError = 3;

    /// <summary>
    /// 작업 취소됨
    /// </summary>
    public const int Cancelled = 4;
}

/// <summary>
/// 명령어 핸들러의 기본 추상 클래스
/// </summary>
/// <typeparam name="TParameters">명령어 매개변수 타입</typeparam>
public abstract class BaseCommandHandler<TParameters> : ICommandHandler
    where TParameters : class, ICommandParameters
{
    protected readonly ILoggerFactory _loggerFactory;
    protected readonly ILogger _logger;

    protected BaseCommandHandler(
        ILoggerFactory loggerFactory,
        ILogger logger)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public abstract Task<int> ExecuteAsync(ICommandParameters parameters);

    public abstract string? GetExample();

    /// <summary>
    /// 공통적인 매개변수 검증을 수행합니다.
    /// </summary>
    protected virtual bool ValidateParameters(TParameters parameters)
    {
        if (parameters == null)
        {
            _logger.LogError("Parameters cannot be null");
            return false;
        }

        if (!parameters.Validate(_logger))
        {
            _logger.LogError("Parameter validation failed");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 에러를 적절히 처리하고 결과 코드를 반환합니다.
    /// </summary>
    protected virtual int HandleError(Exception ex)
    {
        _logger.LogError(ex, "Error executing command");

        return ex switch
        {
            ArgumentException => ExitCodes.InvalidArguments,
            FileNotFoundException or IOException => ExitCodes.FileError,
            OperationCanceledException => ExitCodes.Cancelled,
            _ => ExitCodes.Error
        };
    }

    /// <summary>
    /// 실행 중 예외 발생 시 처리합니다.
    /// </summary>
    protected virtual async Task<int> HandleExceptionAsync(Func<Task<int>> action)
    {
        try
        {
            return await action();
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }
}