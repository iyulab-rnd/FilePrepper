namespace FilePrepper.Tasks;

public abstract class BaseColumnOption : SingleInputOption
{
    public string[] TargetColumns { get; set; } = [];

    public override string[] Validate()
    {
        var errors = new List<string>();
        errors.AddRange(ValidationUtils.ValidateColumns(TargetColumns));
        errors.AddRange(ValidateInternal());
        return [.. errors];
    }
}