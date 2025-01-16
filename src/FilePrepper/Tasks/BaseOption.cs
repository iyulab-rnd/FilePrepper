using FilePrepper.Tasks;

public abstract class BaseOption : ITaskOption
{
    public CommonTaskOptions Common { get; set; } = new();
    public bool HasHeader { get; set; } = true;

    public bool IsValid => Validate().Length == 0;

    public virtual string[] Validate()
    {
        var errors = new List<string>();
        errors.AddRange(ValidateCommon());
        errors.AddRange(ValidateInternal());
        return [.. errors];
    }

    protected virtual string[] ValidateCommon()
    {
        var errors = new List<string>();
        if (Common == null)
        {
            errors.Add("Common options cannot be null");
        }
        return errors.ToArray();
    }

    protected abstract string[] ValidateInternal();
}