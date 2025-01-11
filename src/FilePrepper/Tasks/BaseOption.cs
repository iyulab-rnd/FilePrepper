namespace FilePrepper.Tasks;

public abstract class BaseOption : ITaskOption
{
    public CommonTaskOptions Common { get; set; } = new();

    public bool IsValid => Validate().Length == 0;

    public virtual string[] Validate()
    {
        var errors = new List<string>();

        // IColumnOption 검증은 BaseColumnOption으로 이동
        errors.AddRange(ValidateInternal());

        return [.. errors];
    }

    protected abstract string[] ValidateInternal();
}