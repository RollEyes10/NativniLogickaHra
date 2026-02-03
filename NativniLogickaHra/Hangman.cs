using System.Linq;

namespace NativniLogickaHra
{
    public class Hangman
    {
        public string TargetWord { get; private set; }
        public char[] SecretWord { get; private set; }
        public int ConsonantsGuessed { get; private set; }
        public int RemainingAttempts { get; private set; }

        public int RequiredConsonants { get; }

        private readonly bool vowelsEnabled;
        private int remainingVowels;
        private readonly int requiredConsonants;

        private readonly char[] vowels = { 'a', 'e', 'i', 'o', 'u', 'y' };

        public Hangman(
            string word,
            int attempts,
            bool vowelsEnabled,
            int vowelsCount,
            int requiredConsonants = 3)
        {
            TargetWord = word.ToLower();
            SecretWord = new string('_', word.Length).ToCharArray();
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

            // ❌ Samohlásky vypnuté
            if (isVowel && !vowelsEnabled)
                return "Samohlásky jsou vypnuté!";

            // 🔒 Původní pravidlo – nejdřív určitý počet souhlásek
            if (isVowel && ConsonantsGuessed < requiredConsonants)
                return $"Musíš nejdříve uhodnout {requiredConsonants} souhlásek!";

            // 🔢 Limit samohlásek
            if (isVowel && remainingVowels <= 0)
                return "Došly ti samohlásky!";

            bool found = false;

            for (int i = 0; i < TargetWord.Length; i++)
            {
                if (TargetWord[i] == guess)
                {
                    SecretWord[i] = guess;
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
            else
            {
                RemainingAttempts--;
                return "Špatně!";
            }
        }
    }
}
