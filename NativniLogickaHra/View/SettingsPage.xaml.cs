using NativniLogickaHra.Models;

namespace NativniLogickaHra.View;

public partial class SettingsPage : ContentPage
{
    private bool isInitializing = false;

    public SettingsPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        isInitializing = true;

        // ensure named controls from XAML are available even if generated fields are missing
        RequiredConsonantsSlider = this.FindByName<Slider>("RequiredConsonantsSlider");
        RequiredConsonantsLabel = this.FindByName<Label>("RequiredConsonantsLabel");

        LanguagePicker.SelectedIndex =
            Preferences.Default.Get("Language", 0);

        VolumeSlider.Value =
            Preferences.Default.Get("Volume", 50);

        DifficultyPicker.SelectedIndex =
            Preferences.Default.Get("Difficulty", 1);

        LivesEnabledSwitch.IsToggled =
            Preferences.Default.Get("LivesEnabled", true);

        LivesSlider.IsEnabled = LivesEnabledSwitch.IsToggled;
        LivesSlider.Value =
            Preferences.Default.Get("LivesCount", 5);

        RequiredConsonantsSlider.Value =
            Preferences.Default.Get("RequiredConsonants", 3);

        UpdateLabels();

        isInitializing = false;
    }

    private void UpdateLabels()
    {
        VolumeLabel.Text = $"Hlasitost: {Math.Round(VolumeSlider.Value)} %";

        if (LivesEnabledSwitch.IsToggled)
            LivesLabel.Text = $"Počet životů: {Math.Round(LivesSlider.Value)}";
        else
            LivesLabel.Text = "Počet životů: ∞";

        RequiredConsonantsLabel.Text = $"Souhlásky pro povolení samohlásek: {Math.Round(RequiredConsonantsSlider.Value)}";
    }

    private void OnVolumeChanged(object sender, ValueChangedEventArgs e)
    {
        VolumeLabel.Text = $"Hlasitost: {Math.Round(e.NewValue)} %";
    }

    private void OnLivesChanged(object sender, ValueChangedEventArgs e)
    {
        LivesLabel.Text = $"Počet životů: {Math.Round(e.NewValue)}";
    }

    private void OnLivesToggled(object sender, ToggledEventArgs e)
    {
        LivesSlider.IsEnabled = e.Value;

        if (!e.Value)
        {
            LivesSlider.Value = 0;
            LivesLabel.Text = "Počet životů: ∞";
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        Preferences.Default.Set("Language", LanguagePicker.SelectedIndex);
        Preferences.Default.Set("Volume", (int)VolumeSlider.Value);
        Preferences.Default.Set("Difficulty", DifficultyPicker.SelectedIndex);

        // don't re-apply preset on save — user selection should be kept as-is

        Preferences.Default.Set("LivesEnabled", LivesEnabledSwitch.IsToggled);
        Preferences.Default.Set(
            "LivesCount",
            LivesEnabledSwitch.IsToggled ? (int)LivesSlider.Value : 0
        );

        Preferences.Default.Set("RequiredConsonants", (int)RequiredConsonantsSlider.Value);

        await Navigation.PopAsync();
    }

    private void OnDifficultyChanged(object sender, EventArgs e)
    {
        // only apply presets when user actively changes DifficultyPicker
        if (DifficultyPicker == null) return;
        if (isInitializing) return;

        var diff = (Difficulty)DifficultyPicker.SelectedIndex;

        switch (diff)
        {
            case Difficulty.Easy:
                LivesEnabledSwitch.IsToggled = true;
                LivesSlider.Value = 8;
                break;

            case Difficulty.Normal:
                LivesEnabledSwitch.IsToggled = true;
                LivesSlider.Value = 5;
                break;

            case Difficulty.Hard:
                LivesEnabledSwitch.IsToggled = true;
                LivesSlider.Value = 3;
                break;

            case Difficulty.Custom:
                break;
        }

        UpdateLabels();
    }

    // Make the handler public so the XAML loader (and hot reload) can resolve it by name and signature
    public void OnRequiredConsonantsChanged(object sender, Microsoft.Maui.Controls.ValueChangedEventArgs e)
    {
        RequiredConsonantsLabel.Text = $"Souhlásky pro povolení samohlásek: {Math.Round(e.NewValue)}";
    }
}
