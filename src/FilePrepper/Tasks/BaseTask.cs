using CsvHelper;
using FilePrepper.Utils;
using Microsoft.Extensions.Logging;

namespace FilePrepper.Tasks;

public abstract class BaseTask<TOption> : ITask where TOption : BaseOption
{
    protected readonly ILogger _logger;
    protected readonly IOptionValidator _validator;

    public string Name => GetType().Name.Replace("Task", string.Empty);
    public TOption Options { get; }
    ITaskOption ITask.Options => Options;

    protected BaseTask(
        TOption options,
        ILogger logger,
        IOptionValidator validator)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
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

        if (!_validator.Validate(Options, out var errors))
        {
            return ValidationUtils.ValidateAndLogErrors(errors, _logger);
        }

        try
        {
            // 공통 전처리
            _logger.LogInformation("Reading input file: {InputPath}", context.InputPath);
            var records = await ReadAndPreProcessAsync(context);

            try
            {
                // 실제 작업 수행 (파생 클래스에서 구현)
                records = await ProcessRecordsAsync(records);

                // 공통 후처리 및 저장
                await PostProcessAndSaveAsync(records, context);
                return true;
            }
            catch (Exception ex) when (Options.IgnoreErrors)
            {
                // Warning 로그만 기록하고 계속 진행
                _logger.LogWarning(ex, "Error ignored and continuing in {TaskName} task", Name);
                await PostProcessAndSaveAsync(records, context);
                return true;
            }
        }
        catch (Exception ex)
        {
            if (Options.IgnoreErrors)
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
        var records = await ReadCsvFileAsync(context.InputPath, GetRequiredColumns());
        return await PreProcessRecordsAsync(records);
    }

    private async Task PostProcessAndSaveAsync(
        List<Dictionary<string, string>> records,
        TaskContext context)
    {
        records = await PostProcessRecordsAsync(records);

        _logger.LogInformation("Writing output file: {OutputPath}", context.OutputPath);
        var headers = records.FirstOrDefault()?.Keys ?? Enumerable.Empty<string>();
        await WriteCsvFileAsync(context.OutputPath, headers, records);

        _logger.LogInformation("Task completed successfully");
    }

    protected virtual Task<List<Dictionary<string, string>>> PreProcessRecordsAsync(
        List<Dictionary<string, string>> records)
    {
        return Task.FromResult(records);
    }

    protected virtual Task<List<Dictionary<string, string>>> PostProcessRecordsAsync(
        List<Dictionary<string, string>> records)
    {
        return Task.FromResult(records);
    }

    // 파생 클래스에서 구현할 메서드
    protected abstract Task<List<Dictionary<string, string>>> ProcessRecordsAsync(
        List<Dictionary<string, string>> records);

    protected virtual IEnumerable<string> GetRequiredColumns() =>
        Options is IColumnOption columnOption ? columnOption.TargetColumns : Array.Empty<string>();

    protected async Task<List<Dictionary<string, string>>> ReadCsvFileAsync(
        string filePath,
        IEnumerable<string>? requiredColumns = null)
    {
        var records = new List<Dictionary<string, string>>();
        var csvConfig = CsvUtils.GetDefaultConfiguration();

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, csvConfig);

        await csv.ReadAsync();
        csv.ReadHeader();
        var headers = csv.HeaderRecord?.ToList() ?? new List<string>();

        if (requiredColumns != null)
        {
            var headerErrors = CsvUtils.ValidateHeaders(requiredColumns, headers);
            if (headerErrors.Any())
            {
                throw new InvalidOperationException(
                    $"Header validation failed: {string.Join(", ", headerErrors)}");
            }
        }

        while (await csv.ReadAsync())
        {
            var record = new Dictionary<string, string>();
            foreach (var header in headers)
            {
                record[header] = csv.GetField(header) ?? string.Empty;
            }
            records.Add(record);
        }

        return records;
    }

    protected async Task WriteCsvFileAsync(
        string filePath,
        IEnumerable<string> headers,
        IEnumerable<Dictionary<string, string>> records)
    {
        var csvConfig = CsvUtils.GetDefaultConfiguration();

        using var writer = new StreamWriter(filePath);
        using var csvWriter = new CsvWriter(writer, csvConfig);

        // Write headers
        foreach (var header in headers)
        {
            csvWriter.WriteField(header);
        }
        await csvWriter.NextRecordAsync();

        // Write data
        foreach (var record in records)
        {
            foreach (var header in headers)
            {
                csvWriter.WriteField(record.GetValueOrDefault(header, string.Empty));
            }
            await csvWriter.NextRecordAsync();
        }
    }
}