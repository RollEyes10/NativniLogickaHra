using System.ComponentModel;
using System.Text.Json;

namespace NativniLogickaHra.Utils;

/// <summary>
/// Dynamická lokalizace přes JSON soubory (Resources/Raw/cs.json, en.json).
/// Inicializuj při startu: LocalizationHelper.SetLanguage(index)
/// XAML: Text="{Binding [Game_Loading], Source={x:Static utils:LocalizationHelper.Instance}}"
/// C#:   L.Get("Game_Loading")  nebo  L.Get("Game_Info", a, b, c)
/// </summary>
public sealed class LocalizationHelper : INotifyPropertyChanged
{
    public static readonly LocalizationHelper Instance = new();

    private Dictionary<string, string> _strings = new();

    // Singleton — konstruktor neprovádí žádné IO
    private LocalizationHelper() { }

    // ── XAML indexer ─────────────────────────────────────────────────────────
    public string this[string key] =>
        _strings.TryGetValue(key, out var val) ? val : key;

    // ── C# zkratky ───────────────────────────────────────────────────────────
    public static string Get(string key)
        => Instance[key];

    public static string Get(string key, params object[] args)
    {
        var template = Instance[key];
        try { return string.Format(template, args); }
        catch { return template; }
    }

    // ── Přepnutí / inicializace jazyka ───────────────────────────────────────
    public static void SetLanguage(int index)
    {
        Preferences.Default.Set("Language", index);
        Instance.LoadLanguage(index);
        Instance.PropertyChanged?.Invoke(Instance, new PropertyChangedEventArgs(null));
    }

    // ── Načtení JSON souboru ─────────────────────────────────────────────────
    private void LoadLanguage(int index)
    {
        string fileName = index == 1 ? "en.json" : "cs.json";
        try
        {
            using var stream = FileSystem.OpenAppPackageFileAsync(fileName).GetAwaiter().GetResult();
            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            _strings = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                       ?? new Dictionary<string, string>();
            System.Diagnostics.Debug.WriteLine($"[L10n] Loaded {_strings.Count} keys from {fileName}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[L10n] ERROR loading {fileName}: {ex.Message}");
            _strings = new Dictionary<string, string>();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}

// Kratší alias pro C# použití
public static class L
{
    public static string Get(string key) => LocalizationHelper.Get(key);
    public static string Get(string key, params object[] args) => LocalizationHelper.Get(key, args);
}