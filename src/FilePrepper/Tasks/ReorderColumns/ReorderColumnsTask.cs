using Microsoft.Extensions.Logging;
using CsvHelper;

namespace FilePrepper.Tasks.ReorderColumns
{
    public class ReorderColumnsTask : BaseTask<ReorderColumnsOption>
    {
        public ReorderColumnsTask(
            ReorderColumnsOption options,
            ILogger<ReorderColumnsTask> logger)
            : base(options, logger)
        {
        }

        protected override Task<List<Dictionary<string, string>>> ProcessRecordsAsync(
            List<Dictionary<string, string>> records)
        {
            _logger.LogInformation("Reordering columns as specified");

            var desiredOrder = Options.Order;
            var newHeaderOrder = new List<string>();

            // 재정렬된 헤더 순서 생성
            foreach (var col in desiredOrder)
            {
                if (_originalHeaders.Contains(col))
                {
                    newHeaderOrder.Add(col);
                }
            }

            // 지정되지 않은 나머지 컬럼들 추가
            foreach (var col in _originalHeaders)
            {
                if (!newHeaderOrder.Contains(col))
                {
                    newHeaderOrder.Add(col);
                }
            }

            _originalHeaders = newHeaderOrder;

            return Task.FromResult(records);
        }

        protected override async Task WriteOutputAsync(
            string outputPath,
            IEnumerable<string> headers,
            IEnumerable<Dictionary<string, string>> records)
        {
            string[] finalHeaders = [.. _originalHeaders];
            if (finalHeaders.Length == 0)
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
    }
}
