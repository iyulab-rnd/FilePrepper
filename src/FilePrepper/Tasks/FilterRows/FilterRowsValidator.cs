namespace FilePrepper.Tasks.FilterRows;

public class FilterRowsValidator : BaseValidator<FilterRowsOption>
{
    public FilterRowsValidator(ILogger<FilterRowsValidator> logger) : base(logger)
    {
    }

    // 필요한 추가 검증 로직이 있으면 아래 메서드에서 처리
    protected override string[] ValidateSpecific(FilterRowsOption option)
    {
        // 기본 검증 로직은 이미 BaseValidator에서 처리
        return Array.Empty<string>();
    }
}
