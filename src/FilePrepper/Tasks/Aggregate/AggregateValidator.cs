using Microsoft.Extensions.Logging;

namespace FilePrepper.Tasks.Aggregate;

public class AggregateValidator : BaseValidator<AggregateOption>
{
    public AggregateValidator(ILogger<AggregateValidator> logger)
        : base(logger)
    {
    }
}