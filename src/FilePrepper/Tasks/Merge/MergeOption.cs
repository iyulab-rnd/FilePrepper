namespace FilePrepper.Tasks.Merge;

public enum MergeType
{
    /// <summary>
    /// 세로로 머지 (union). 모든 컬럼 통합, 중복 열은 그대로 유지
    /// </summary>
    Vertical,

    /// <summary>
    /// 가로로 머지 (join). 지정된 키 컬럼을 기준으로 데이터 결합
    /// </summary>
    Horizontal
}

public enum JoinType
{
    /// <summary>
    /// 두 집합에서 키가 모두 있는 레코드만 결합 (INNER JOIN)
    /// </summary>
    Inner,

    /// <summary>
    /// 왼쪽 집합 기준, 오른쪽이 없어도 결합 (LEFT JOIN)
    /// </summary>
    Left,

    /// <summary>
    /// 오른쪽 집합 기준, 왼쪽이 없어도 결합 (RIGHT JOIN)
    /// </summary>
    Right,

    /// <summary>
    /// 키가 있든 없든 전부 결합 (FULL OUTER JOIN)
    /// </summary>
    Full
}

public class MergeOption : BaseOption
{
    public List<string> InputPaths { get; set; } = new();
    public MergeType MergeType { get; set; } = MergeType.Vertical;
    public JoinType JoinType { get; set; } = JoinType.Inner;
    public List<ColumnIdentifier> JoinKeyColumns { get; set; } = new();
    public bool StrictColumnCount { get; set; }
    public bool HasHeader { get; set; } = true;


    protected override string[] ValidateInternal()
    {
        var errors = new List<string>();

        // 1) 파일 리스트 검증
        if (InputPaths == null || InputPaths.Count < 2)
        {
            errors.Add("At least two input files must be specified for merging.");
        }
        else
        {
            for (int i = 0; i < InputPaths.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(InputPaths[i]))
                {
                    errors.Add($"Input path at index {i} cannot be empty or whitespace.");
                }
                else if (!File.Exists(InputPaths[i]))
                {
                    errors.Add($"Input file does not exist: {InputPaths[i]}");
                }
            }
        }

        // 2) Join Key가 지정된 경우에만 JoinType 관련 검증
        if (JoinKeyColumns?.Count > 0)
        {
            if (MergeType != MergeType.Horizontal)
            {
                errors.Add("Join key columns can only be specified for horizontal merge.");
            }
            else
            {
                // Join Key Column 유효성 검증
                for (int i = 0; i < JoinKeyColumns.Count; i++)
                {
                    var keyCol = JoinKeyColumns[i];
                    if (!keyCol.IsValid)
                    {
                        errors.Add($"Join key column at index {i} must specify either Name or Index.");
                    }
                    if (keyCol.Index.HasValue && keyCol.Index.Value < 0)
                    {
                        errors.Add($"Join key column index at position {i} cannot be negative.");
                    }
                }
            }
        }

        return errors.ToArray();
    }
}
