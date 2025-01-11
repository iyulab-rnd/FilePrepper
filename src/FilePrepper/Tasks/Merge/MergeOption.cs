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
    /// <summary>
    /// 머지할 파일 경로 목록
    /// </summary>
    public List<string> InputPaths { get; set; } = new();

    /// <summary>
    /// 세로(Union)로 머지할지, 가로(Join)으로 머지할지
    /// </summary>
    public MergeType MergeType { get; set; } = MergeType.Vertical;

    /// <summary>
    /// MergeType이 Horizontal일 때, JoinType(Inner, Left, Right, Full 등)을 설정
    /// </summary>
    public JoinType JoinType { get; set; } = JoinType.Inner;

    /// <summary>
    /// MergeType이 Horizontal일 때, Join 키로 사용할 컬럼 리스트 (복수 키 조합 가능)
    /// </summary>
    public List<string> JoinKeyColumns { get; set; } = new();

    protected override string[] ValidateInternal()
    {
        var errors = new List<string>();

        // 1) 파일 리스트 검증
        if (InputPaths.Count < 2)
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
            }
        }

        // 2) Horizontal Merge 시, JoinKeyColumns가 적어도 1개 이상 필요
        if (MergeType == MergeType.Horizontal && JoinKeyColumns.Count == 0)
        {
            errors.Add("At least one join key column must be specified for horizontal merge.");
        }

        return [.. errors];
    }
}
