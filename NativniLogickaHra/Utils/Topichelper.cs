namespace NativniLogickaHra.Utils;

/// <summary>
/// Témata slov — mapuje index z Pickeru na prompt pro AI.
/// </summary>
public static class TopicHelper
{
    // Témata česky pro AI prompt (vždy anglicky pro AI, výsledek závisí na jazyku hry)
    private static readonly string[] TopicPromptsCzech =
    {
        "",                  // 0 = náhodné
        "zvířata",          // 1
        "jídlo a pití",     // 2
        "sport",            // 3
        "příroda a rostliny", // 4
        "technologie a počítače", // 5
        "země, města a hlavní města", // 6
        "povolání a profese", // 7
        "části lidského těla", // 8
    };

    private static readonly string[] TopicPromptsEnglish =
    {
        "",
        "animals",
        "food and drinks",
        "sport",
        "nature and plants",
        "technology and computers",
        "countries, cities and capitals",
        "jobs and professions",
        "parts of the human body",
    };

    public static string GetTopicPrompt(int topicIndex, int languageIndex)
    {
        var topics = languageIndex == 1 ? TopicPromptsEnglish : TopicPromptsCzech;
        if (topicIndex < 0 || topicIndex >= topics.Length) return "";
        return topics[topicIndex];
    }

    public static int SavedTopic
    {
        get => Preferences.Default.Get("WordTopic", 0);
        set => Preferences.Default.Set("WordTopic", value);
    }
}