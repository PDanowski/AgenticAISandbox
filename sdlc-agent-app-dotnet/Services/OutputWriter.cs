namespace SdlcAgentApp.Services;

public sealed class OutputWriter
{
    private readonly string _outDir;

    public OutputWriter(string outDir)
    {
        _outDir = outDir;
        Directory.CreateDirectory(_outDir);
    }

    public string Write(string fileName, string content)
    {
        var full = Path.Combine(_outDir, fileName);
        File.WriteAllText(full, content);
        return Path.GetFullPath(full);
    }
}

