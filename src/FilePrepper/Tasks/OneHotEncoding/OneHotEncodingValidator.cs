namespace FilePrepper.Tasks.OneHotEncoding;

public class OneHotEncodingValidator : BaseValidator<OneHotEncodingOption>
{
    public OneHotEncodingValidator(ILogger<OneHotEncodingValidator> logger) : base(logger)
    {
    }

    protected override string[] ValidateSpecific(OneHotEncodingOption option)
    {
        // BaseValidator calls ValidateCommon -> ValidateSpecific.
        // If we had additional checks beyond what's in OneHotEncodingOption.ValidateInternal(),
        // we would place them here.
        return Array.Empty<string>();
    }
}
