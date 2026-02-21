namespace NativniLogickaHra.View;

using NativniLogickaHra;
using NativniLogickaHra.Services;
using NativniLogickaHra.Utils;

public partial class Game : ContentPage
{
    private Hangman? hra;
    private HashSet<char> wrongLetters = new();

    public Game()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Logger.Log("Game OnAppearing");
        await StartNewGameAsync();
    }

    private async Task StartNewGameAsync()
    {
        wrongLetters.Clear();
        lblStatus.Text = "Načítám slovo…";
        Logger.Log("StartNewGameAsync - starting new game");

        // read provider selected in ConnectionAI (fallback to Gemini)
        string provider = Preferences.Default.Get("SelectedAIProvider", "Gemini");
        string? apiKey = await SecureStorage.Default.GetAsync(provider);
        Logger.Log($"SelectedAIProvider: {provider}, API key exists: {!string.IsNullOrEmpty(apiKey)}");
        string word = "programovani";

        lblStatus.Text = $"Načítám slovo z: {provider}…";

        if (string.IsNullOrEmpty(apiKey))
        {
            lblStatus.Text = $"Chybí API klíč pro {provider}, použito záložní slovo.";
            Logger.Log($"No API key for provider {provider}");
        }
        else
        {
            try
            {
                Logger.Log($"Calling AiWordService.GetWordAsync for {provider}");
                var aiWord = await AiWordService.GetWordAsync(provider, apiKey);
                Logger.Log($"AiWordService result for {provider}: {aiWord}");
                if (IsValidWord(aiWord))
                    word = aiWord!;
                else
                {
                    lblStatus.Text = "AI vrátila neplatné slovo, použito záložní.";
                    Logger.Log($"Invalid word from AI: {aiWord}");
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"AI nedostupná: {ex.Message}. Použito záložní slovo.";
                Logger.Log($"AI call failed for {provider}: {ex}");
            }
        }

        int lives = Preferences.Default.Get("LivesEnabled", true)
            ? Preferences.Default.Get("LivesCount", 6)
            : int.MaxValue;

        bool vowelsEnabled = Preferences.Default.Get("VowelsEnabled", true);
        int vowelsCount = Preferences.Default.Get("VowelsCount", 3);
        int requiredConsonants = Preferences.Default.Get("RequiredConsonants", 3);

        hra = new Hangman(word, lives, vowelsEnabled, vowelsCount, requiredConsonants);
        UpdateUI();
    }

    private static bool IsValidWord(string? word)
        => !string.IsNullOrWhiteSpace(word) && word.All(char.IsLetter);

    // 🔥 AUTOMATICKÉ HÁDÁNÍ PŘI PSANÍ
    private void OnGuessTextChanged(object sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.NewTextValue))
            return;

        DoGuess();
    }


    // 🔥 ENTER = POTVRZENÍ
    private void OnGuessCompleted(object sender, EventArgs e)
    {
        DoGuess();
    }

    private void DoGuess()
    {
        if (hra is null) return;
        if (string.IsNullOrWhiteSpace(entGuess.Text)) return;

        char guess = char.ToLower(entGuess.Text[0]);
        string result = hra.Guess(entGuess.Text);

        if (result == "Špatně!")
            wrongLetters.Add(guess);

        lblStatus.Text = result;
        entGuess.Text = string.Empty;

        UpdateUI();
        CheckEndGame();
    }

    private void UpdateUI()
    {
        if (hra is null) return;

        lblSecretWord.Text = string.Join(" ", hra.SecretWord);
        lblInfo.Text =
            $"Souhlásky: {hra.ConsonantsGuessed}/{hra.RequiredConsonants} | Životy: {hra.RemainingAttempts}";
        lblWrongLetters.Text = "Špatně uhodnutá písmena: " + string.Join(" ", wrongLetters);
    }

    private async void CheckEndGame()
    {
        if (hra is null) return;

        if (!lblSecretWord.Text.Contains('_'))
        {
            await DisplayAlert("Výhra!", "Uhodl jsi slovo 🎉", "OK");
            await StartNewGameAsync();
        }
        else if (hra.RemainingAttempts <= 0)
        {
            await DisplayAlert("Prohra",
                $"Slovo bylo: {hra.TargetWord}",
                "Znovu");
            await StartNewGameAsync();
        }
    }
}
