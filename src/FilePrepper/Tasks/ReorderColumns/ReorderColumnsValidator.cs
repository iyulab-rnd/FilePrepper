namespace FilePrepper.Tasks.ReorderColumns;

public class ReorderColumnsValidator : BaseValidator<ReorderColumnsOption>
{
    public ReorderColumnsValidator(ILogger<ReorderColumnsValidator> logger) : base(logger) { }
    protected override string[] ValidateSpecific(ReorderColumnsOption option)
    {
        return Array.Empty<string>();
    }
}
