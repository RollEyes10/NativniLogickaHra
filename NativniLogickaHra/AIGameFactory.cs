using System.Net.Http.Json;
using System.Text.Json;
using System.Linq;
using NativniLogickaHra;
using Google.GenAI;
using Google.GenAI.Types;

public class AIGameFactory
{
    public async Task<Hangman> CreateHangmanGameAsync(string provider)
    {
        // fallback slovo
        string word = "programovani";

        // načtení nastavení
        int lives = Preferences.Default.Get("LivesEnabled", true)
            ? Preferences.Default.Get("LivesCount", 6)
            : int.MaxValue;

        int vowelsCount = Preferences.Default.Get("VowelsEnabled", true)
            ? Preferences.Default.Get("VowelsCount", 3)
            : 0;

        bool vowelsEnabled = vowelsCount > 0;

        if (provider == "Gemini")
        {
            string? apiKey = await SecureStorage.Default.GetAsync("api_Gemini");
            if (string.IsNullOrEmpty(apiKey))
                throw new Exception("Chybí API klíč pro Gemini!");

            // 🌟 Gemini SDK - fully qualify to avoid ambiguity with other 'Client' types
            var client = new Google.GenAI.Client();

            var response = await client.Models.GenerateContentAsync(
                model: "gemini-3-flash-preview",
                contents: "Jedno české podstatné jméno pro hru šibenice, bez diakritiky, pouze slovo."
            );

            // safe null checks to avoid CS8602
            var candidateText = response?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
            if (!string.IsNullOrWhiteSpace(candidateText))
            {
                word = candidateText!; // already guarded by IsNullOrWhiteSpace
            }
        }
        else
        {
            // REST API pro ChatGPT / Claude
            using var client = new HttpClient();
            string url = "";
            object payload = new { };

            switch (provider)
            {
                case "ChatGPT":
                    string? chatApiKey = await SecureStorage.Default.GetAsync("api_ChatGPT");
                    if (string.IsNullOrEmpty(chatApiKey))
                        throw new Exception("Chybí API klíč pro ChatGPT!");

                    url = "https://api.openai.com/v1/chat/completions";
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {chatApiKey}");
                    payload = new
                    {
                        model = "gpt-4o-mini",
                        messages = new[]
                        {
                            new { role = "user", content = "Jedno české slovo pro hru šibenice. Jen to slovo." }
                        }
                    };
                    break;

                case "Claude":
                    string? claudeApiKey = await SecureStorage.Default.GetAsync("api_Claude");
                    if (string.IsNullOrEmpty(claudeApiKey))
                        throw new Exception("Chybí API klíč pro Claude!");

                    url = "https://api.anthropic.com/v1/messages";
                    client.DefaultRequestHeaders.Add("x-api-key", claudeApiKey);
                    client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
                    payload = new
                    {
                        model = "claude-3-haiku-20240307",
                        max_tokens = 10,
                        messages = new[]
                        {
                            new { role = "user", content = "Jedno české slovo pro hru šibenice. Jen to slovo." }
                        }
                    };
                    break;
            }

            if (!string.IsNullOrEmpty(url))
            {
                var response = await client.PostAsJsonAsync(url, payload);
                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    throw new Exception($"AI Error {response.StatusCode}: {errorBody}");
                }

                word = await ExtractWordFromResponse(response, provider);
            }
        }

        // načtení dodatečného nastavení
        int requiredConsonants = Preferences.Default.Get("RequiredConsonants", 3);

        // vytvoření Hangmana
        return new Hangman(
            word.Trim().ToLower(),
            lives,
            vowelsEnabled,
            vowelsCount,
            requiredConsonants
        );
    }

    private async Task<string> ExtractWordFromResponse(HttpResponseMessage response, string provider)
    {
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        try
        {
            // Defensive JSON parsing to avoid null dereferences
            if (provider == "ChatGPT")
            {
                if (json.ValueKind == JsonValueKind.Object
                    && json.TryGetProperty("choices", out var choices)
                    && choices.ValueKind == JsonValueKind.Array
                    && choices.GetArrayLength() > 0)
                {
                    var first = choices[0];
                    if (first.ValueKind == JsonValueKind.Object
                        && first.TryGetProperty("message", out var message)
                        && message.ValueKind == JsonValueKind.Object
                        && message.TryGetProperty("content", out var content))
                    {
                        if (content.ValueKind == JsonValueKind.String)
                            return content.GetString() ?? "hangman";
                        // sometimes content can be an object containing 'text'
                        if (content.ValueKind == JsonValueKind.Object && content.TryGetProperty("text", out var textProp))
                            return textProp.GetString() ?? "hangman";
                    }
                }
            }
            else if (provider == "Claude")
            {
                if (json.ValueKind == JsonValueKind.Object
                    && json.TryGetProperty("content", out var contents)
                    && contents.ValueKind == JsonValueKind.Array
                    && contents.GetArrayLength() > 0)
                {
                    var first = contents[0];
                    if (first.ValueKind == JsonValueKind.Object && first.TryGetProperty("text", out var text))
                        return text.GetString() ?? "hangman";
                }
            }

            return "hangman";
        }
        catch
        {
            return "hangman";
        }
    }
}
