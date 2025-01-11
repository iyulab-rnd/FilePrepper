using Microsoft.Extensions.Logging;

namespace FilePrepper.Tasks.DataTypeConvert;

public class DataTypeConvertValidator : BaseValidator<DataTypeConvertOption>
{
    public DataTypeConvertValidator(ILogger<DataTypeConvertValidator> logger)
        : base(logger)
    {
    }
}