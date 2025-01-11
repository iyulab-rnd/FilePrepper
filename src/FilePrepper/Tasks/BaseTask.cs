using CsvHelper;

namespace FilePrepper.Tasks;

public abstract class BaseTask<TOption> : ITask where TOption : BaseOption
{
    protected readonly ILogger _logger;
    protected List<string> _originalHeaders = new();

    public string Name => GetType().Name.Replace("Task", string.Empty);
    public TOption Options { get; }
    ITaskOption ITask.Options => Options;

    protected BaseTask(TOption options, ILogger logger)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public bool Execute(TaskContext context)
    {
        return ExecuteAsync(context).GetAwaiter().GetResult();
    }

    public async Task<bool> ExecuteAsync(TaskContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        try
        {
            // 필수 컬럼 검증
            var requiredColumns = GetRequiredColumns().ToList();
            if (requiredColumns.Count != 0)
            {
                using var reader = new StreamReader(context.InputPath);
                using var csv = new CsvReader(reader, CsvUtils.GetDefaultConfiguration());

                await csv.ReadAsync();
                csv.ReadHeader();
                var headers = csv.HeaderRecord?.ToList() ?? new List<string>();

                var missingColumns = requiredColumns.Where(col => !headers.Contains(col)).ToList();
                if (missingColumns.Count != 0)
                {
                    _logger.LogError("Required columns not found: {Columns}", string.Join(", ", missingColumns));
                    return false;
                }
            }

            var records = await ReadAndPreProcessAsync(context);
            records = await ProcessRecordsAsync(records);
            await PostProcessAndWriteAsync(records, context);
            return true;
        }
        catch (Exception ex)
        {
            if (Options.Common.ErrorHandling.IgnoreErrors)
            {
                _logger.LogWarning(ex, "Error ignored in {TaskName} task", Name);
                return true;
            }

            _logger.LogError(ex, "Error executing {TaskName} task", Name);
            return false;
        }
    }

    private async Task<List<Dictionary<string, string>>> ReadAndPreProcessAsync(TaskContext context)
    {
        _logger.LogInformation("Reading input file: {InputPath}", context.InputPath);
        var (records, headers) = await ReadCsvFileAsync(context.InputPath);
        _originalHeaders = headers;
        return await PreProcessRecordsAsync(records);
    }

    protected virtual Task<List<Dictionary<string, string>>> PreProcessRecordsAsync(
        List<Dictionary<string, string>> records)
    {
        return Task.FromResult(records);
    }

    protected abstract Task<List<Dictionary<string, string>>> ProcessRecordsAsync(
        List<Dictionary<string, string>> records);

    protected virtual Task<List<Dictionary<string, string>>> PostProcessRecordsAsync(
        List<Dictionary<string, string>> records)
    {
        return Task.FromResult(records);
    }

    protected virtual IEnumerable<string> GetRequiredColumns() =>
        Options is BaseColumnOption columnOption ? columnOption.TargetColumns : Array.Empty<string>();

    protected virtual async Task<(List<Dictionary<string, string>>, List<string>)> ReadCsvFileAsync(
        string filePath,
        IEnumerable<string>? requiredColumns = null)
    {
        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, CsvUtils.GetDefaultConfiguration());

        await csv.ReadAsync();
        csv.ReadHeader();
        var headers = csv.HeaderRecord?.ToList() ?? new List<string>();

        if (requiredColumns != null)
        {
            var headerErrors = CsvUtils.ValidateHeaders(requiredColumns, headers);
            if (headerErrors.Count != 0)
            {
                throw new ValidationException(string.Join(", ", headerErrors));
            }
        }

        var records = new List<Dictionary<string, string>>();
        while (await csv.ReadAsync())
        {
            var record = new Dictionary<string, string>();
            foreach (var header in headers)
            {
                record[header] = csv.GetField(header) ?? string.Empty;
            }
            records.Add(record);
        }

        return (records, headers);
    }

    protected virtual async Task WriteOutputAsync(
        string outputPath,
        IEnumerable<string> headers,
        IEnumerable<Dictionary<string, string>> records)
    {
        var finalHeaders = headers.Any() ? headers : _originalHeaders;
        if (!finalHeaders.Any())
        {
            finalHeaders = ["NoData"];
        }

        await using var writer = new StreamWriter(outputPath);
        await using var csv = new CsvWriter(writer, CsvUtils.GetDefaultConfiguration());

        foreach (var header in finalHeaders)
        {
            csv.WriteField(header);
        }
        csv.NextRecord();

        foreach (var record in records)
        {
            foreach (var header in finalHeaders)
            {
                csv.WriteField(record.GetValueOrDefault(header, string.Empty));
            }
            csv.NextRecord();
        }
    }

    private async Task PostProcessAndWriteAsync(
        List<Dictionary<string, string>> records,
        TaskContext context)
    {
        records = await PostProcessRecordsAsync(records);
        _logger.LogInformation("Writing output file: {OutputPath}", context.OutputPath);

        var headers = records.FirstOrDefault()?.Keys ?? Enumerable.Empty<string>();
        await WriteOutputAsync(context.OutputPath!, headers, records);

        _logger.LogInformation("Task completed successfully");
    }
}