namespace FilePrepper.Tasks;

public class TaskContext
{
    private readonly ITaskOption _options;

    public TaskContext(ITaskOption options)
    {
        _options = options;
    }

    public ITaskOption Options => _options;

    public string InputPath
    {
        get
        {
            return _options switch
            {
                SingleInputOption single => single.InputPath,
                MultipleInputOption multiple => multiple.InputPaths.FirstOrDefault()
                    ?? throw new InvalidOperationException("No input paths specified"),
                _ => throw new InvalidOperationException($"Unsupported option type: {_options.GetType().Name}")
            };
        }
    }

    public string OutputPath => _options.OutputPath;

    public Dictionary<string, object> Parameters { get; } = [];

    public T GetOptions<T>() where T : class, ITaskOption
    {
        if (_options is not T typedOptions)
        {
            throw new InvalidOperationException($"Options is not of type {typeof(T).Name}");
        }
        return typedOptions;
    }

    // 다중 입력 파일을 위한 헬퍼 메서드
    public IReadOnlyList<string> GetAllInputPaths()
    {
        return _options switch
        {
            SingleInputOption single => new[] { single.InputPath },
            MultipleInputOption multiple => multiple.InputPaths,
            _ => throw new InvalidOperationException($"Unsupported option type: {_options.GetType().Name}")
        };
    }

    // 특정 인덱스의 입력 파일 경로를 가져오는 메서드
    public string GetInputPath(int index = 0)
    {
        var paths = GetAllInputPaths();
        if (index < 0 || index >= paths.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index),
                $"Index {index} is out of range. Available paths: {paths.Count}");
        }
        return paths[index];
    }
}