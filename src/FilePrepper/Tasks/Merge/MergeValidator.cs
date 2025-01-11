namespace FilePrepper.Tasks.Merge;

public class MergeValidator : BaseValidator<MergeOption>
{
    public MergeValidator(ILogger<MergeValidator> logger) : base(logger)
    {
    }
}