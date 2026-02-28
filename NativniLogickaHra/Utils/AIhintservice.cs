using Google.GenAI;
using NativniLogickaHra.Utils;

namespace NativniLogickaHra.Services;

/// <summary>
/// Generuje nápovědu od AI pro aktuálně hádané slovo.
/// Nápověda NEODHALÍ slovo — AI popíše co to je bez přímého pojmenování.
/// </summary>
public static class AiHintService
{
    public static async Task<string> GetHintAsync(string provider, string apiKey, string word, string language)
    {
        string langInstruction = language == "cs"
            ? "Odpověz česky."
            : "Answer in English.";

        string prompt =
            $"The hidden word in a hangman game is \"{word}\". " +
            $"Give a short, helpful hint (1-2 sentences) that describes what the word means " +
            $"WITHOUT saying the word itself or its direct translation. " +
            $"Do not reveal the word. {langInstruction}";

        try
        {
            return provider switch
            {
                "Gemini" => await CallGemini(apiKey, prompt),
                "ChatGPT" => await CallChatGpt(apiKey, prompt),
                "Claude" => await CallClaude(apiKey, prompt),
                _ => "?"
            };
        }
        catch (Exception ex)
        {
            Logger.Log($"AiHintService error: {ex.Message}");
            return "?";
        }
    }

    // ── Gemini — SDK (stejně jako AiWordService) ──────────────────────────────
    private static async Task<string> CallGemini(string apiKey, string prompt)
    {
        Environment.SetEnvironmentVariable("GOOGLE_API_KEY", apiKey);
        using var client = new Google.GenAI.Client();
        var response = await client.Models.GenerateContentAsync(
            model: AiConfig.GeminiModel,
            contents: prompt
        );
        string? text = response?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
        return text?.Trim() ?? "?";
    }

    // ── ChatGPT ───────────────────────────────────────────────────────────────
    private static async Task<string> CallChatGpt(string apiKey, string prompt)
    {
        var body = new
        {
            model = AiConfig.ChatGptModel,
            messages = new[] { new { role = "user", content = prompt } },
            max_tokens = 150
        };

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        using var response = await client.PostAsync(
            "https://api.openai.com/v1/chat/completions",
            new StringContent(
                System.Text.Json.JsonSerializer.Serialize(body),
                System.Text.Encoding.UTF8,
                "application/json"));

        var json = await response.Content.ReadAsStringAsync();
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        return doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString()?.Trim() ?? "?";
    }

    // ── Claude ────────────────────────────────────────────────────────────────
    private static async Task<string> CallClaude(string apiKey, string prompt)
    {
        var body = new
        {
            model = AiConfig.ClaudeModel,
            max_tokens = 150,
            messages = new[] { new { role = "user", content = prompt } }
        };

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("x-api-key", apiKey);
        client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
        using var response = await client.PostAsync(
            "https://api.anthropic.com/v1/messages",
            new StringContent(
                System.Text.Json.JsonSerializer.Serialize(body),
                System.Text.Encoding.UTF8,
                "application/json"));

        var json = await response.Content.ReadAsStringAsync();
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        return doc.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString()?.Trim() ?? "?";
    }
}