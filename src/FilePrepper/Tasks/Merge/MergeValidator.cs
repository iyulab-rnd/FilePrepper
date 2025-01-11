namespace FilePrepper.Tasks.Merge;

public class MergeValidator : BaseValidator<MergeOption>
{
    public MergeValidator(ILogger<MergeValidator> logger) : base(logger)
    {
    }

    // BaseValidator에서 MergeOption.ValidateInternal()을 이미 호출하므로
    // 별도 추가 검증이 필요한 경우만 여기서 처리
    protected override string[] ValidateSpecific(MergeOption option)
    {
        return [];
    }
}
