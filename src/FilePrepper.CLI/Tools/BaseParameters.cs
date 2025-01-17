//using CommandLine;
//using FilePrepper.Tasks;
//using Microsoft.Extensions.Logging;

//namespace FilePrepper.CLI.Tools;


///// <summary>
///// CLI 명령어의 기본 매개변수 클래스
///// </summary>
//public abstract class BaseParameters : ICommandParameters
//{
//    [Option("has-header", Default = true,
//        HelpText = "Whether input files have headers")]
//    public bool HasHeader { get; set; } = true;

//    [Option("ignore-errors", Required = false, Default = false,
//        HelpText = "Whether to ignore errors during processing")]
//    public bool IgnoreErrors { get; set; }

//    [Option('o', "output", Required = true,
//        HelpText = "Output file path")]
//    public string OutputPath { get; set; } = string.Empty;

//    public abstract Type GetHandlerType();

//    public virtual bool Validate(ILogger logger)
//    {
//        if (!ValidateOutputPath(OutputPath, logger))
//        {
//            return false;
//        }

//        return ValidateInternal(logger);
//    }

//    protected virtual bool ValidateInternal(ILogger logger) => true;

//    protected bool ValidateOutputPath(string outputPath, ILogger logger)
//    {
//        if (string.IsNullOrEmpty(outputPath))
//        {
//            logger.LogError("Output path is not specified");
//            return false;
//        }

//        var outputDir = Path.GetDirectoryName(outputPath);
//        if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
//        {
//            logger.LogError($"Output directory does not exist: {outputDir}");
//            return false;
//        }

//        return true;
//    }

//    protected bool ValidateInputPath(string inputPath, ILogger logger)
//    {
//        if (string.IsNullOrEmpty(inputPath))
//        {
//            logger.LogError("Input path is not specified");
//            return false;
//        }

//        if (!File.Exists(inputPath))
//        {
//            logger.LogError($"Input file does not exist: {inputPath}");
//            return false;
//        }

//        return true;
//    }

//    /// <summary>
//    /// 공통 TaskOption 속성들을 설정합니다.
//    /// </summary>
//    protected void ConfigureBaseOption(BaseOption option)
//    {
//        option.HasHeader = HasHeader;
//        option.IgnoreErrors = IgnoreErrors;
//        option.OutputPath = OutputPath;
//    }
//}

///// <summary>
///// 단일 입력 파일을 처리하는 명령어를 위한 기본 클래스
///// </summary>
//public abstract class SingleInputParameters : BaseParameters
//{
//    [Option('i', "input", Required = true,
//        HelpText = "Input file path")]
//    public string InputPath { get; set; } = string.Empty;

//    [Option("default-value", Required = false,
//        HelpText = "Default value to use when encountering errors")]
//    public string? DefaultValue { get; set; }

//    [Option("append-to-source", Required = false, Default = false,
//        HelpText = "Whether to append the result to source columns")]
//    public bool AppendToSource { get; set; }

//    [Option("output-column", Required = false,
//        HelpText = "Template for the output column name")]
//    public string? OutputColumnTemplate { get; set; }

//    protected override bool ValidateInternal(ILogger logger)
//    {
//        return ValidateInputPath(InputPath, logger);
//    }

//    protected void ConfigureSingleInputOption(SingleInputOption option)
//    {
//        ConfigureBaseOption(option);
//        option.InputPath = InputPath;

//        if (option is IDefaultValueOption defaultValueOption)
//        {
//            defaultValueOption.DefaultValue = DefaultValue;
//        }

//        if (option is IAppendableOption appendableOption)
//        {
//            appendableOption.AppendToSource = AppendToSource;
//            appendableOption.OutputColumnTemplate = OutputColumnTemplate;
//        }
//    }
//}

///// <summary>
///// 다중 입력 파일을 처리하는 명령어를 위한 기본 클래스
///// </summary>
//public abstract class MultipleInputParameters : BaseParameters
//{
//    [Value(0, Required = true, Min = 2,
//        HelpText = "Input CSV files to merge (minimum 2 files required)")]
//    public IEnumerable<string> InputFiles { get; set; } = [];

//    [Option("default-value", Required = false,
//        HelpText = "Default value to use when encountering errors")]
//    public string? DefaultValue { get; set; }

//    [Option("append-to-source", Required = false, Default = false,
//        HelpText = "Whether to append the result to source columns")]
//    public bool AppendToSource { get; set; }

//    [Option("output-column", Required = false,
//        HelpText = "Template for the output column name")]
//    public string? OutputColumnTemplate { get; set; }

//    protected override bool ValidateInternal(ILogger logger)
//    {
//        var inputList = InputFiles.ToList();
//        if (inputList.Count < 2)
//        {
//            logger.LogError("At least two input files are required");
//            return false;
//        }

//        return inputList.All(path => ValidateInputPath(path, logger));
//    }

//    protected void ConfigureMultipleInputOption(MultipleInputOption option)
//    {
//        ConfigureBaseOption(option);
//        option.InputPaths = InputFiles.ToList();

//        if (option is IDefaultValueOption defaultValueOption)
//        {
//            defaultValueOption.DefaultValue = DefaultValue;
//        }

//        if (option is IAppendableOption appendableOption)
//        {
//            appendableOption.AppendToSource = AppendToSource;
//            appendableOption.OutputColumnTemplate = OutputColumnTemplate;
//        }
//    }
//}