namespace FilePrepper.Tasks.RemoveColumns;

public class RemoveColumnsValidator : BaseValidator<RemoveColumnsOption>
{
    public RemoveColumnsValidator(ILogger<RemoveColumnsValidator> logger)
        : base(logger)
    {
    }
}