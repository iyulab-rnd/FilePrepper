using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Text.RegularExpressions;

namespace FilePrepper.Pipelines;

public class MlPreprocessingPipeline : IConversionPipeline
{
    private readonly ILogger<MlPreprocessingPipeline> _logger;
    private readonly MlPreprocessingOptions _options;

    public MlPreprocessingPipeline(
        ILogger<MlPreprocessingPipeline> logger,
        IOptions<MlPreprocessingOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public async Task<Stream> ProcessAsync(Stream inputStream, ConversionOptions options)
    {
        _logger.LogInformation("Starting ML preprocessing pipeline");

        var outputStream = new MemoryStream();
        await using var writer = new StreamWriter(outputStream, options.OutputEncoding, leaveOpen: true);
        using var reader = new StreamReader(inputStream, options.OutputEncoding);

        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = options.Delimiter,
            HasHeaderRecord = options.IncludeHeaders
        };

        // 헤더 처리
        string[]? headers = null;
        if (options.IncludeHeaders)
        {
            var headerLine = await reader.ReadLineAsync();
            if (headerLine != null)
            {
                headers = headerLine.Split(options.Delimiter);
                await writer.WriteLineAsync(headerLine);
            }
        }

        // 데이터 통계 수집
        var columnStats = new Dictionary<int, ColumnStatistics>();
        var allRows = new List<string[]>();

        // 첫번째 패스: 통계 수집
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            var values = ParseCsvLine(line, options.Delimiter);
            allRows.Add(values);

            for (int i = 0; i < values.Length; i++)
            {
                if (!columnStats.ContainsKey(i))
                {
                    columnStats[i] = new ColumnStatistics();
                }

                var stats = columnStats[i];
                var value = values[i].Trim();

                if (string.IsNullOrWhiteSpace(value) || value.ToLower() == "null" || value.ToLower() == "nan")
                {
                    stats.NullCount++;
                }
                else if (double.TryParse(value, out double numericValue))
                {
                    stats.NumericValues.Add(numericValue);
                    stats.IsNumeric = true;
                }
                else
                {
                    stats.Categories.Add(value);
                }
            }
        }

        // 전처리 옵션 적용
        var processedRows = allRows.AsEnumerable();

        // 결측치가 많은 열 제거
        var columnsToKeep = columnStats
            .Where(kvp => (double)kvp.Value.NullCount / allRows.Count <= _options.MaxNullPercentage)
            .Select(kvp => kvp.Key)
            .ToList();

        // 결측치가 있는 행 제거 또는 대체
        if (_options.RemoveRowsWithMissingValues)
        {
            processedRows = processedRows.Where(row =>
                columnsToKeep.All(i => !IsNullOrMissing(row[i])));
        }
        else
        {
            processedRows = processedRows.Select(row =>
            {
                var newRow = row.ToArray();
                foreach (var colIndex in columnsToKeep)
                {
                    if (IsNullOrMissing(newRow[colIndex]))
                    {
                        var stats = columnStats[colIndex];
                        newRow[colIndex] = GetImputedValue(stats);
                    }
                }
                return newRow;
            });
        }

        // 이상치 제거 (수치형 컬럼에 대해)
        if (_options.RemoveOutliers)
        {
            var numericColumns = columnStats
                .Where(kvp => kvp.Value.IsNumeric && columnsToKeep.Contains(kvp.Key))
                .Select(kvp => kvp.Key)
                .ToList();

            processedRows = processedRows.Where(row =>
                numericColumns.All(i => !IsOutlier(row[i], columnStats[i])));
        }

        // 결과 쓰기
        foreach (var row in processedRows)
        {
            var filteredValues = columnsToKeep.Select(i => row[i]);
            await writer.WriteLineAsync(string.Join(options.Delimiter, filteredValues));
        }

        await writer.FlushAsync();
        outputStream.Position = 0;
        return outputStream;
    }

    private static bool IsNullOrMissing(string value)
    {
        value = value.Trim().ToLower();
        return string.IsNullOrWhiteSpace(value) || value == "null" || value == "nan";
    }

    private static bool IsOutlier(string value, ColumnStatistics stats)
    {
        if (!stats.IsNumeric || !double.TryParse(value, out double numericValue))
            return false;

        var q1 = stats.NumericValues.OrderBy(x => x).ElementAt((int)(stats.NumericValues.Count * 0.25));
        var q3 = stats.NumericValues.OrderBy(x => x).ElementAt((int)(stats.NumericValues.Count * 0.75));
        var iqr = q3 - q1;
        var lowerBound = q1 - (1.5 * iqr);
        var upperBound = q3 + (1.5 * iqr);

        return numericValue < lowerBound || numericValue > upperBound;
    }

    private static string GetImputedValue(ColumnStatistics stats)
    {
        if (stats.IsNumeric)
        {
            // 수치형 데이터는 중앙값으로 대체
            var median = stats.NumericValues.OrderBy(x => x)
                .ElementAt(stats.NumericValues.Count / 2);
            return median.ToString(CultureInfo.InvariantCulture);
        }
        else
        {
            // 범주형 데이터는 최빈값으로 대체
            return stats.Categories
                .GroupBy(x => x)
                .OrderByDescending(g => g.Count())
                .First()
                .Key;
        }
    }

    private static string[] ParseCsvLine(string line, string delimiter)
    {
        return Regex.Split(line, $"{delimiter}(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)")
            .Select(value => value.Trim('"'))
            .ToArray();
    }
}

public class ColumnStatistics
{
    public int NullCount { get; set; }
    public bool IsNumeric { get; set; }
    public List<double> NumericValues { get; } = new();
    public HashSet<string> Categories { get; } = new();
}

public class MlPreprocessingOptions
{
    public double MaxNullPercentage { get; set; } = 0.5; // 50%
    public bool RemoveRowsWithMissingValues { get; set; } = false;
    public bool RemoveOutliers { get; set; } = false;
    public bool EnableFeatureScaling { get; set; } = false;
}