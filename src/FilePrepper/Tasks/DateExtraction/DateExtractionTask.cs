﻿using Microsoft.Extensions.Logging;
using System.Globalization;

namespace FilePrepper.Tasks.DateExtraction;

public class DateExtractionTask : BaseTask<DateExtractionOption>
{
    public DateExtractionTask(
        DateExtractionOption options,
        ILogger<DateExtractionTask> logger,
        ILogger<DateExtractionValidator> validatorLogger)
        : base(options, logger, new DateExtractionValidator(validatorLogger))
    {
    }

    protected override async Task<List<Dictionary<string, string>>> ProcessRecordsAsync(
        List<Dictionary<string, string>> records)
    {
        var processedRecords = new List<Dictionary<string, string>>();

        foreach (var record in records)
        {
            var newRecord = new Dictionary<string, string>(record);

            foreach (var extraction in Options.Extractions)
            {
                try
                {
                    ProcessDateExtraction(newRecord, extraction);
                }
                catch (FormatException ex)
                {
                    _logger.LogWarning(ex, "Failed to parse date value '{Value}' for column {Column}",
                        record[extraction.SourceColumn], extraction.SourceColumn);

                    // 실패한 컴포넌트들에 대해 빈 값 설정
                    foreach (var component in extraction.Components)
                    {
                        var columnName = GetOutputColumnName(extraction, component);
                        newRecord[columnName] = string.Empty;
                    }

                    if (!Options.IgnoreErrors)
                    {
                        throw;
                    }
                }
            }

            processedRecords.Add(newRecord);
        }

        return processedRecords;
    }

    private void ProcessDateExtraction(Dictionary<string, string> record, DateColumnExtraction extraction)
    {
        if (!record.ContainsKey(extraction.SourceColumn) ||
            string.IsNullOrWhiteSpace(record[extraction.SourceColumn]))
        {
            foreach (var component in extraction.Components)
            {
                var columnName = GetOutputColumnName(extraction, component);
                record[columnName] = string.Empty;
            }
            return;
        }

        var value = record[extraction.SourceColumn];
        var culture = extraction.Culture ?? CultureInfo.InvariantCulture;
        var dateTime = extraction.DateFormat != null
            ? DateTime.ParseExact(value, extraction.DateFormat, culture)
            : DateTime.Parse(value, culture);

        foreach (var component in extraction.Components)
        {
            var componentValue = ExtractComponent(dateTime, component);
            var columnName = GetOutputColumnName(extraction, component);
            record[columnName] = componentValue;
        }
    }

    private string ExtractComponent(DateTime date, DateComponent component) =>
        component switch
        {
            DateComponent.Year => date.Year.ToString(),
            DateComponent.Month => date.Month.ToString(),
            DateComponent.Day => date.Day.ToString(),
            DateComponent.Hour => date.Hour.ToString(),
            DateComponent.Minute => date.Minute.ToString(),
            DateComponent.Second => date.Second.ToString(),
            DateComponent.DayOfWeek => ((int)date.DayOfWeek).ToString(),
            DateComponent.WeekOfYear => ISOWeek.GetWeekOfYear(date).ToString(),
            DateComponent.Quarter => ((date.Month - 1) / 3 + 1).ToString(),
            DateComponent.DayOfYear => date.DayOfYear.ToString(),
            _ => throw new ArgumentException($"Unsupported date component: {component}")
        };

    private string GetOutputColumnName(DateColumnExtraction extraction, DateComponent component)
    {
        if (!Options.Common.AppendToSource)
        {
            return extraction.OutputColumnTemplate!
                .Replace("{column}", extraction.SourceColumn)
                .Replace("{component}", component.ToString());
        }

        return Options.Common.OutputColumnTemplate!
            .Replace("{column}", extraction.SourceColumn)
            .Replace("{component}", component.ToString());
    }
}