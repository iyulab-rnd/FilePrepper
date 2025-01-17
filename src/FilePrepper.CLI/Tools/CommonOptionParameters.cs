namespace FilePrepper.CLI.Tools;

/// <summary>
/// TaskOption에서 사용되는 공통 옵션들을 담는 클래스
/// </summary>
public class CommonOptionParameters
{
    /// <summary>
    /// 입력 파일이 헤더를 가지고 있는지 여부
    /// </summary>
    public bool HasHeader { get; set; } = true;

    /// <summary>
    /// 처리 중 에러 발생 시 무시하고 계속 진행할지 여부
    /// </summary>
    public bool IgnoreErrors { get; set; }

    /// <summary>
    /// 처리 결과가 저장될 출력 파일 경로
    /// </summary>
    public string OutputPath { get; set; } = string.Empty;

    /// <summary>
    /// 공통 옵션 상태 복사본을 생성
    /// </summary>
    public CommonOptionParameters Clone()
    {
        return new CommonOptionParameters
        {
            HasHeader = this.HasHeader,
            IgnoreErrors = this.IgnoreErrors,
            OutputPath = this.OutputPath
        };
    }

    /// <summary>
    /// 다른 CommonOptionParameters의 값을 현재 객체에 복사
    /// </summary>
    public void CopyFrom(CommonOptionParameters other)
    {
        HasHeader = other.HasHeader;
        IgnoreErrors = other.IgnoreErrors;
        OutputPath = other.OutputPath;
    }

    /// <summary>
    /// 현재 옵션값들의 유효성을 검사
    /// </summary>
    public virtual string[] Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(OutputPath))
        {
            errors.Add("Output path cannot be empty or whitespace");
        }

        return [.. errors];
    }
}