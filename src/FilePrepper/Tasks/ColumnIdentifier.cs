namespace FilePrepper.Tasks;

/// <summary>
/// 컬럼을 이름 또는 인덱스로 지정
/// </summary>
public class ColumnIdentifier
{
    public string? Name { get; set; }
    public int? Index { get; set; }

    public bool IsValid => Name != null || Index != null;

    public static ColumnIdentifier ByName(string name) => new() { Name = name };
    public static ColumnIdentifier ByIndex(int index) => new() { Index = index };
}