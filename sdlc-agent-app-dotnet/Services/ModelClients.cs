using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;
using System.Text;
using System.Text.Json;

namespace SdlcAgentApp.Services;

public interface IModelClient
{
    Task<string> CallAsync(string model, string systemPrompt, string userPrompt);
}

public sealed class OpenAiClient(string apiKey, string baseUrl) : IModelClient
{
    private readonly string _apiKey = apiKey;
    private readonly string _baseUrl = baseUrl;

    public async Task<string> CallAsync(string model, string systemPrompt, string userPrompt)
    {
        var options = new OpenAIClientOptions();
        if (!string.IsNullOrWhiteSpace(_baseUrl))
        {
            options.Endpoint = new Uri(_baseUrl);
        }

        var openAiClient = new OpenAIClient(new ApiKeyCredential(_apiKey), options);
        IChatClient chatClient = openAiClient.GetChatClient(model).AsIChatClient();

        var response = await chatClient.GetResponseAsync(
            [
                new ChatMessage(ChatRole.System, systemPrompt),
                new ChatMessage(ChatRole.User, userPrompt),
            ]);

        var text = response.Text?.Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException("No text output found in OpenAI chat response.");
        }

        return text;
    }
}

public sealed class GitHubModelsClient(
    HttpClient http,
    string token,
    string baseUrl,
    string githubOrg,
    string githubApiVersion) : IModelClient
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
