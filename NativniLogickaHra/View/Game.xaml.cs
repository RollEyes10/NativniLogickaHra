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
        lblStatus.Text = L.Get("Game_Loading");

        string provider = Preferences.Default.Get("SelectedAIProvider", "Gemini");
        string? apiKey = await SecureStorage.Default.GetAsync(provider);
        string word = "programovani";

        lblStatus.Text = L.Get("Game_LoadingFrom", provider);

        if (string.IsNullOrEmpty(apiKey))
        {
            lblStatus.Text = L.Get("Game_NoApiKey", provider);
        }
        else
        {
            try
            {
                var aiWord = await AiWordService.GetWordAsync(provider, apiKey);
                if (IsValidWord(aiWord))
                    word = aiWord!;
                else
                    lblStatus.Text = L.Get("Game_InvalidWord");
            }
            catch (Exception ex)
            {
                lblStatus.Text = L.Get("Game_AIUnavailable", ex.Message);
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

    // Enter nebo tlačítko — obojí volá DoGuess
    private void OnGuessCompleted(object sender, EventArgs e) => DoGuess();

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
        lblInfo.Text = L.Get("Game_Info", hra.ConsonantsGuessed, hra.RequiredConsonants, hra.RemainingAttempts);
        lblWrongLetters.Text = L.Get("Game_WrongLetters") + string.Join(" ", wrongLetters);
    }

    private async void CheckEndGame()
    {
        if (hra is null) return;

        if (!lblSecretWord.Text.Contains('_'))
        {
            await DisplayAlertAsync(L.Get("Game_Win_Title"), L.Get("Game_Win_Message"), L.Get("Game_Win_Button"));
            await StartNewGameAsync();
        }
        else if (hra.RemainingAttempts <= 0)
        {
            await DisplayAlertAsync(L.Get("Game_Lose_Title"), L.Get("Game_Lose_Message", hra.TargetWord), L.Get("Game_Lose_Button"));
            await StartNewGameAsync();
        }
    }
}