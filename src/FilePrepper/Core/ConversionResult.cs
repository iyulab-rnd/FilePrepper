using System.Text;

namespace FilePrepper.Core;

/// <summary>
/// 파일 변환 작업의 결과를 나타내는 클래스
/// </summary>
public class ConversionResult
{
    public bool Success { get; set; }
    public Stream? ResultStream { get; set; }
    public string? ErrorMessage { get; set; }
    public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}

/// <summary>
/// 파일 변환 작업의 설정을 정의하는 클래스
/// </summary>
public class ConversionOptions
{
    public string Delimiter { get; set; } = ",";
    public bool IncludeHeaders { get; set; } = true;
    public Encoding OutputEncoding { get; set; } = Encoding.UTF8;
    public IDictionary<string, string> CustomOptions { get; set; } = new Dictionary<string, string>();
}

/// <summary>
/// 파일 변환기의 기본 인터페이스
/// </summary>
public interface IFileConverter
{
    Task<ConversionResult> ConvertToCsvAsync(
        Stream inputStream,
        ConversionOptions options,
        CancellationToken cancellationToken = default);

    bool CanHandle(string sourceFormat);
}

/// <summary>
/// 파일 변환 과정의 각 단계를 처리하는 파이프라인 인터페이스
/// </summary>
public interface IConversionPipeline
{
    Task<Stream> ProcessAsync(Stream inputStream, ConversionOptions options);
}

/// <summary>
/// 변환 과정에서 발생할 수 있는 예외들
/// </summary>
public class FilePreppingException : Exception
{
    public FilePreppingException(string message) : base(message) { }
    public FilePreppingException(string message, Exception inner) : base(message, inner) { }
}