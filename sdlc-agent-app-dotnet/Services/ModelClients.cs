using System.Text;
using System.Text.Json;

namespace SdlcAgentApp.Services;

public interface IModelClient
{
    Task<string> CallAsync(string model, string systemPrompt, string userPrompt);
}

public sealed class OpenAiClient(HttpClient http, string apiKey, string baseUrl) : IModelClient
{
    public async Task<string> CallAsync(string model, string systemPrompt, string userPrompt)
    {
        var url = $"{baseUrl.TrimEnd('/')}/responses";
        var body = new
        {
            model,
            input = new object[]
            {
                new
                {
                    role = "system",
                    content = new object[] { new { type = "input_text", text = systemPrompt } }
                },
                new
                {
                    role = "user",
                    content = new object[] { new { type = "input_text", text = userPrompt } }
                }
            }
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, url);
        req.Headers.Add("Authorization", $"Bearer {apiKey}");
        req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        using var resp = await http.SendAsync(req);
        var txt = await resp.Content.ReadAsStringAsync();
        if (!resp.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"API HTTP {(int)resp.StatusCode}: {txt}");
        }

        using var doc = JsonDocument.Parse(txt);
        var output = doc.RootElement.GetProperty("output");
        var sb = new StringBuilder();
        foreach (var item in output.EnumerateArray())
        {
            if (!item.TryGetProperty("type", out var type) || type.GetString() != "message")
            {
                continue;
            }
            if (!item.TryGetProperty("content", out var content))
            {
                continue;
            }
            foreach (var c in content.EnumerateArray())
            {
                if (c.TryGetProperty("type", out var ct) && ct.GetString() == "output_text")
                {
                    if (c.TryGetProperty("text", out var t) && !string.IsNullOrWhiteSpace(t.GetString()))
                    {
                        sb.AppendLine(t.GetString());
                    }
                }
            }
        }

        var result = sb.ToString().Trim();
        if (string.IsNullOrWhiteSpace(result))
        {
            throw new InvalidOperationException($"No text output found in API response: {txt}");
        }
        return result;
    }
}

public sealed class GitHubModelsClient(
    HttpClient http,
    string token,
    string baseUrl,
    string githubOrg,
    string githubApiVersion = "2026-03-10") : IModelClient
{
    public async Task<string> CallAsync(string model, string systemPrompt, string userPrompt)
    {
        var url = string.IsNullOrWhiteSpace(githubOrg)
            ? $"{baseUrl.TrimEnd('/')}/inference/chat/completions"
            : $"{baseUrl.TrimEnd('/')}/orgs/{githubOrg}/inference/chat/completions";

        var body = new
        {
            model,
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            }
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, url);
        req.Headers.Add("Accept", "application/vnd.github+json");
        req.Headers.Add("Authorization", $"Bearer {token}");
        req.Headers.Add("X-GitHub-Api-Version", githubApiVersion);
        req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        using var resp = await http.SendAsync(req);
        var txt = await resp.Content.ReadAsStringAsync();
        if (!resp.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"API HTTP {(int)resp.StatusCode}: {txt}");
        }

        using var doc = JsonDocument.Parse(txt);
        var content = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException($"No text output found in API response: {txt}");
        }
        return content.Trim();
    }
}

