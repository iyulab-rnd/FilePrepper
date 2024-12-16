using System.Text;

namespace FilePrepper.CLI;

public static class Utils
{
    public static string GetOutputFilePath(string? outputOption, string inputFile)
    {
        if (string.IsNullOrEmpty(outputOption))
        {
            var inputDir = Path.GetDirectoryName(inputFile) ?? Environment.CurrentDirectory;
            return Path.Combine(inputDir, $"{Path.GetFileNameWithoutExtension(inputFile)}_output.csv");
        }

        return Path.HasExtension(outputOption) ?
            outputOption :
            Path.Combine(outputOption, $"{Path.GetFileNameWithoutExtension(inputFile)}_output.csv");
    }

    public static Encoding GetEncoding(string encodingName)
    {
        return encodingName.ToLower() switch
        {
            "ascii" => Encoding.ASCII,
            "utf-16" => Encoding.Unicode,
            "utf-32" => Encoding.UTF32,
            _ => Encoding.UTF8
        };
    }
}