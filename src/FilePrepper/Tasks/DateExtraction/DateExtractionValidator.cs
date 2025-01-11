using Microsoft.Extensions.Logging;

namespace FilePrepper.Tasks.DateExtraction;

public class DateExtractionValidator : BaseValidator<DateExtractionOption>
{
    public DateExtractionValidator(ILogger<DateExtractionValidator> logger)
        : base(logger)
    {
    }
}