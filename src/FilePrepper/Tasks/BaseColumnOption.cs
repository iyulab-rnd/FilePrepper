using FilePrepper.Utils;

namespace FilePrepper.Tasks;

public abstract class BaseColumnOption : BaseOption, IColumnOption
{
    public string[] TargetColumns { get; set; } = Array.Empty<string>();

    protected override string[] ValidateInternal()
    {
        return ValidationUtils.ValidateColumns(TargetColumns, "target columns");
    }
}