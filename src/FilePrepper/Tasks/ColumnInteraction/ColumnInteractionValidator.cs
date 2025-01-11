using Microsoft.Extensions.Logging;

namespace FilePrepper.Tasks.ColumnInteraction;


public class ColumnInteractionValidator : BaseValidator<ColumnInteractionOption>
{
    public ColumnInteractionValidator(ILogger<ColumnInteractionValidator> logger)
        : base(logger)
    {
    }
}