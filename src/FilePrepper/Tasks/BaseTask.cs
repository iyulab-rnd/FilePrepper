using CsvHelper;
using Microsoft.Extensions.Logging;

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
                    var error = $"Required columns not found: {string.Join(", ", missingColumns)}";
                    _logger.LogError(error);
                    if (!Options.Common.ErrorHandling.IgnoreErrors)
                    {
                        throw new ValidationException(error, ValidationExceptionErrorCode.General);
                    }
                    return true;
                }
            }

            var records = await ReadAndPreProcessAsync(context);
            records = await ProcessRecordsAsync(records);
            await PostProcessAndWriteAsync(records, context);
            return true;
        }
        catch (ValidationException ex)
        {
            if (Options.Common.ErrorHandling.IgnoreErrors)
            {
                _logger.LogWarning(ex, "Validation error ignored in {TaskName} task: {Message}", Name, ex.Message);
                return true;
            }
            _logger.LogError(ex, "Validation error in {TaskName} task: {Message}", Name, ex.Message);
            throw; // ValidationException은 다시 throw
        }
        catch (Exception ex)
        {
            if (Options.Common.ErrorHandling.IgnoreErrors)
            {
                _logger.LogWarning(ex, "Error ignored in {TaskName} task: {Message}", Name, ex.Message);
                return true;
            }
            _logger.LogError(ex, "Error executing {TaskName} task: {Message}", Name, ex.Message);
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

    protected virtual async Task<(List<Dictionary<string, string>> records, List<string> headers)> ReadCsvFileAsync(string path)
    {
        _logger.LogInformation("Reading input file: {Path}", path);

        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, CsvUtils.GetDefaultConfiguration(Options.HasHeader));

        var records = new List<Dictionary<string, string>>();
        var headers = new List<string>();

        if (Options.HasHeader)
        {
            await csv.ReadAsync();
            csv.ReadHeader();
            headers.AddRange(csv.HeaderRecord);
        }
        else
        {
            // 헤더 없는 경우 첫 줄을 읽어서 컬럼 수 파악
            if (await csv.ReadAsync())
            {
                var fieldCount = csv.Parser.RawRecord.Count(c => c == ',') + 1;
                headers.AddRange(Enumerable.Range(0, fieldCount).Select(i => i.ToString()));
            }
        }

        // 레코드 읽기
        while (await csv.ReadAsync())
        {
            var record = new Dictionary<string, string>();
            for (int i = 0; i < headers.Count; i++)
            {
                record[headers[i]] = csv.GetField(i) ?? string.Empty;
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