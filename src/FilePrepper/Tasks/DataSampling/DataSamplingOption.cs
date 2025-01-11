namespace FilePrepper.Tasks.DataSampling;

public enum SamplingMethod
{
    Random,         // 임의 샘플링
    Systematic,     // 체계적 샘플링 (n번째 항목마다)
    Stratified     // 층화 샘플링 (특정 컬럼 기준)
}

public class DataSamplingOption : BaseOption
{
    public SamplingMethod Method { get; set; } = SamplingMethod.Random;
    public double SampleSize { get; set; }
    public int? Seed { get; set; }
    public string? StratifyColumn { get; set; }
    public int? SystematicInterval { get; set; }

    public bool IsSizeRatio => SampleSize > 0 && SampleSize < 1;

    protected override string[] ValidateInternal()
    {
        var errors = new List<string>();

        if (SampleSize <= 0)
        {
            errors.Add("Sample size must be greater than 0");
        }

        if (Method == SamplingMethod.Stratified && string.IsNullOrWhiteSpace(StratifyColumn))
        {
            errors.Add("Stratify column must be specified for stratified sampling");
        }

        if (Method == SamplingMethod.Systematic)
        {
            if (!SystematicInterval.HasValue || SystematicInterval.Value <= 0)
            {
                errors.Add("Systematic interval must be greater than 0");
            }
        }

        return [.. errors];
    }
}
