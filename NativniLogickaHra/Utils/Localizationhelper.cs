using System.ComponentModel;

namespace NativniLogickaHra.Utils;

public sealed class LocalizationHelper : INotifyPropertyChanged
{
    public static readonly LocalizationHelper Instance = new();
    private Dictionary<string, string> _strings = new();
    private LocalizationHelper() { }

    public string this[string key] =>
        _strings.TryGetValue(key, out var val) ? val : key;

    public static string Get(string key) => Instance[key];
    public static string Get(string key, params object[] args)
    {
        try { return string.Format(Instance[key], args); }
        catch { return Instance[key]; }
    }

    public static void SetLanguage(int index)
    {
        Preferences.Default.Set("Language", index);
        Instance._strings = index == 1 ? English() : Czech();
        Instance.PropertyChanged?.Invoke(Instance, new PropertyChangedEventArgs(null));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private static Dictionary<string, string> Czech() => new()
    {
        // MainPage
        ["MainPage_Title"] = "Vítejte ve Šibenici",
        ["MainPage_StartGame"] = "Spustit hru",
        ["MainPage_Settings"] = "Nastavení",
        ["MainPage_ConnectionAI"] = "Připojení k AI",

        // Game
        ["Game_Title"] = "Šibenice",
        ["Game_Loading"] = "Načítám slovo…",
        ["Game_LoadingFrom"] = "Načítám slovo z: {0}…",
        ["Game_NoApiKey"] = "Chybí API klíč pro {0}, použito záložní slovo.",
        ["Game_InvalidWord"] = "AI vrátila neplatné slovo, použito záložní.",
        ["Game_AIUnavailable"] = "AI nedostupná: {0}. Použito záložní slovo.",
        ["Game_GuessPlaceholder"] = "Zadej písmeno",
        ["Game_GuessButton"] = "Hádej",
        ["Game_HintButton"] = "💡 Nápověda",
        ["Game_HintTitle"] = "Nápověda",
        ["Game_HintLoading"] = "AI připravuje nápovědu…",
        ["Game_HintNoKey"] = "Pro nápovědu je potřeba API klíč.",
        ["Game_HintError"] = "Nápovědu se nepodařilo načíst.",
        ["Game_HintOk"] = "OK",
        ["Game_WrongLetters"] = "Špatně uhodnutá písmena: ",
        ["Game_Info"] = "Souhlásky: {0}/{1} | Životy: {2}",
        ["Game_Win_Title"] = "Výhra!",
        ["Game_Win_Message"] = "Uhodl jsi slovo 🎉",
        ["Game_Win_Button"] = "OK",
        ["Game_Lose_Title"] = "Prohra",
        ["Game_Lose_Message"] = "Slovo bylo: {0}",
        ["Game_Lose_Button"] = "Znovu",

        // Settings
        ["Settings_Title"] = "Nastavení",
        ["Settings_Heading"] = "Nastavení hry",
        ["Settings_Language"] = "Jazyk aplikace:",
        ["Settings_Volume"] = "Hlasitost: {0} %",
        ["Settings_Difficulty"] = "Obtížnost:",
        ["Settings_Topic"] = "Téma slov:",
        ["Settings_Topic_Random"] = "🎲 Vše (náhodné)",
        ["Settings_Topic_Animals"] = "🐾 Zvířata",
        ["Settings_Topic_Food"] = "🍕 Jídlo",
        ["Settings_Topic_Sport"] = "⚽ Sport",
        ["Settings_Topic_Nature"] = "🌿 Příroda",
        ["Settings_Topic_Tech"] = "💻 Technologie",
        ["Settings_Topic_Countries"] = "🌍 Země a města",
        ["Settings_Topic_Jobs"] = "👷 Povolání",
        ["Settings_Topic_Body"] = "🫀 Lidské tělo",
        ["Settings_AI_Title"] = "Adaptivní obtížnost AI",
        ["Settings_AI_Level"] = "Aktuální úroveň:",
        ["Settings_AI_Easy"] = "Easy 🟢",
        ["Settings_AI_Medium"] = "Medium 🟡",
        ["Settings_AI_Hard"] = "Hard 🔴",
        ["Settings_Wins"] = "Výhry",
        ["Settings_Losses"] = "Prohry",
        ["Settings_Streak"] = "Série",
        ["Settings_WinRate"] = "Úspěšnost",
        ["Settings_ResetButton"] = "Resetovat statistiky",
        ["Settings_ResetConfirm_Title"] = "Resetovat statistiky",
        ["Settings_ResetConfirm_Message"] = "Opravdu chceš smazat všechny statistiky a vrátit obtížnost na Easy?",
        ["Settings_ResetConfirm_Yes"] = "Ano, resetovat",
        ["Settings_ResetConfirm_No"] = "Zrušit",
        ["Settings_ResetDone_Title"] = "Hotovo",
        ["Settings_ResetDone_Message"] = "Statistiky byly resetovány.",
        ["Settings_LivesEnabled"] = "Omezené životy:",
        ["Settings_Lives"] = "Počet životů: {0}",
        ["Settings_LivesInfinite"] = "Počet životů: ∞",
        ["Settings_Consonants"] = "Souhlásky pro povolení samohlásek: {0}",
        ["Settings_Save"] = "Uložit a zpět",
        ["Settings_OK"] = "OK",

        // ConnectionAI
        ["Connection_Title"] = "Správa API klíčů",
        ["Connection_InstructionBtn"] = "📘 Návod – jak přidat API klíč",
        ["Connection_SavedConnections"] = "Uložená připojení",
        ["Connection_NoKey"] = "❌ Bez klíče",
        ["Connection_KeySaved"] = "✅ Uložen",
        ["Connection_KeySavedMode"] = "✅ Uložen ({0})",
        ["Connection_Detail"] = "Detail připojení",
        ["Connection_Provider"] = "Poskytovatel",
        ["Connection_ApiKey"] = "API klíč",
        ["Connection_ApiKeyPlaceholder"] = "Vložte API klíč",
        ["Connection_GeminiMode"] = "Režim:",
        ["Connection_GeminiPaid"] = "PAID (bez omezení)",
        ["Connection_GeminiInfo"] = "FREE: 5 dotazů / min | PAID: bez omezení",
        ["Connection_Save"] = "Uložit / Aktualizovat",
        ["Connection_Delete"] = "Smazat připojení",
        ["Connection_Saved"] = "Klíč pro {0} uložen.",
        ["Connection_Editing"] = "Editace připojení: {0}",
        ["Connection_Deleted"] = "Připojení {0} smazáno.",
        ["Connection_Error_NoKey"] = "Chyba",
        ["Connection_Error_NoKeyMsg"] = "Zadejte API klíč.",
        ["Connection_DeleteConfirm_Title"] = "Smazat",
        ["Connection_DeleteConfirm_Message"] = "Opravdu smazat připojení pro {0}?",
        ["Connection_DeleteConfirm_Yes"] = "Ano",
        ["Connection_DeleteConfirm_No"] = "Ne",

        // Instruction
        ["Instruction_Title"] = "Návod – API klíče",
        ["Instruction_Back"] = "⬅ Zpět",
        ["Instruction_Heading"] = "Jak přidat API klíč",
        ["Instruction_Subtitle"] = "Tato aplikace podporuje Gemini, ChatGPT a Claude.",
        ["Instruction_Step1"] = "1. Otevři:",
        ["Instruction_Gemini_Step2"] = "2. Vytvoř API klíč",
        ["Instruction_Gemini_Step3"] = "3. Vyber Gemini v aplikaci a klíč vlož",
        ["Instruction_ChatGpt_Step"] = "Vytvoř klíč a vlož ho do aplikace.",
        ["Instruction_Claude_Step"] = "Vytvoř API klíč a ulož ho do aplikace.",
        ["Instruction_Security_Title"] = "⚠️ Bezpečnost",
        ["Instruction_Security_1"] = "• API klíč nikdy nesdílej",
        ["Instruction_Security_2"] = "• Pokud unikne, okamžitě ho zneplatni",
    };

    private static Dictionary<string, string> English() => new()
    {
        // MainPage
        ["MainPage_Title"] = "Welcome to Hangman",
        ["MainPage_StartGame"] = "Start Game",
        ["MainPage_Settings"] = "Settings",
        ["MainPage_ConnectionAI"] = "AI Connection",

        // Game
        ["Game_Title"] = "Hangman",
        ["Game_Loading"] = "Loading word…",
        ["Game_LoadingFrom"] = "Loading word from: {0}…",
        ["Game_NoApiKey"] = "Missing API key for {0}, using fallback word.",
        ["Game_InvalidWord"] = "AI returned an invalid word, using fallback.",
        ["Game_AIUnavailable"] = "AI unavailable: {0}. Using fallback word.",
        ["Game_GuessPlaceholder"] = "Enter a letter",
        ["Game_GuessButton"] = "Guess",
        ["Game_HintButton"] = "💡 Hint",
        ["Game_HintTitle"] = "Hint",
        ["Game_HintLoading"] = "AI is preparing a hint…",
        ["Game_HintNoKey"] = "An API key is required for hints.",
        ["Game_HintError"] = "Could not load the hint.",
        ["Game_HintOk"] = "OK",
        ["Game_WrongLetters"] = "Wrong letters: ",
        ["Game_Info"] = "Consonants: {0}/{1} | Lives: {2}",
        ["Game_Win_Title"] = "You won!",
        ["Game_Win_Message"] = "You guessed the word 🎉",
        ["Game_Win_Button"] = "OK",
        ["Game_Lose_Title"] = "Game over",
        ["Game_Lose_Message"] = "The word was: {0}",
        ["Game_Lose_Button"] = "Try again",

        // Settings
        ["Settings_Title"] = "Settings",
        ["Settings_Heading"] = "Game Settings",
        ["Settings_Language"] = "App language:",
        ["Settings_Volume"] = "Volume: {0} %",
        ["Settings_Difficulty"] = "Difficulty:",
        ["Settings_Topic"] = "Word topic:",
        ["Settings_Topic_Random"] = "🎲 Everything (random)",
        ["Settings_Topic_Animals"] = "🐾 Animals",
        ["Settings_Topic_Food"] = "🍕 Food",
        ["Settings_Topic_Sport"] = "⚽ Sport",
        ["Settings_Topic_Nature"] = "🌿 Nature",
        ["Settings_Topic_Tech"] = "💻 Technology",
        ["Settings_Topic_Countries"] = "🌍 Countries & cities",
        ["Settings_Topic_Jobs"] = "👷 Professions",
        ["Settings_Topic_Body"] = "🫀 Human body",
        ["Settings_AI_Title"] = "AI Adaptive Difficulty",
        ["Settings_AI_Level"] = "Current level:",
        ["Settings_AI_Easy"] = "Easy 🟢",
        ["Settings_AI_Medium"] = "Medium 🟡",
        ["Settings_AI_Hard"] = "Hard 🔴",
        ["Settings_Wins"] = "Wins",
        ["Settings_Losses"] = "Losses",
        ["Settings_Streak"] = "Streak",
        ["Settings_WinRate"] = "Win rate",
        ["Settings_ResetButton"] = "Reset statistics",
        ["Settings_ResetConfirm_Title"] = "Reset statistics",
        ["Settings_ResetConfirm_Message"] = "Are you sure you want to delete all statistics and reset difficulty to Easy?",
        ["Settings_ResetConfirm_Yes"] = "Yes, reset",
        ["Settings_ResetConfirm_No"] = "Cancel",
        ["Settings_ResetDone_Title"] = "Done",
        ["Settings_ResetDone_Message"] = "Statistics have been reset.",
        ["Settings_LivesEnabled"] = "Limited lives:",
        ["Settings_Lives"] = "Lives: {0}",
        ["Settings_LivesInfinite"] = "Lives: ∞",
        ["Settings_Consonants"] = "Consonants required to unlock vowels: {0}",
        ["Settings_Save"] = "Save and go back",
        ["Settings_OK"] = "OK",

        // ConnectionAI
        ["Connection_Title"] = "Manage API Keys",
        ["Connection_InstructionBtn"] = "📘 Guide – how to add an API key",
        ["Connection_SavedConnections"] = "Saved connections",
        ["Connection_NoKey"] = "❌ No key",
        ["Connection_KeySaved"] = "✅ Saved",
        ["Connection_KeySavedMode"] = "✅ Saved ({0})",
        ["Connection_Detail"] = "Connection detail",
        ["Connection_Provider"] = "Provider",
        ["Connection_ApiKey"] = "API key",
        ["Connection_ApiKeyPlaceholder"] = "Enter API key",
        ["Connection_GeminiMode"] = "Mode:",
        ["Connection_GeminiPaid"] = "PAID (no limits)",
        ["Connection_GeminiInfo"] = "FREE: 5 requests / min | PAID: no limits",
        ["Connection_Save"] = "Save / Update",
        ["Connection_Delete"] = "Delete connection",
        ["Connection_Saved"] = "Key for {0} saved.",
        ["Connection_Editing"] = "Editing connection: {0}",
        ["Connection_Deleted"] = "Connection {0} deleted.",
        ["Connection_Error_NoKey"] = "Error",
        ["Connection_Error_NoKeyMsg"] = "Please enter an API key.",
        ["Connection_DeleteConfirm_Title"] = "Delete",
        ["Connection_DeleteConfirm_Message"] = "Really delete connection for {0}?",
        ["Connection_DeleteConfirm_Yes"] = "Yes",
        ["Connection_DeleteConfirm_No"] = "No",

        // Instruction
        ["Instruction_Title"] = "Guide – API keys",
        ["Instruction_Back"] = "⬅ Back",
        ["Instruction_Heading"] = "How to add an API key",
        ["Instruction_Subtitle"] = "This app supports Gemini, ChatGPT and Claude.",
        ["Instruction_Step1"] = "1. Open:",
        ["Instruction_Gemini_Step2"] = "2. Create an API key",
        ["Instruction_Gemini_Step3"] = "3. Select Gemini in the app and paste the key",
        ["Instruction_ChatGpt_Step"] = "Create a key and paste it into the app.",
        ["Instruction_Claude_Step"] = "Create an API key and save it in the app.",
        ["Instruction_Security_Title"] = "⚠️ Security",
        ["Instruction_Security_1"] = "• Never share your API key",
        ["Instruction_Security_2"] = "• If it leaks, revoke it immediately",
    };
}

public static class L
{
    public static string Get(string key) => LocalizationHelper.Get(key);
    public static string Get(string key, params object[] args) => LocalizationHelper.Get(key, args);
}