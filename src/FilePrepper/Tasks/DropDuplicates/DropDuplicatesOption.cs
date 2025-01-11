namespace FilePrepper.Tasks.DropDuplicates;

public class DropDuplicatesOption : BaseOption
{
    /// <summary>
    /// 첫 번째 발견된 중복 데이터를 유지할지 여부
    /// false인 경우 마지막 발견된 중복 데이터를 유지
    /// </summary>
    public bool KeepFirst { get; set; } = true;

    /// <summary>
    /// 특정 컬럼만을 기준으로 중복을 체크할지 여부
    /// false인 경우 모든 컬럼을 체크
    /// </summary>
    public bool SubsetColumnsOnly { get; set; } = false;

    public string[] TargetColumns { get; set; } = Array.Empty<string>();

    protected override string[] ValidateInternal()
    {
        var errors = new List<string>();

        if (SubsetColumnsOnly && (TargetColumns == null || TargetColumns.Length == 0))
        {
            errors.Add("Target columns must be specified when using subset columns");
        }

        return errors.ToArray();
    }
}