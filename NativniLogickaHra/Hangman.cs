using System;
using System.Collections.Generic;
using System.Text;

namespace NativniLogickaHra
{
    internal class Hangman
    {
        public static int approvalsCount;

        static public void Hadej(int countApproval,string input)
        {
            
            
            // Kontrola pokud byli hadany samohlasky
            char[] approval = { 'a', 'e', 'i', 'o', 'u', 'y' };
            bool isVowels = approval.Contains(Char.ToLower(input[0]));
            string choice = "programovani";      // slovo k hádání
            char[] chosen = choice.ToCharArray();
            char[] secret = new string('_', choice.Length).ToCharArray();
            int attempt = 0;
            int hangman = 0;
            int notGuess = 0;
            int countLetter = 0;
            // Pokud je to samohlaska a nesouhlaska a je mene nez 3 vrat se znova
            if (isVowels && approvalsCount < countApproval)
            {
                Console.WriteLine("Nemůžeš hádat samohlásky, dokud neuhádneš 3 souhlásky.");
                return;
            }

            for (int i = 0; i < choice.Length; i++)
            {

                if (chosen[i] == input[0])
                {
                    Console.WriteLine("Uhadl jsi.");
                    secret[i] = input[0];
                }
                else
                {
                    notGuess++;
                }
                if (countLetter == notGuess)
                {
                    Console.WriteLine("Neuhadl jsi");
                    attempt++;
                    countLetter = 0;
                    hangman++;
                }

            }
            
            
            // Zvetsovani uhadnutich souhlasek pokud bylu]i hadany
            if (!isVowels)
            {
                approvalsCount++;

            }
        }
    }
}
