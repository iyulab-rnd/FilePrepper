namespace FilePrepper.Tasks.RenameColumns;

public class RenameColumnsValidator : BaseValidator<RenameColumnsOption>
{
    public RenameColumnsValidator(ILogger<RenameColumnsValidator> logger) : base(logger)
    {
    }
}