namespace FilePrepper.Tasks.RemoveColumns;

public class RemoveColumnsValidator : BaseValidator<RemoveColumnsOption>
{
    public RemoveColumnsValidator(ILogger<RemoveColumnsValidator> logger)
        : base(logger)
    {
    }

    // If you need additional validations beyond what's in RemoveColumnsOption.ValidateInternal, 
    // implement them in ValidateSpecific().
    protected override string[] ValidateSpecific(RemoveColumnsOption option)
    {
        return Array.Empty<string>();
    }
}
