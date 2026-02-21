using System.Net.Http.Json;
using System.Linq;
using System.Text.Json;
using NativniLogickaHra.Utils;

namespace NativniLogickaHra.Services;

public static class AiWordService
{
    private static readonly RateLimiter GeminiLimiter = new RateLimiter(5, TimeSpan.FromMinutes(1));

    private const int MaxRetries = 3; // kolikrát zkusit znovu při nevhodném slově

    private static readonly string[] Themes =
    {
        "zvíře", "předmět", "povolání", "příroda", "město", "technologie", "sport", "jídlo", "činnost", "náhodné slovo"
    };

    private static string BuildPrompt()
    {
        var theme = Themes[Random.Shared.Next(Themes.Length)];
        return $"Vrať jedno náhodné české slovo z kategorie {theme}. Pouze slovo, bez uvozovek a vysvětlení.";
    }

    public static async Task<string?> GetWordAsync(string provider, string apiKey)
    {
        Logger.Log($"GetWordAsync start - provider: {provider}");
        try
        {
            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                string? word = provider switch
                {
                    "Gemini" => await FromGemini(apiKey),
                    "ChatGPT" => await FromChatGPT(apiKey),
                    "Claude" => await FromClaude(apiKey),
                    _ => null
                };

                if (WordFilter.IsAllowed(word))
                {
                    Logger.Log($"GetWordAsync: accepted word '{word}' on attempt {attempt}");
                    WordHistory.Add(word);
                    return word;
                }

                Logger.Log($"GetWordAsync: word '{word}' rejected by filter (attempt {attempt}/{MaxRetries})");
            }

            Logger.Log("GetWordAsync: all attempts returned filtered words, returning null");
            return null;
        }
        finally
        {
            Logger.Log($"GetWordAsync end - provider: {provider}");
        }
    }

    private static async Task<string?> FromGemini(string apiKey)
    {
        if (!GeminiLimiter.TryAcquire(out var retryAfter))
            throw new Exception($"Limit vyčerpán. Zkuste za {retryAfter.Seconds}s");
        Logger.Log("FromGemini: calling Gemini SDK");
        Environment.SetEnvironmentVariable("GOOGLE_API_KEY", apiKey);

        using var client = new Google.GenAI.Client();
        var response = await client.Models.GenerateContentAsync(
            model: "gemini-3-flash-preview",
            contents: BuildPrompt()
        );

        string? text = response?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
        Logger.Log($"FromGemini response: {text}");
        return CleanWord(text);
    }

    private static async Task<string?> FromChatGPT(string apiKey)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

        var payload = new
        {
            model = "gpt-4.1-mini",
            input = BuildPrompt(),
            temperature = 1.1
        };

        Logger.Log("FromChatGPT: sending request");

        var response = await client.PostAsJsonAsync(
            "https://api.openai.com/v1/responses",
            payload
        );

        var raw = await response.Content.ReadAsStringAsync();
        Logger.Log($"FromChatGPT raw response: {raw}");

        if (!response.IsSuccessStatusCode) return null;

        try
        {
            var doc = JsonDocument.Parse(raw);
            string? extracted = null;
            try
            {
                var root = doc.RootElement;
                if (root.TryGetProperty("output", out var output) && output.ValueKind == JsonValueKind.Array && output.GetArrayLength() > 0)
                {
                    var first = output[0];
                    if (first.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.Array && content.GetArrayLength() > 0)
                    {
                        var c0 = content[0];
                        if (c0.TryGetProperty("text", out var textEl) && textEl.ValueKind == JsonValueKind.String)
                            extracted = textEl.GetString();
                    }
                }
            }
            catch { }

            return CleanWord(extracted);
        }
        catch
        {
            return null;
        }
    }

    private static async Task<string?> FromClaude(string apiKey)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("x-api-key", apiKey);
        client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

        var payload = new
        {
            model = "claude-3-haiku-20240307",
            max_tokens = 20,
            temperature = 1.0,
            messages = new[]
            {
                new { role = "user", content = BuildPrompt() }
            }
        };

        Logger.Log("FromClaude: sending request");

        var response = await client.PostAsJsonAsync(
            "https://api.anthropic.com/v1/messages",
            payload
        );

        var raw = await response.Content.ReadAsStringAsync();
        Logger.Log($"FromClaude raw response: {raw}");

        if (!response.IsSuccessStatusCode) return null;

        try
        {
            var doc = JsonDocument.Parse(raw);
            string? extracted = null;
            try
            {
                var root = doc.RootElement;
                if (root.TryGetProperty("content", out var contents) && contents.ValueKind == JsonValueKind.Array && contents.GetArrayLength() > 0)
                {
                    var first = contents[0];
                    if (first.TryGetProperty("text", out var text) && text.ValueKind == JsonValueKind.String)
                        extracted = text.GetString();
                }
            }
            catch { }

            return CleanWord(extracted);
        }
        catch
        {
            return null;
        }
    }

    private static string? CleanWord(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;

        var s = raw.Trim().Trim('"', '\'', '.', ',', '!', '?', '\n', '\r');

        // regex zahrnuje česká písmena
        var m = System.Text.RegularExpressions.Regex.Match(
            s,
            @"[a-záčďéěíňóřšťúůýž]+",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );

        return m.Success ? m.Value : null;
    }
}