namespace FilePrepper.Tasks.DateExtraction;

public enum DateComponent
{
    Year,
    Month,
    Day,
    Hour,
    Minute,
    Second,
    DayOfWeek,
    WeekOfYear,
    Quarter,
    DayOfYear
}

public class DateColumnExtraction
{
    public string SourceColumn { get; set; } = string.Empty;
    public string? DateFormat { get; set; }
    public CultureInfo? Culture { get; set; }
    public List<DateComponent> Components { get; set; } = new();
    public string? OutputColumnTemplate { get; set; }
}

public class DateExtractionOption : SingleInputOption, IAppendableOption
{
    public List<DateColumnExtraction> Extractions { get; set; } = new();

    // IAppendableOption implementation
    public bool AppendToSource { get; set; }
    public string? OutputColumnTemplate { get; set; }

    protected override string[] ValidateInternal()
    {
        var errors = new List<string>();

        if (Extractions == null || Extractions.Count == 0)
        {
            errors.Add("At least one date extraction must be specified");
            return [.. errors];
        }

        foreach (var extraction in Extractions)
        {
            if (string.IsNullOrWhiteSpace(extraction.SourceColumn))
            {
                errors.Add("Source column name cannot be empty");
            }

            if (extraction.Components == null || extraction.Components.Count == 0)
            {
                errors.Add($"At least one component must be specified for column {extraction.SourceColumn}");
            }

            if (string.IsNullOrWhiteSpace(extraction.OutputColumnTemplate) && !AppendToSource)
            {
                errors.Add($"Output column template must be specified for column {extraction.SourceColumn} when not appending to source");
            }
        }

        if (AppendToSource && string.IsNullOrWhiteSpace(OutputColumnTemplate))
        {
            errors.Add("OutputColumnTemplate is required when AppendToSource is true");
        }

        return [.. errors];
    }
}