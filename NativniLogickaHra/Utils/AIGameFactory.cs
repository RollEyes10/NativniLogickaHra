using NativniLogickaHra;
using NativniLogickaHra.Services;
using NativniLogickaHra.Utils;

public class AIGameFactory
{

    public async Task<Hangman> CreateHangmanGameAsync(string provider)
    {
        // ── Nastavení hry ────────────────────────────────────────────────────
        int lives = Preferences.Default.Get("LivesEnabled", true)
            ? Preferences.Default.Get("LivesCount", 6)
            : int.MaxValue;

        int vowelsCount = Preferences.Default.Get("VowelsEnabled", true)
            ? Preferences.Default.Get("VowelsCount", 3)
            : 0;

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

        // ── Výběr slova podle obtížnosti hráče ───────────────────────────────
        string word = await WordSelector.GetWordForPlayerAsync(provider, apiKey);

        Logger.Log($"AIGameFactory: word='{word}', difficulty={PlayerStats.Difficulty}");

        // ── Vytvoření hry ────────────────────────────────────────────────────
        return new Hangman(
            word.Trim().ToLower(),
            lives,
            vowelsEnabled: vowelsCount > 0,
            vowelsCount,
            requiredConsonants
        );
    }

    /// <summary>
    /// Zavolej po každé dokončené hře, aby se aktualizovala obtížnost.
    /// </summary>
    public static void RecordResult(bool playerWon)
    {
        if (playerWon)
            PlayerStats.RecordWin();
        else
            PlayerStats.RecordLoss();

        Logger.Log($"AIGameFactory: result recorded — won={playerWon}, " +
                   $"newDifficulty={PlayerStats.Difficulty}, streak={PlayerStats.Streak}");
    }
}