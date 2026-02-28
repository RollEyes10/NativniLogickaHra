namespace NativniLogickaHra.Utils;

/// <summary>
/// Uchovává posledních 20 slov vrácených AI.
/// Thread-safe pomocí lock. Historie přežije restart aplikace (uloženo v Preferences).
/// </summary>
public static class WordHistory
{
    private const int MaxHistory = 20;
    private const int MaxRetries = 5;  // zvýšeno pro lepší unikátnost
    private const string PrefsKey = "word_history";
    private const char Separator = '|';

    private static readonly object Lock = new();
    private static LinkedList<string>? _history;

    private static LinkedList<string> History
    {
        get
        {
            _history ??= LoadFromPrefs();
            return _history;
        }
    }

    /// <summary>
    /// Přidá slovo do historie. Pokud je seznam plný, odstraní nejstarší.
    /// Ignoruje duplicity — stejné slovo se neuloží dvakrát.
    /// </summary>
    public static void Add(string word)
    {
        if (string.IsNullOrWhiteSpace(word)) return;
        word = word.ToLower().Trim();

        lock (Lock)
        {
            // Ignoruj pokud už slovo v historii je
            if (History.Any(w => string.Equals(w, word, StringComparison.OrdinalIgnoreCase)))
            {
                Logger.Log($"WordHistory: '{word}' already in history, skipping");
                return;
            }

            History.AddLast(word);

            if (History.Count > MaxHistory)
                History.RemoveFirst();

            SaveToPrefs();
            Logger.Log($"WordHistory: added '{word}', count={History.Count}");
        }
    }

    /// <summary>
    /// Vrátí true pokud bylo slovo použito v posledních N slovech.
    /// </summary>
    public static bool WasRecentlyUsed(string? word, int lookback = MaxHistory)
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
    /// Vrátí kopii historie (nejstarší → nejnovější).
    /// </summary>
    public static IReadOnlyList<string> GetAll()
    {
        lock (Lock) { return History.ToList().AsReadOnly(); }
    }

    /// <summary>
    /// Vymaže celou historii.
    /// </summary>
    public static void Clear()
    {
        lock (Lock)
        {
            History.Clear();
            Preferences.Default.Remove(PrefsKey);
            Logger.Log("WordHistory: cleared");
        }
    }

    public static int Count
    {
        get { lock (Lock) { return History.Count; } }
    }

    private static LinkedList<string> LoadFromPrefs()
    {
        var raw = Preferences.Default.Get(PrefsKey, string.Empty);
        if (string.IsNullOrWhiteSpace(raw))
            return new LinkedList<string>();

        var words = raw.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
        Logger.Log($"WordHistory: loaded {words.Length} words from Preferences");
        return new LinkedList<string>(words);
    }

    private static void SaveToPrefs()
    {
        Preferences.Default.Set(PrefsKey, string.Join(Separator, History));
    }
}