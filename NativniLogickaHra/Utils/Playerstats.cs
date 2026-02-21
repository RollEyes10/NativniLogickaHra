namespace NativniLogickaHra.Utils;

/// <summary>
/// Sleduje statistiky hráče a počítá aktuální obtížnost (1–3).
/// Data jsou persistována přes Preferences.
/// </summary>
public static class PlayerStats
{
    private const string KeyWins = "stats_wins";
    private const string KeyLosses = "stats_losses";
    private const string KeyStreak = "stats_streak";     // aktuální série výher
    private const string KeyDifficulty = "stats_difficulty";

    // ── Čtení ────────────────────────────────────────────────────────────────

    public static int Wins => Preferences.Default.Get(KeyWins, 0);
    public static int Losses => Preferences.Default.Get(KeyLosses, 0);
    public static int Streak => Preferences.Default.Get(KeyStreak, 0);
    public static int Difficulty => Preferences.Default.Get(KeyDifficulty, 1); // 1 = Easy, 2 = Medium, 3 = Hard

    public static int TotalGames => Wins + Losses;
    public static double WinRate => TotalGames == 0 ? 0.0 : (double)Wins / TotalGames;

    // ── Aktualizace po hře ───────────────────────────────────────────────────

    /// <summary>Zavolej po výhře hráče.</summary>
    public static void RecordWin()
    {
        Preferences.Default.Set(KeyWins, Wins + 1);
        Preferences.Default.Set(KeyStreak, Streak + 1);
        RecalculateDifficulty();
        Logger.Log($"PlayerStats: WIN — streak={Streak}, difficulty={Difficulty}, winRate={WinRate:P0}");
    }

    /// <summary>Zavolej po prohře hráče.</summary>
    public static void RecordLoss()
    {
        Preferences.Default.Set(KeyLosses, Losses + 1);
        Preferences.Default.Set(KeyStreak, 0);          // série se přeruší
        RecalculateDifficulty();
        Logger.Log($"PlayerStats: LOSS — streak=0, difficulty={Difficulty}, winRate={WinRate:P0}");
    }

    /// <summary>Resetuje veškeré statistiky (např. nový profil).</summary>
    public static void Reset()
    {
        Preferences.Default.Set(KeyWins, 0);
        Preferences.Default.Set(KeyLosses, 0);
        Preferences.Default.Set(KeyStreak, 0);
        Preferences.Default.Set(KeyDifficulty, 1);
        Logger.Log("PlayerStats: reset");
    }

    // ── Adaptivní logika ─────────────────────────────────────────────────────

    /// <summary>
    /// Pravidla pro změnu obtížnosti:
    ///
    ///  → HARD (3):   série ≥ 5  NEBO  winRate ≥ 75 % (min. 10 her)
    ///  → MEDIUM (2): série ≥ 3  NEBO  winRate ≥ 55 % (min. 6 her)
    ///  → EASY (1):   jinak
    ///
    /// Obtížnost nikdy neskočí o více než 1 stupeň najednou.
    /// </summary>
    private static void RecalculateDifficulty()
    {
        int current = Difficulty;
        int target;

        if ((Streak >= 5) || (TotalGames >= 10 && WinRate >= 0.75))
            target = 3;
        else if ((Streak >= 3) || (TotalGames >= 6 && WinRate >= 0.55))
            target = 2;
        else
            target = 1;

        // Jemný přechod — max 1 stupeň najednou
        int next = target > current ? current + 1
                 : target < current ? current - 1
                 : current;

        Preferences.Default.Set(KeyDifficulty, next);
    }
}