namespace FilePrepper;

public class FilePrepperOptions
{
    public bool EnableLogging { get; set; } = true;
    public int MaxFileSizeInMb { get; set; } = 100;
    public Dictionary<string, string> DefaultConverterOptions { get; set; }
        = new Dictionary<string, string>();
}