namespace FilePrepper.Tasks.NormalizeData;

public class NormalizeDataValidator : BaseValidator<NormalizeDataOption>
{
    public NormalizeDataValidator(ILogger<NormalizeDataValidator> logger) : base(logger)
    {
    }

    /// <summary>
    /// Additional validations beyond the base validator,
    /// if needed. Currently none, so return empty array.
    /// </summary>
    protected override string[] ValidateSpecific(NormalizeDataOption option)
    {
        return Array.Empty<string>();
    }
}