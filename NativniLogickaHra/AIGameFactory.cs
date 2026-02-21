using NativniLogickaHra;
using NativniLogickaHra.Services;
using NativniLogickaHra.Utils;

public class AIGameFactory
{
    private const string FallbackWord = "programovani";

    public async Task<Hangman> CreateHangmanGameAsync(string provider)
    {
        // ── Nastavení hry ────────────────────────────────────────────────────
        int lives = Preferences.Default.Get("LivesEnabled", true)
            ? Preferences.Default.Get("LivesCount", 6)
            : int.MaxValue;

        int vowelsCount = Preferences.Default.Get("VowelsEnabled", true)
            ? Preferences.Default.Get("VowelsCount", 3)
            : 0;

        bool vowelsEnabled = vowelsCount > 0;
        int requiredConsonants = Preferences.Default.Get("RequiredConsonants", 3);

        // ── API klíč ─────────────────────────────────────────────────────────
        string apiKeyName = provider switch
        {
            "Gemini" => "api_Gemini",
            "ChatGPT" => "api_ChatGPT",
            "Claude" => "api_Claude",
            _ => throw new Exception($"Neznámý provider: {provider}")
        };

        string? apiKey = await SecureStorage.Default.GetAsync(apiKeyName);
        if (string.IsNullOrEmpty(apiKey))
            throw new Exception($"Chybí API klíč pro {provider}!");

        // ── Získání slova přes AiWordService (obsahuje WordFilter + WordHistory) ──
        string word = FallbackWord;

        string? aiWord = await AiWordService.GetWordAsync(provider, apiKey);

        if (!string.IsNullOrWhiteSpace(aiWord))
        {
            // Volitelně: přeskoč slova, která byla nedávno použita
            if (WordHistory.WasRecentlyUsed(aiWord))
            {
                Logger.Log($"AIGameFactory: slovo '{aiWord}' bylo nedávno použito, zkouším znovu");

                // Jeden další pokus o jiné slovo
                string? retryWord = await AiWordService.GetWordAsync(provider, apiKey);
                if (!string.IsNullOrWhiteSpace(retryWord) && !WordHistory.WasRecentlyUsed(retryWord))
                    aiWord = retryWord;
            }

            if (!string.IsNullOrWhiteSpace(aiWord))
                word = aiWord;
        }
        else
        {
            Logger.Log($"AIGameFactory: AI nevrátila platné slovo, použit fallback '{FallbackWord}'");
        }

        // ── Vytvoření hry ────────────────────────────────────────────────────
        return new Hangman(
            word.Trim().ToLower(),
            lives,
            vowelsEnabled,
            vowelsCount,
            requiredConsonants
        );
    }
}