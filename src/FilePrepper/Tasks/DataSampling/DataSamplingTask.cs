using Microsoft.Extensions.Logging;

namespace FilePrepper.Tasks.DataSampling;

public class DataSamplingTask : BaseTask<DataSamplingOption>
{
    public DataSamplingTask(
        DataSamplingOption options,
        ILogger<DataSamplingTask> logger,
        ILogger<DataSamplingValidator> validatorLogger)
        : base(options, logger, new DataSamplingValidator(validatorLogger))
    {
    }

    protected override async Task<List<Dictionary<string, string>>> ProcessRecordsAsync(
        List<Dictionary<string, string>> records)
    {
        int sampleSize = CalculateSampleSize(records.Count);

        _logger.LogInformation(
            "Sampling {SampleSize} records from {TotalSize} records",
            sampleSize,
            records.Count);

        var random = Options.Seed.HasValue ? new Random(Options.Seed.Value) : new Random();

        return Options.Method switch
        {
            SamplingMethod.Random => await GetRandomSampleAsync(records, sampleSize, random),
            SamplingMethod.Systematic => await GetSystematicSampleAsync(records),
            SamplingMethod.Stratified => await GetStratifiedSampleAsync(records, sampleSize, random),
            _ => throw new ArgumentException($"Unsupported sampling method: {Options.Method}")
        };
    }

    private int CalculateSampleSize(int totalCount)
    {
        return Options.IsSizeRatio
            ? Math.Max(1, (int)(totalCount * Options.SampleSize))
            : Math.Min(totalCount, (int)Options.SampleSize);
    }

    private async Task<List<Dictionary<string, string>>> GetRandomSampleAsync(
        List<Dictionary<string, string>> records,
        int sampleSize,
        Random random)
    {
        return await Task.Run(() =>
            records.OrderBy(_ => random.NextDouble())
                   .Take(sampleSize)
                   .ToList());
    }

    private async Task<List<Dictionary<string, string>>> GetSystematicSampleAsync(
        List<Dictionary<string, string>> records)
    {
        if (!Options.SystematicInterval.HasValue)
        {
            throw new InvalidOperationException("Systematic interval must be specified");
        }

        return await Task.Run(() =>
            records.Where((_, index) => index % Options.SystematicInterval.Value == 0)
                   .ToList());
    }

    private async Task<List<Dictionary<string, string>>> GetStratifiedSampleAsync(
        List<Dictionary<string, string>> records,
        int totalSampleSize,
        Random random)
    {
        if (string.IsNullOrWhiteSpace(Options.StratifyColumn))
        {
            throw new InvalidOperationException("Stratify column must be specified");
        }

        return await Task.Run(() =>
        {
            var stratifiedGroups = records
                .GroupBy(r => r[Options.StratifyColumn!])
                .ToDictionary(g => g.Key, g => g.ToList());

            var result = new List<Dictionary<string, string>>();
            var ratio = Options.IsSizeRatio ? Options.SampleSize : (double)totalSampleSize / records.Count;

            foreach (var group in stratifiedGroups)
            {
                var groupSampleSize = Math.Max(1, (int)(group.Value.Count * ratio));
                var groupSample = group.Value
                    .OrderBy(_ => random.NextDouble())
                    .Take(groupSampleSize)
                    .ToList();
                result.AddRange(groupSample);
            }

            return result;
        });
    }

    protected override IEnumerable<string> GetRequiredColumns()
    {
        return Options.Method == SamplingMethod.Stratified && !string.IsNullOrWhiteSpace(Options.StratifyColumn)
            ? new[] { Options.StratifyColumn }
            : Array.Empty<string>();
    }
}
