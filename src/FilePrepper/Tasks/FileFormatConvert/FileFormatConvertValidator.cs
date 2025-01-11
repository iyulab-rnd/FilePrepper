namespace FilePrepper.Tasks.FileFormatConvert;

public class FileFormatConvertValidator : BaseValidator<FileFormatConvertOption>
{
    public FileFormatConvertValidator(ILogger<FileFormatConvertValidator> logger)
        : base(logger)
    {
    }
}