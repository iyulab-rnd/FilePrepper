namespace FilePrepper.Tasks.BasicStatistics;

public class BasicStatisticsValidator : BaseValidator<BasicStatisticsOption>
{
    public BasicStatisticsValidator(ILogger<BasicStatisticsValidator> logger)
        : base(logger)
    {
    }
}