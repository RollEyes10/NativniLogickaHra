namespace NativniLogickaHra.Utils;

/// <summary>
/// Uchovává posledních 20 slov vrácených AI.
/// Thread-safe pomocí lock.
/// </summary>
public static class WordHistory
{
    private const int MaxHistory = 20;
    private static readonly LinkedList<string> History = new();
    private static readonly object Lock = new();

    /// <summary>
    /// Přidá slovo do historie. Pokud je seznam plný, odstraní nejstarší.
    /// </summary>
    public static void Add(string word)
    {
        if (string.IsNullOrWhiteSpace(word)) return;

        lock (Lock)
        {
            // Zabrání duplicitám po sobě jdoucím
            if (History.Last?.Value == word) return;

            History.AddLast(word);

            if (History.Count > MaxHistory)
                History.RemoveFirst();

            Logger.Log($"WordHistory: added '{word}', count={History.Count}");
        }
    }

    /// <summary>
    /// Vrátí kopii historie (nejstarší → nejnovější).
    /// </summary>
    public static IReadOnlyList<string> GetAll()
    {
        lock (Lock)
        {
            return History.ToList().AsReadOnly();
        }
    }

    /// <summary>
    /// Vrátí true pokud bylo slovo použito v posledních N slovech.
    /// </summary>
    public static bool WasRecentlyUsed(string word, int lookback = MaxHistory)
    {
        if (string.IsNullOrWhiteSpace(word)) return false;

        lock (Lock)
        {
            return History
                .TakeLast(lookback)
                .Any(w => string.Equals(w, word, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// Vymaže celou historii (např. při novém kole hry).
    /// </summary>
    public static void Clear()
    {
        lock (Lock)
        {
            History.Clear();
            Logger.Log("WordHistory: cleared");
        }
    }

    /// <summary>
    /// Počet aktuálně uložených slov.
    /// </summary>
    public static int Count
    {
        get { lock (Lock) { return History.Count; } }
    }
}