using FilePrepper.Utils;
using Microsoft.Extensions.Logging;

namespace FilePrepper.Tasks;

public abstract class BaseValidator<TOption> : IOptionValidator where TOption : BaseOption
{
    protected readonly ILogger _logger;

    protected BaseValidator(ILogger logger)
    {
        _logger = logger;
    }

    public virtual bool Validate(ITaskOption option, out string[] errors)
    {
        if (option == null)
        {
            errors = new[] { "Options cannot be null" };
            return false;
        }

        if (option is not TOption typedOption)
        {
            errors = new[] { $"Invalid option type: {option.GetType()}" };
            return false;
        }

        // 공통 검증
        errors = ValidateCommon(typedOption);
        if (errors.Length > 0)
        {
            return false;
        }

        // 추가 검증
        errors = ValidateSpecific(typedOption);
        return errors.Length == 0;
    }

    protected virtual string[] ValidateCommon(TOption option)
    {
        var errors = new List<string>();

        if (option is IColumnOption columnOption)
        {
            errors.AddRange(ValidationUtils.ValidateColumns(columnOption.TargetColumns));
        }

        return errors.ToArray();
    }

    protected virtual string[] ValidateSpecific(TOption option) => Array.Empty<string>();

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