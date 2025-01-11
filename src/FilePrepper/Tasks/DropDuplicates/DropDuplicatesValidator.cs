namespace FilePrepper.Tasks.DropDuplicates;

public class DropDuplicatesValidator : BaseValidator<DropDuplicatesOption>
{
    public DropDuplicatesValidator(ILogger<DropDuplicatesValidator> logger)
        : base(logger)
    {
    }

    protected override string[] ValidateSpecific(DropDuplicatesOption option)
    {
        var errors = new List<string>();

        // SubsetColumnsOnly가 true일 때만 TargetColumns 검증
        if (option.SubsetColumnsOnly)
        {
            errors.AddRange(ValidationUtils.ValidateColumns(option.TargetColumns));
        }

        return [.. errors];
    }
}