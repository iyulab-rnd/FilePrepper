using Microsoft.Extensions.Logging;

namespace FilePrepper.Tasks.AddColumns;

public class AddColumnsValidator : BaseValidator<AddColumnsOption>
{
    public AddColumnsValidator(ILogger<AddColumnsValidator> logger)
        : base(logger)
    {
    }
}