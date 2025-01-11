using Microsoft.Extensions.Logging;

namespace FilePrepper.Tasks.FillMissingValues;

public class FillMissingValuesValidator : BaseValidator<FillMissingValuesOption>
{
    public FillMissingValuesValidator(ILogger<FillMissingValuesValidator> logger)
        : base(logger)
    {
    }
}