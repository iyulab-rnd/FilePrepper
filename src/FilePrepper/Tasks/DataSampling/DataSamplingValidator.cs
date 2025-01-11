namespace FilePrepper.Tasks.DataSampling;

public class DataSamplingValidator : BaseValidator<DataSamplingOption>
{
    public DataSamplingValidator(ILogger<DataSamplingValidator> logger)
        : base(logger)
    {
    }
}