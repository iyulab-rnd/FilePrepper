using CsvHelper;

namespace FilePrepper.Tasks;

public abstract class BaseTask<TOption> : ITask where TOption : BaseOption
{
    protected readonly ILogger _logger;
    protected readonly IOptionValidator _validator;

    public string Name => GetType().Name.Replace("Task", string.Empty);
    public TOption Options { get; }
    ITaskOption ITask.Options => Options;

    protected List<string> _originalHeaders = [];

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
            _logger.LogInformation("Reading input file: {InputPath}", context.InputPath);
            var records = await ReadAndPreProcessAsync(context);

            try
            {
                records = await ProcessRecordsAsync(records);
                await PostProcessAndSaveAsync(records, context);
                return true;
            }
            catch (Exception ex) when (Options.IgnoreErrors)
            {
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
        var (records, headers) = await ReadCsvFileAndHeadersAsync(context.InputPath, GetRequiredColumns());
        _originalHeaders = headers;
        return await PreProcessRecordsAsync(records);
    }

    private async Task<(List<Dictionary<string, string>> Records, List<string> Headers)>
        ReadCsvFileAndHeadersAsync(string filePath, IEnumerable<string>? requiredColumns = null)
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

        return (records, headers);
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

    protected abstract Task<List<Dictionary<string, string>>> ProcessRecordsAsync(
        List<Dictionary<string, string>> records);

    protected virtual IEnumerable<string> GetRequiredColumns() =>
        Options is IColumnOption columnOption ? columnOption.TargetColumns : Array.Empty<string>();

    protected async Task<List<Dictionary<string, string>>> ReadCsvFileAsync(
        string filePath,
        IEnumerable<string>? requiredColumns = null)
    {
        var (records, _) = await ReadCsvFileAndHeadersAsync(filePath, requiredColumns);
        return records;
    }

    protected virtual async Task WriteOutputAsync(
        string outputPath,
        IEnumerable<string> headers,
        IEnumerable<Dictionary<string, string>> records)
    {
        var finalHeaders = headers.Any() ? headers : _originalHeaders;
        if (!finalHeaders.Any())
        {
            finalHeaders = new[] { "NoData" };
        }

        await using var writer = new StreamWriter(outputPath);
        await using var csv = new CsvWriter(writer, CsvUtils.GetDefaultConfiguration());

        // Write headers
        foreach (var header in finalHeaders)
        {
            csv.WriteField(header);
        }
        csv.NextRecord();

        // Write data rows
        foreach (var record in records)
        {
            foreach (var header in finalHeaders)
            {
                csv.WriteField(record.GetValueOrDefault(header, string.Empty));
            }
            csv.NextRecord();
        }
    }

    private async Task PostProcessAndSaveAsync(
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
