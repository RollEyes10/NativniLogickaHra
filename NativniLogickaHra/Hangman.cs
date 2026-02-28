using System.Linq;
using NativniLogickaHra.Utils;

namespace NativniLogickaHra;

public class Hangman
{
    public string TargetWord { get; }
    public char[] SecretWord { get; }
    public int ConsonantsGuessed { get; private set; }
    public int RemainingAttempts { get; private set; }
    public int RequiredConsonants { get; }

    private readonly string normalizedWord;
    private readonly bool vowelsEnabled;
    private int remainingVowels;
    private readonly int requiredConsonants;

    // Y je v češtině SOUHLÁSKA — odstraněno z pole samohlásek
    private readonly char[] vowels = { 'a', 'e', 'i', 'o', 'u' };

    public Hangman(string word, int attempts, bool vowelsEnabled, int vowelsCount, int requiredConsonants = 3)
    {
        TargetWord = word.ToLower();
        normalizedWord = TextUtils.RemoveDiacritics(TargetWord);

        SecretWord = TargetWord.Select(c => char.IsLetter(c) ? '_' : c).ToArray();

        RemainingAttempts = attempts;
        ConsonantsGuessed = 0;
        this.vowelsEnabled = vowelsEnabled;
        remainingVowels = vowelsEnabled ? vowelsCount : 0;
        this.requiredConsonants = Math.Max(0, requiredConsonants);
        RequiredConsonants = this.requiredConsonants;
    }

    public string Guess(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "Zadej písmeno!";

        char guess = char.ToLower(input[0]);
        bool isVowel = vowels.Contains(guess);

        if (isVowel && !vowelsEnabled)
            return "Samohlásky jsou vypnuté!";

        if (isVowel && ConsonantsGuessed < requiredConsonants)
            return $"Musíš nejdříve uhodnout {requiredConsonants} souhlásek!";

        if (isVowel && remainingVowels <= 0)
            return "Došly ti samohlásky!";

        bool found = false;

        for (int i = 0; i < normalizedWord.Length; i++)
        {
            if (normalizedWord[i] == guess)
            {
                SecretWord[i] = TargetWord[i];
                found = true;
            }
        }

        if (found)
        {
            if (isVowel)
                remainingVowels--;
            else
                ConsonantsGuessed++;

            return "Správně!";
        }

        RemainingAttempts--;
        return "Špatně!";
    }
}