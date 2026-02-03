using NativniLogickaHra.View;

namespace NativniLogickaHra
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Musí zde být NavigationPage, aby aplikace věděla, co zobrazit
            return new Window(new NavigationPage(new MainPage()));
        }
    }
}