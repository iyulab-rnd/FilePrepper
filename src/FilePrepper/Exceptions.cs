namespace FilePrepper;

public enum TaskExecutionErrorCode
{
    General
}

public class TaskExecutionException : Exception
{
    public TaskExecutionErrorCode Code { get; set; }
    public TaskExecutionException(string? message, TaskExecutionErrorCode code) : base(message)
    {
        this.Code = code;
    }
}

public enum ValidationExceptionErrorCode
{
    General
}

public class ValidationException : Exception
{
    public ValidationExceptionErrorCode Code { get; set; }

    public ValidationException(string? message, ValidationExceptionErrorCode code) : base(message)
    {
        this.Code = code;
    }
}
