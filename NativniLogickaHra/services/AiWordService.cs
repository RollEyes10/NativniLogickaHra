using System.Net.Http.Json;
using System.Linq;
using Google.GenAI;

namespace NativniLogickaHra.Services;

public static class AiWordService
{
    private static readonly RateLimiter GeminiLimiter =
        new RateLimiter(5, TimeSpan.FromMinutes(1));

    public static async Task<string?> GetWordAsync(string provider, string apiKey)
    {
        return provider switch
        {
            "Gemini" => await FromGemini(apiKey),
            "ChatGPT" => await FromChatGPT(apiKey),
            "Claude" => await FromClaude(apiKey),
            _ => null
        };
    }
    public class RateLimitException : Exception
    {
        public TimeSpan RetryAfter { get; }

        public RateLimitException(TimeSpan retryAfter)
            : base($"Limit vyčerpán. Zkuste za {retryAfter.Seconds} s.")
        {
            RetryAfter = retryAfter;
        }
    }

    // ---------- GEMINI ----------
    private static async Task<string?> FromGemini(string apiKey)
    {
        if (!GeminiLimiter.TryAcquire(out var retryAfter))
            throw new RateLimitException(retryAfter);

        Environment.SetEnvironmentVariable("GOOGLE_API_KEY", apiKey);

        using var client = new Google.GenAI.Client();
        var response = await client.Models.GenerateContentAsync(
            model: "gemini-3-flash-preview",
            contents: "Vrať jedno české slovo bez diakritiky pro hru šibenice. Jen slovo."
        );

        string? text = response?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
        return CleanWord(text);
    }

    // ---------- CHATGPT ----------
    private static async Task<string?> FromChatGPT(string apiKey)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

        var payload = new
        {
            model = "gpt-4.1-mini",
            input = "Vrať jedno české slovo bez diakritiky pro hru šibenice. Jen slovo.",
            temperature = 0.8
        };

        var response = await client.PostAsJsonAsync(
            "https://api.openai.com/v1/responses",
            payload
        );

        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadFromJsonAsync<dynamic?>();
        string? raw = null;
        try
        {
            raw = json?.output?[0]?.content?[0]?.text as string;
        }
        catch
        {
            raw = null;
        }

        return CleanWord(raw);
    }

    // ---------- CLAUDE ----------
    private static async Task<string?> FromClaude(string apiKey)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("x-api-key", apiKey);
        client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

        var payload = new
        {
            model = "claude-3-haiku-20240307",
            max_tokens = 20,
            temperature = 0.8,
            messages = new[]
            {
                new { role = "user", content = "Vrať jedno české slovo bez diakritiky pro hru šibenice. Jen slovo." }
            }
        };

        var response = await client.PostAsJsonAsync(
            "https://api.anthropic.com/v1/messages",
            payload
        );

        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadFromJsonAsync<dynamic?>();
        string? raw = null;
        try
        {
            raw = json?.content?[0]?.text as string;
        }
        catch
        {
            raw = null;
        }

        return CleanWord(raw);
    }

    // Normalize and extract a single ASCII word (no diacritics). Returns null if no valid word.
    private static string? CleanWord(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;

        // Trim and remove surrounding quotes/punctuation
        var s = raw.Trim().Trim('"', '\'', '\n', '\r');

        // Remove diacritics
        s = RemoveDiacritics(s);

        // Lowercase and find first alpha token
        s = s.ToLowerInvariant();
        var m = System.Text.RegularExpressions.Regex.Match(s, "[a-z]+", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (!m.Success) return null;
        return m.Value;
    }

    private static string RemoveDiacritics(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        var normalized = text.Normalize(System.Text.NormalizationForm.FormD);
        var sb = new System.Text.StringBuilder();
        foreach (var ch in normalized)
        {
            var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
            if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }
        return sb.ToString().Normalize(System.Text.NormalizationForm.FormC);
    }
}
