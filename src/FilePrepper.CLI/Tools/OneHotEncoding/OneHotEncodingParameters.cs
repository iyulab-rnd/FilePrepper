using CommandLine;
using FilePrepper.Tasks.OneHotEncoding;
using Microsoft.Extensions.Logging;

namespace FilePrepper.CLI.Tools.OneHotEncoding;

[Verb("one-hot-encoding", HelpText = "Perform one-hot encoding on categorical columns")]
public class OneHotEncodingParameters : BaseColumnParameters
{
    [Option("drop-first", Default = false,
        HelpText = "Drop first category to avoid dummy variable trap")]
    public bool DropFirst { get; set; }

    [Option("keep-original", Default = false,
        HelpText = "Keep original columns alongside encoded ones")]
    public bool KeepOriginalColumns { get; set; }

    public override Type GetHandlerType() => typeof(OneHotEncodingHandler);

    protected override bool ValidateInternal(ILogger logger)
    {
        if (!base.ValidateInternal(logger))
            return false;

        // 각 컬럼명 검증
        foreach (var column in TargetColumns)
        {
            if (string.IsNullOrWhiteSpace(column))
            {
                logger.LogError("Column name cannot be empty or whitespace");
                return false;
            }
        }

        return true;
    }

    public override string? GetExample() =>
        "one-hot-encoding -i input.csv -o output.csv -c \"Category,Status\" --keep-original";
}