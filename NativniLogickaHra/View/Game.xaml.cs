namespace NativniLogickaHra.View;

using NativniLogickaHra;
using NativniLogickaHra.Services;

public partial class Game : ContentPage
{
    private Hangman? hra;
    private bool livesEnabled = true;

    public Game()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await StartNewGameAsync();
    }

    private async Task StartNewGameAsync()
    {
        if (lblStatus != null)
            lblStatus.Text = "Načítám slovo…";

        string provider = "Gemini";
        string storageKey = provider;
        string? apiKey = await SecureStorage.Default.GetAsync(storageKey);

        string word = "programovani";

        if (!string.IsNullOrEmpty(apiKey))
        {
            try
            {
                var aiWord = await AiWordService.GetWordAsync(provider, apiKey);
                if (IsValidWord(aiWord))
                    word = aiWord!;
                else
                    lblStatus?.Text = "AI vrátila neplatné slovo, použito záložní.";
            }
            catch
            {
                lblStatus?.Text = "AI nedostupná, použito záložní slovo.";
            }
        }
        else
        {
            lblStatus?.Text = "Chybí API klíč, použito výchozí slovo.";
        }

        // ⬇⬇⬇ NAČTENÍ NASTAVENÍ ⬇⬇⬇
        var livesEnabledPref = Preferences.Default.Get("LivesEnabled", true);
        // if lives are disabled in settings, treat as infinite lives
        int lives = livesEnabledPref
            ? Preferences.Default.Get("LivesCount", 6)
            : int.MaxValue;
        bool vowelsEnabled = Preferences.Default.Get("VowelsEnabled", true);
        int vowelsCount = Preferences.Default.Get("VowelsCount", 3);
        int requiredConsonants = Preferences.Default.Get("RequiredConsonants", 3);

        livesEnabled = livesEnabledPref;

        hra = new Hangman(
            word,
            lives,
            vowelsEnabled,
            vowelsCount,
            requiredConsonants
        );

        UpdateUI();
    }

    private static bool IsValidWord(string? word)
    {
        if (string.IsNullOrWhiteSpace(word))
            return false;

        return word.All(c => c >= 'a' && c <= 'z') && word.Length >= 4;
    }

    private void OnGuessClicked(object sender, EventArgs e)
    {
        if (hra is null) return;

        var guessText = entGuess?.Text;
        if (string.IsNullOrWhiteSpace(guessText)) return;

        lblStatus?.Text = hra.Guess(guessText);
        if (entGuess != null) entGuess.Text = string.Empty;
        UpdateUI();
        CheckEndGame();
    }

    private void UpdateUI()
    {
        if (hra is null) return;

        if (lblSecretWord != null)
            lblSecretWord.Text = string.Join(" ", hra.SecretWord);

        if (lblInfo != null)
            lblInfo.Text =
                $"Uhodnuté souhlásky: {hra.ConsonantsGuessed}/{hra.RequiredConsonants} | Životy: {hra.RemainingAttempts}";
    }

    private async void CheckEndGame()
    {
        if (hra is null) return;

        var secretText = lblSecretWord?.Text ?? string.Empty;

        if (!secretText.Contains('_'))
        {
            await DisplayAlertAsync("Výhra!", "Gratuluji, uhodl jsi slovo.", "OK");
            ResetGame();
        }
        else if (hra.RemainingAttempts <= 0)
        {
            await DisplayAlertAsync("Prohra",
                $"Došly ti životy. Slovo bylo: {hra.TargetWord}",
                "Zkusit znovu");

            ResetGame();
        }
    }

    private async void ResetGame()
    {
        await StartNewGameAsync();
    }
}
