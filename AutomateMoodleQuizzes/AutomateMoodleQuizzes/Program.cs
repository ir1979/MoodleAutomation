using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Text.RegularExpressions;

namespace AutomateMoodleQuizzes
{
    class Program
    {
        static string inputfilename = "data.txt";
        static string outputfilenameSpell, outputfilenamePreposition;
        static string[] commonWords = null;
        static string[] allWords = null;
        static int minWordLength = 5;
        static int maxTries = 1000;
        static System.Random rnd = new System.Random();
        static char[,] similar = new char[26,3] {
        {'i', 'o', 'u'}, //a
        {'c', 'd', 'p'}, //b
        {'k', 'f', 's'}, //c
        {'b', 'z', 't'}, //d
        {'i', 'a', 'y'}, //e
        {'p', 'b', 'r'}, //f
        {'j', 'q', 'd'}, //g
        {'e', 'r', 'e'}, //h
        {'y', 'a', 'e'}, //i
        {'g', 'd', 'h'}, //j
        {'l', 'a', 'e'}, //k
        {'m', 'n', 'e'}, //l
        {'n', 'o', 'l'}, //m
        {'m', 'e', 'k'}, //n
        {'a', 'u', 'y'}, //o
        {'b', 'f', 'r'}, //p
        {'c', 'u', 'k'}, //q
        {'v', 'd', 'e'}, //r
        {'c', 'z', 'b'}, //s
        {'d', 'b', 'z'}, //t
        {'a', 'y', 'o'}, //u
        {'w', 'y', 'b'}, //v
        {'y', 'b', 'd'}, //w
        {'z', 'e', 's'}, //x
        {'u', 'a', 'e'}, //y
        {'s', 'e', 'w'}, //z
        };

        
        static string[] readCommonWords()
        {
            System.IO.StreamReader sr = new System.IO.StreamReader("common10k.txt");
            string[] result = sr.ReadToEnd().Split("\n \t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            sr.Close();
            return result;
        }

        static string[] readAllWords()
        {
            System.IO.StreamReader sr = new System.IO.StreamReader("words_alpha.txt");
            string[] result = sr.ReadToEnd().Split("\n \t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            sr.Close();
            return result;
        }

        static bool Uncommon(string w)
        {
            return Array.IndexOf(commonWords, w) < 0;
        }


        static string makeSpellQuestion(string w)
        {
            string r = "Find the missing letter in  ";
            int randomIndex = rnd.Next(w.Length);
            char randomChar = w[randomIndex];

            string q = w.Substring(0, randomIndex) + "_" + w.Substring(randomIndex + 1);   // the questioned word with a missing letter
            r += q + "  \r\n" + "A) " + randomChar + "\r\n";

            char c2, c3, c4;

            c2 = similar[randomChar - 'a', 0];
            c3 = similar[randomChar - 'a', 1];
            c4 = similar[randomChar - 'a', 2];

            int tries = 0;
            while (tries++ < maxTries && (c2 == randomChar || Array.IndexOf(allWords, q.Replace('_', c2)) >= 0))
                c2 = (char)('a' + rnd.Next(26));
            r += "B) " + c2 + "\r\n";
            if (tries == maxTries)
                return "";

            tries = 0;
            while (tries++ < maxTries && (c3 == c2 || c3 == randomChar || Array.IndexOf(allWords, q.Replace('_', c3)) >= 0))
                c3 = (char)('a' + rnd.Next(26));
            r += "C) " + c3 + "\r\n";
            if (tries == maxTries)
                return "";

            tries = 0;
            while (tries++ < maxTries && (c4 == c3 || c4 == c2 || c4 == randomChar || Array.IndexOf(allWords, q.Replace('_', c4)) >= 0))
                c4 = (char)('a' + rnd.Next(26));
            r += "D) " + c4 + "\r\n";
            if (tries == maxTries)
                return "";

            r += "ANSWER: A" + "\r\n";
            return r;
        }


        static void generateSpellQuestions(string originalData)
        {
            string allData = Regex.Replace(originalData, @"[^a-z]", " ");
            allData = new string(allData.Where(c => char.IsLetter(c) || c == ' ').ToArray());
            string[] words = allData.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            words = words.Distinct().Where(w => Uncommon(w)).OrderBy(w => w).OrderBy(w => w.Length).Where(w => w.Length >= minWordLength).ToArray();

            System.IO.StreamWriter sw = new System.IO.StreamWriter(outputfilenameSpell, true);

            foreach (string w in words)
            {
                sw.WriteLine(makeSpellQuestion(w));
            }
            sw.Close();

        }

        static void generatePrepositionQuestions(string originalData)
        {
            var sentences = Regex.Split(originalData, "([A-Z][^\t\r\n.?!]{10,100}[\t\r\n.?!])[^a-zA-Z]")
                .Select(s => s.Replace(System.Environment.NewLine, ""))
                .Select(s => s.Trim())
                .Where(p => p.Length>=10 && p.Length<=100 && p != string.Empty)
                .Where(p=> p[0]>='A' && p[0]<='Z' && !p.Contains("...") && !p.Contains("…") && (p.EndsWith(".") || p.EndsWith("!") || p.EndsWith(".")))
                .ToList();

            System.IO.StreamWriter sw = new System.IO.StreamWriter(outputfilenamePreposition, true);

            foreach (var sentenceStr in sentences)
            {
                string[] words = sentenceStr.Split(" .!()[],0123456789".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                foreach(var word in words)
                {
                    if (word.ToString() == "in")
                    {
                        sw.WriteLine(sentenceStr.Replace(" in ", " ... "));
                        sw.WriteLine("A) in");
                        sw.WriteLine("B) on");
                        sw.WriteLine("C) at");
                        sw.WriteLine("D) for");
                        sw.WriteLine("ANSWER: A");
                        sw.WriteLine();
                        break;
                    } else if (word.ToString() == "on")
                    {
                        sw.WriteLine(sentenceStr.Replace(" on ", " ... "));
                        sw.WriteLine("A) on");
                        sw.WriteLine("B) in");
                        sw.WriteLine("C) at");
                        sw.WriteLine("D) for");
                        sw.WriteLine("ANSWER: A");
                        sw.WriteLine();
                        break;
                    }
                    else if (word.ToString() == "at")
                    {
                        sw.WriteLine(sentenceStr.Replace(" at ", " ... "));
                        sw.WriteLine("A) at");
                        sw.WriteLine("B) on");
                        sw.WriteLine("C) in");
                        sw.WriteLine("D) for");
                        sw.WriteLine("ANSWER: A");
                        sw.WriteLine();
                        break;
                    }
                    else if (word.ToString() == "for")
                    {
                        sw.WriteLine(sentenceStr.Replace(" for ", " ... "));
                        sw.WriteLine("A) for");
                        sw.WriteLine("B) on");
                        sw.WriteLine("C) at");
                        sw.WriteLine("D) in");
                        sw.WriteLine("ANSWER: A");
                        sw.WriteLine();
                        break;
                    }
                }
            }

            sw.Close();
        }

        static void Main(string[] args)
        {
            if (args.Count()>0)
            { 
                inputfilename = args[0]; 
            }
            else
            {
                Console.WriteLine("Please enter input filename: ");
                inputfilename = Console.ReadLine();
            }
            outputfilenameSpell = System.IO.Path.GetFileNameWithoutExtension(inputfilename) + "_Spell.txt";
            outputfilenamePreposition = System.IO.Path.GetFileNameWithoutExtension(inputfilename) + "_Preposition.txt";

            commonWords = readCommonWords();
            allWords = readAllWords();

            System.IO.StreamReader sr = new System.IO.StreamReader(inputfilename);
            string allDataOriginal = sr.ReadToEnd();

            generateSpellQuestions(allDataOriginal.ToLower());
            generatePrepositionQuestions(allDataOriginal);
            
            Console.WriteLine("Finished.");

        }


    }
}
