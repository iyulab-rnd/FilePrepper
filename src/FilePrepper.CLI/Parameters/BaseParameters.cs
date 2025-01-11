using CommandLine;
using FilePrepper.Tasks;

namespace FilePrepper.CLI.Parameters;

public abstract class BaseParameters : ICommandParameters
{
    [Option('i', "input", Required = true, HelpText = "Input file path")]
    public string InputPath { get; set; } = string.Empty;

    [Option('o', "output", Required = true, HelpText = "Output file path")]
    public string OutputPath { get; set; } = string.Empty;

    [Option("ignore-errors", Required = false, Default = false,
        HelpText = "Whether to ignore errors during processing")]
    public bool IgnoreErrors { get; set; }

    [Option("default-value", Required = false,
        HelpText = "Default value to use when encountering errors")]
    public string? DefaultValue { get; set; }

    public abstract Type GetHandlerType();

    public CommonTaskOptions GetCommonOptions() => new()
    {
        ErrorHandling = new()
        {
            IgnoreErrors = IgnoreErrors,
            DefaultValue = DefaultValue
        }
    };
}
