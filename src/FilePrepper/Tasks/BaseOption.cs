using FilePrepper.Utils;

namespace FilePrepper.Tasks;

public abstract class BaseOption : ITaskOption
{
    public CommonTaskOptions Common { get; set; } = new();

    public bool IgnoreErrors
    {
        get => Common.IgnoreErrors;
        set => Common.IgnoreErrors = value;
    }

    public string? DefaultValue
    {
        get => Common.DefaultValue;
        set => Common.DefaultValue = value;
    }

    public bool IsValid => Validate().Length == 0;

    public virtual string[] Validate()  // virtual로 변경
    {
        var errors = new List<string>();

        // 공통 검증 로직
        if (this is IColumnOption columnOption)
        {
            errors.AddRange(ValidationUtils.ValidateColumns(columnOption.TargetColumns));
        }

        // 추가 검증은 파생 클래스에서 수행
        errors.AddRange(ValidateInternal());

        return errors.ToArray();
    }

    protected abstract string[] ValidateInternal();
}