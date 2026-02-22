using Microsoft.Extensions.Logging;
using NativniLogickaHra.Utils;

namespace NativniLogickaHra
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();

            // ── Inicializuj lokalizaci hned po startu ──
            int language = Preferences.Default.Get("Language", 0);
            LocalizationHelper.SetLanguage(language);

            return app;
        }
    }
}