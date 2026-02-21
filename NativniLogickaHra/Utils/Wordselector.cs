using System.Net.Http.Json;
using System.Text.Json;
using NativniLogickaHra.Utils;

namespace NativniLogickaHra.Services;

/// <summary>
/// Vybírá slovo výhradně přes AI API.
/// Obtížnost se odvíjí od statistik hráče (PlayerStats).
/// Výsledek vždy prochází WordFilter (bezpečnost) a WordHistory (anti-repeat).
/// </summary>
public static class WordSelector
{
    // ── Veřejné API ──────────────────────────────────────────────────────────

    /// <summary>
    /// Vrátí slovo vygenerované AI přizpůsobené aktuální obtížnosti hráče.
    /// </summary>
    public static async Task<string> GetWordForPlayerAsync(string provider, string apiKey)
    {
        int difficulty = PlayerStats.Difficulty; // 1 = Easy, 2 = Medium, 3 = Hard
        Logger.Log($"WordSelector: difficulty={difficulty}, streak={PlayerStats.Streak}, winRate={PlayerStats.WinRate:P0}");

        string? word = await GetFromAiAsync(provider, apiKey, difficulty);

        return word ?? FallbackForDifficulty(difficulty);
    }

    // ── AI generování se složitostním promptem ────────────────────────────────

    private static async Task<string?> GetFromAiAsync(string provider, string apiKey, int difficulty)
    {
        var prompt = BuildDifficultyPrompt(difficulty);

        for (int attempt = 1; attempt <= 3; attempt++)
        {
            string? raw = await CallAiDirectAsync(provider, apiKey, prompt);
            string? cleaned = CleanWord(raw);

            if (WordFilter.IsAllowed(cleaned) && !WordHistory.WasRecentlyUsed(cleaned!))
            {
                WordHistory.Add(cleaned!);
                Logger.Log($"WordSelector: AI word '{cleaned}' accepted (difficulty={difficulty}, attempt={attempt})");
                return cleaned;
            }

            Logger.Log($"WordSelector: AI word '{cleaned}' rejected (attempt {attempt}/3)");
        }

        return null;
    }

    /// <summary>
    /// Prompt se liší podle obtížnosti — délka slova roste s levelem.
    /// </summary>
    private static string BuildDifficultyPrompt(int difficulty) => difficulty switch
    {
        1 => "Vrať jedno krátké české podstatné jméno vhodné pro děti (3–5 písmen), " +
             "vhodné pro hru šibenice, bez diakritiky, pouze slovo. " +
             "Příklady: pes, kos, ryba, koza.",

        2 => "Vrať jedno středně dlouhé české podstatné jméno (6–8 písmen), " +
             "vhodné pro hru šibenice, bez diakritiky, pouze slovo. " +
             "Příklady: zahrada, vlajka, kometa.",

        _ => "Vrať jedno dlouhé nebo méně obvyklé české podstatné jméno (9+ písmen), " +
             "vhodné pro pokročilou hru šibenice, bez diakritiky, pouze slovo. " +
             "Příklady: orchestr, dalekohledy, vydavatelstvi.",
    };

    // ── Přímé AI volání ───────────────────────────────────────────────────────

    private static async Task<string?> CallAiDirectAsync(string provider, string apiKey, string prompt)
    {
        try
        {
            return provider switch
            {
                "Gemini" => await CallGeminiAsync(apiKey, prompt),
                "ChatGPT" => await CallChatGptAsync(apiKey, prompt),
                "Claude" => await CallClaudeAsync(apiKey, prompt),
                _ => null
            };
        }
        catch (Exception ex)
        {
            Logger.Log($"WordSelector: AI call error — {ex.Message}");
            return null;
        }
    }

    private static async Task<string?> CallGeminiAsync(string apiKey, string prompt)
    {
        Environment.SetEnvironmentVariable("GOOGLE_API_KEY", apiKey);
        using var client = new Google.GenAI.Client();
        var response = await client.Models.GenerateContentAsync(
            model: AiConfig.GeminiModel,
            contents: prompt
        );
        return response?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
    }

    private static async Task<string?> CallChatGptAsync(string apiKey, string prompt)
    {
        using var http = new System.Net.Http.HttpClient();
        http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

        var payload = new { model = AiConfig.ChatGptModel, input = prompt, temperature = 1.0 };
        var response = await http.PostAsJsonAsync("https://api.openai.com/v1/responses", payload);
        var raw = await response.Content.ReadAsStringAsync();

        var doc = JsonSerializer.Deserialize<JsonElement>(raw);
        return doc.TryGetProperty("output", out var output) && output.GetArrayLength() > 0
            ? output[0].GetProperty("content")[0].GetProperty("text").GetString()
            : null;
    }

    private static async Task<string?> CallClaudeAsync(string apiKey, string prompt)
    {
        using var http = new System.Net.Http.HttpClient();
        http.DefaultRequestHeaders.Add("x-api-key", apiKey);
        http.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

        var payload = new
        {
            model = AiConfig.ClaudeModel,
            max_tokens = 20,
            messages = new[] { new { role = "user", content = prompt } }
        };

        var response = await http.PostAsJsonAsync("https://api.anthropic.com/v1/messages", payload);
        var raw = await response.Content.ReadAsStringAsync();

        var doc = JsonSerializer.Deserialize<JsonElement>(raw);
        return doc.TryGetProperty("content", out var content) && content.GetArrayLength() > 0
            ? content[0].GetProperty("text").GetString()
            : null;
    }

    // ── Pomocné ───────────────────────────────────────────────────────────────

    private static string FallbackForDifficulty(int difficulty) => difficulty switch
    {
        1 => "pes",
        2 => "zahrada",
        _ => "orchestr"
    };

    private static string? CleanWord(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var s = raw.Trim().Trim('"', '\'', '.', ',', '!', '?', '\n', '\r');
        var m = System.Text.RegularExpressions.Regex.Match(
            s, @"[a-záčďéěíňóřšťúůýž]+",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        return m.Success ? m.Value : null;
    }
}