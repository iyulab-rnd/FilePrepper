namespace FilePrepper.Tasks;

public class BaseValidator<TOption> : IOptionValidator where TOption : BaseOption
{
    protected readonly ILogger _logger;

    protected BaseValidator(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public virtual bool Validate(ITaskOption option, out string[] errors)
    {
        if (option == null)
        {
            errors = ["Options cannot be null"];
            return false;
        }

        if (option is not TOption typedOption)
        {
            errors = [$"Invalid option type: {option.GetType()}"];
            return false;
        }

        var allErrors = new List<string>();
        allErrors.AddRange(ValidateSpecific(typedOption));

        errors = [.. allErrors];
        return errors.Length == 0;
    }

    protected virtual string[] ValidateSpecific(TOption option) => [];

    public void ValidateOrThrow(ITaskOption option)
    {
        if (!Validate(option, out var errors))
        {
            throw new ValidationException(
                string.Join("; ", errors),
                ValidationExceptionErrorCode.General
            );
        }
    }
}