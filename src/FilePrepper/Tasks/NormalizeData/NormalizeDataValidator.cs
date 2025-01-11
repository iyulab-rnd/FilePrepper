namespace FilePrepper.Tasks.NormalizeData;

public class NormalizeDataValidator : BaseValidator<NormalizeDataOption>
{
    public NormalizeDataValidator(ILogger<NormalizeDataValidator> logger) : base(logger)
    {
    }
}