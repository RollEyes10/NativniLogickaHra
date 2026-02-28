using NativniLogickaHra.Models;
using NativniLogickaHra.Utils;

namespace NativniLogickaHra.View;

public partial class SettingsPage : ContentPage
{
    private bool isInitializing = false;
    private int _originalLanguage;

    public SettingsPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        isInitializing = true;

        _originalLanguage = Preferences.Default.Get("Language", 0);
        LanguagePicker.SelectedIndex = _originalLanguage;
        VolumeSlider.Value = Preferences.Default.Get("Volume", 50);
        DifficultyPicker.SelectedIndex = Preferences.Default.Get("Difficulty", 1);
        TopicPicker.SelectedIndex = TopicHelper.SavedTopic;
        LivesEnabledSwitch.IsToggled = Preferences.Default.Get("LivesEnabled", true);
        LivesSlider.IsEnabled = LivesEnabledSwitch.IsToggled;
        LivesSlider.Value = Preferences.Default.Get("LivesCount", 5);
        RequiredConsonantsSlider.Value = Preferences.Default.Get("RequiredConsonants", 3);

        UpdateLabels();
        UpdateAiStats();

        isInitializing = false;
    }

    private void UpdateLabels()
    {
        VolumeLabel.Text = L.Get("Settings_Volume", (int)Math.Round(VolumeSlider.Value));
        LivesLabel.Text = LivesEnabledSwitch.IsToggled
                                         ? L.Get("Settings_Lives", (int)Math.Round(LivesSlider.Value))
                                         : L.Get("Settings_LivesInfinite");
        RequiredConsonantsLabel.Text = L.Get("Settings_Consonants", (int)Math.Round(RequiredConsonantsSlider.Value));
    }

    private void UpdateAiStats()
    {
        (string text, Color color) = PlayerStats.Difficulty switch
        {
            1 => (L.Get("Settings_AI_Easy"), Color.FromArgb("#27AE60")),
            2 => (L.Get("Settings_AI_Medium"), Color.FromArgb("#F39C12")),
            _ => (L.Get("Settings_AI_Hard"), Color.FromArgb("#E74C3C"))
        };
        AiDifficultyLabel.Text = text;
        AiDifficultyLabel.TextColor = color;
        StatsWinsLabel.Text = PlayerStats.Wins.ToString();
        StatsLossesLabel.Text = PlayerStats.Losses.ToString();
        StatsStreakLabel.Text = PlayerStats.Streak.ToString();
        StatsWinRateLabel.Text = PlayerStats.TotalGames == 0 ? "–" : $"{PlayerStats.WinRate:P0}";
    }

    private void OnVolumeChanged(object sender, ValueChangedEventArgs e)
        => VolumeLabel.Text = L.Get("Settings_Volume", (int)Math.Round(e.NewValue));

    private void OnLivesChanged(object sender, ValueChangedEventArgs e)
        => LivesLabel.Text = L.Get("Settings_Lives", (int)Math.Round(e.NewValue));

    private void OnLivesToggled(object sender, ToggledEventArgs e)
    {
        LivesSlider.IsEnabled = e.Value;
        LivesLabel.Text = e.Value
            ? L.Get("Settings_Lives", (int)Math.Round(LivesSlider.Value))
            : L.Get("Settings_LivesInfinite");
    }

    public void OnRequiredConsonantsChanged(object sender, ValueChangedEventArgs e)
        => RequiredConsonantsLabel.Text = L.Get("Settings_Consonants", (int)Math.Round(e.NewValue));

    private async void OnResetStatsClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlertAsync(
            L.Get("Settings_ResetConfirm_Title"),
            L.Get("Settings_ResetConfirm_Message"),
            L.Get("Settings_ResetConfirm_Yes"),
            L.Get("Settings_ResetConfirm_No"));

        if (!confirm) return;
        PlayerStats.Reset();
        UpdateAiStats();
        await DisplayAlertAsync(
            L.Get("Settings_ResetDone_Title"),
            L.Get("Settings_ResetDone_Message"),
            L.Get("Settings_OK"));
    }

    private void OnDifficultyChanged(object sender, EventArgs e)
    {
        if (DifficultyPicker == null || isInitializing) return;
        switch ((Difficulty)DifficultyPicker.SelectedIndex)
        {
            case Difficulty.Easy: LivesEnabledSwitch.IsToggled = true; LivesSlider.Value = 8; break;
            case Difficulty.Normal: LivesEnabledSwitch.IsToggled = true; LivesSlider.Value = 5; break;
            case Difficulty.Hard: LivesEnabledSwitch.IsToggled = true; LivesSlider.Value = 3; break;
        }
        UpdateLabels();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        int newLanguage = LanguagePicker.SelectedIndex;

        Preferences.Default.Set("Language", newLanguage);
        Preferences.Default.Set("Volume", (int)VolumeSlider.Value);
        Preferences.Default.Set("Difficulty", DifficultyPicker.SelectedIndex);
        Preferences.Default.Set("LivesEnabled", LivesEnabledSwitch.IsToggled);
        Preferences.Default.Set("LivesCount", LivesEnabledSwitch.IsToggled ? (int)LivesSlider.Value : 0);
        Preferences.Default.Set("RequiredConsonants", (int)RequiredConsonantsSlider.Value);
        TopicHelper.SavedTopic = TopicPicker.SelectedIndex;

        LocalizationHelper.SetLanguage(newLanguage);

        if (newLanguage != _originalLanguage)
            Application.Current!.Windows[0].Page = new NavigationPage(new MainPage());
        else
            await Navigation.PopAsync();
    }
}