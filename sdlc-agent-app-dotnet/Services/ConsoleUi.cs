namespace SdlcAgentApp.Services;

public interface IUserInterface
{
    string AskChoice(string label, IReadOnlyList<string> options, string @default);
    bool AskYesNo(string label, bool defaultYes);
    string AskRequired(string label);
    string AskMultiLine(string label);
    string AskOptional(string label, string defaultValue);
    void WriteLine(string message);
}

public sealed class ConsoleUi : IUserInterface
{
    public string AskChoice(string label, IReadOnlyList<string> options, string @default)
    {
        while (true)
        {
            var rendered = string.Join(", ", options.Select(o => o == @default ? $"{o} (default)" : o));
            Console.Write($"{label} [{rendered}]: ");
            var raw = (Console.ReadLine() ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(raw))
            {
                return @default;
            }
            if (options.Contains(raw))
            {
                return raw;
            }
            Console.WriteLine($"Invalid value: {raw}");
        }
    }

    public bool AskYesNo(string label, bool defaultYes)
    {
        var suffix = defaultYes ? "Y/n" : "y/N";
        while (true)
        {
            Console.Write($"{label} [{suffix}]: ");
            var raw = (Console.ReadLine() ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(raw))
            {
                return defaultYes;
            }
            if (raw is "y" or "yes")
            {
                return true;
            }
            if (raw is "n" or "no")
            {
                return false;
            }
            Console.WriteLine($"Invalid value: {raw}");
        }
    }

    public string AskRequired(string label)
    {
        while (true)
        {
            Console.Write($"{label}: ");
            var raw = (Console.ReadLine() ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(raw))
            {
                return raw;
            }
            Console.WriteLine("Value is required.");
        }
    }

    public string AskMultiLine(string label)
    {
        Console.WriteLine($"{label} (type END on a new line to finish)");
        var lines = new List<string>();
        while (true)
        {
            var line = Console.ReadLine() ?? string.Empty;
            if (line.Trim() == "END")
            {
                break;
            }
            lines.Add(line);
        }

        var text = string.Join(Environment.NewLine, lines).Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException("Input cannot be empty.");
        }

        return text;
    }

    public string AskOptional(string label, string defaultValue)
    {
        Console.Write($"{label} ");
        var raw = (Console.ReadLine() ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(raw) ? defaultValue : raw;
    }

    public void WriteLine(string message) => Console.WriteLine(message);
}

