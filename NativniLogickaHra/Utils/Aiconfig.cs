namespace NativniLogickaHra.Services;

/// <summary>
/// Centrální konfigurace AI modelů.
/// Změna modelu = změna jednoho řádku zde, nic jiného v projektu měnit nemusíš.
/// </summary>
public static class AiConfig
{
    // ── Aktivní modely ───────────────────────────────────────────────────────
    // Změň hodnotu na jakýkoliv jiný model ze seznamu níže.

    public static string GeminiModel { get; } = GeminiModels.Flash;
    public static string ChatGptModel { get; } = ChatGptModels.Gpt41Mini;
    public static string ClaudeModel { get; } = ClaudeModels.Haiku3;

    // ── Dostupné modely ──────────────────────────────────────────────────────

    public static class GeminiModels
    {
        public const string Flash = "gemini-2.0-flash";
        public const string FlashLite = "gemini-2.0-flash-lite";
        public const string Pro = "gemini-2.5-pro-preview-03-25";
    }

    public static class ChatGptModels
    {
        public const string Gpt41Mini = "gpt-4.1-mini";   // ← doporučeno pro demo
        public const string Gpt41Nano = "gpt-4.1-nano";
        public const string Gpt41 = "gpt-4.1";
        public const string Gpt4o = "gpt-4o";
        public const string Gpt4oMini = "gpt-4o-mini";
    }

    public static class ClaudeModels
    {
        public const string Haiku3 = "claude-3-haiku-20240307";
        public const string Haiku35 = "claude-3-5-haiku-20241022";
        public const string Sonnet35 = "claude-3-5-sonnet-20241022";
        public const string Sonnet4 = "claude-sonnet-4-20250514";
    }
}