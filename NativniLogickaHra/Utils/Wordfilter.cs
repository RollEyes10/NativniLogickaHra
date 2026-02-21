namespace NativniLogickaHra.Utils;

/// <summary>
/// Filtr slov nevhodných pro děti. Slova jsou uložena v lowercase.
/// Pokud AI vrátí nevhodné slovo, služba si vyžádá nové.
/// </summary>
public static class WordFilter
{
    // -----------------------------------------------------------------------
    // Zakázaná slova (obsah nevhodný pro děti)
    // Přidej/odeber dle potřeby. Vše lowercase, bez diakritiky i s diakritikou.
    // -----------------------------------------------------------------------
    private static readonly HashSet<string> Blacklist = new(StringComparer.OrdinalIgnoreCase)
    {
        // ── ČESKY ───────────────────────────────────────────────────────────

        // násilí
        "zbraň", "zbrane", "pistole", "nůž", "kuše", "granát", "bomba",
        "vražda", "zabití", "krev", "sekera", "meč",

        // alkohol / drogy
        "alkohol", "pivo", "víno", "vodka", "rum", "whisky", "whiskey",
        "droga", "marihuána", "kokain", "heroin", "pervitin",

        // vulgarity / sexuální obsah
        "penis", "vagína", "sex", "porn", "porno", "erotika",
        "kurva", "hovno", "pička", "kokot", "píča", "sráč",

        // hazard
        "kasino", "sázka", "ruleta", "poker",

        // jiné nevhodné
        "sebevražda", "smrt", "mrtvola",

        // ── ANGLICKY ────────────────────────────────────────────────────────

        // violence / weapons
        "gun", "weapon", "pistol", "knife", "grenade", "bomb", "rifle",
        "murder", "killing", "blood", "axe", "sword", "bullet", "shoot",
        "stab", "torture", "corpse", "gore",

        // alcohol / drugs
        "alcohol", "beer", "wine", "vodka", "rum", "whiskey", "whisky",
        "drug", "marijuana", "weed", "cocaine", "heroin", "meth",
        "ecstasy", "overdose", "needle",

        // profanity / sexual content
        "penis", "vagina", "sex", "porn", "porno", "erotic",
        "fuck", "shit", "bitch", "cunt", "cock", "ass", "dick",
        "nude", "naked", "boobs", "tits",

        // gambling
        "casino", "gambling", "roulette", "poker", "bet", "betting",
        "slots", "jackpot",

        // other inappropriate
        "suicide", "death", "corpse", "rape", "abuse", "pedophile",
        "racist", "racism", "nazi",
    };

    // -----------------------------------------------------------------------
    // (Volitelné) Povolená slova – pokud je seznam neprázdný,
    // projde POUZE slovo, které je v tomto seznamu.
    // Nechej prázdné = bez omezení whitelistu.
    // -----------------------------------------------------------------------
    private static readonly HashSet<string> Whitelist = new(StringComparer.OrdinalIgnoreCase)
    {
        // příklady bezpečných slov – rozšiř dle kategorií ze hry
        // "pes", "kočka", "auto", "stůl", ...
        // Pokud necháš prázdné, whitelist se nepoužije.
    };

    /// <summary>
    /// Vrátí true pokud je slovo bezpečné pro děti.
    /// </summary>
    public static bool IsAllowed(string? word)
    {
        if (string.IsNullOrWhiteSpace(word))
            return false;

        var w = word.Trim().ToLowerInvariant();

        // 1. Blacklist – explicitní zákaz
        if (Blacklist.Contains(w))
            return false;

        // 2. Whitelist – pokud je definován, musí slovo projít
        if (Whitelist.Count > 0 && !Whitelist.Contains(w))
            return false;

        return true;
    }
}