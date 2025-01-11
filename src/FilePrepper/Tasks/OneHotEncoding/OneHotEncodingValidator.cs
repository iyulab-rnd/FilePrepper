namespace FilePrepper.Tasks.OneHotEncoding;

public class OneHotEncodingValidator : BaseValidator<OneHotEncodingOption>
{
    public OneHotEncodingValidator(ILogger<OneHotEncodingValidator> logger) : base(logger)
    {
    }
}