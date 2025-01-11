namespace FilePrepper.Tasks;

public interface ITaskOption
{
    bool IsValid { get; }  // 옵션이 유효한지 확인
    string[] Validate();   // 유효성 검사 결과 메시지 반환
}


public interface IErrorHandlingOption
{
    bool IgnoreErrors { get; set; }
    string? DefaultValue { get; set; }
}


public interface IColumnOption
{
    string[] TargetColumns { get; set; }
}
