using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using System.Diagnostics;

namespace SpellChecker;
[MemoryDiagnoser(true)]
[SimpleJob(runStrategy: RunStrategy.ColdStart, launchCount: 1, iterationCount: 1, invocationCount:1)]

public class SpellCheck
{
    [Benchmark]
    public void Check()
    {
        string words = "rain spain plain plaint pain main mainly the in on fall falls his was ";
        string input = "hte rame in pain fells mainy oon teh lain was hints pliant";
        

        // если раскоментировать две строки снизу, то в словарь добавится ещё 250к слов (просто для оценки затрат ресурсов)
        // но при таком большом количестве слов в вловаре на выходе появляются не совсем корректные данные,
        // это связано с тем, что среди 250к слов есть такие, которые лучше чем изначальные подходят для замены по условиям ТЗ

        //string[] w = File.ReadAllText("words250k.json").Split(',', StringSplitOptions.RemoveEmptyEntries);
        //words += string.Join(' ', w);

        string output = "";

        List<string> wordsToList = words.Split(" ").ToList();
        
        List<string> inputToList = input.Split(" ").ToList();

        // Определим минимальное и максимальное количество букв в стовах (исходя из условия о максимальном количестве правок = 2)
        List<string> ordered = inputToList.OrderBy(x => x.Length).ToList();
        int minNum = ordered[0].Length;
        int maxNum = ordered[ordered.Count - 1].Length;

        // Сразу значительно сокращаем список слов для замены отсекая слишком длинные и слишком короткие        
        wordsToList = wordsToList.Where(_ => _.Length > minNum - 2 && _.Length < maxNum + 2).ToList();
        int num = wordsToList.Count;


        List<PairWord> pairWords = new List<PairWord>();

        for (int i = 0; i < inputToList.Count; i++)
        {
            List<DTO> dtos = new List<DTO>();
            for (int j = 0; j < wordsToList.Count; j++)
            {
                int c = LevenshteinDistance(inputToList[i], wordsToList[j]);
                dtos.Add(new DTO { numberOfSteps = c, word = wordsToList[j] });

            }
            dtos = dtos.OrderBy(_ => _.numberOfSteps).ToList();

            int n = dtos[0].numberOfSteps;
            //если подходит несколько вариантов, то беру самый подходящий, если одинаково хорошо подходят несколько вариантов, то беру все
            dtos = dtos.Where(_ => _.numberOfSteps == n).ToList();

            //исключаем варианты, в которых более 2 корректировок
            if (dtos[0].numberOfSteps > 2)
            {
                string str = "{" + $"{inputToList[i]}" + "?}";
                pairWords.Add(new PairWord { OriginalWord = inputToList[i], NewWord = str });
                continue;
            }
            //здесь будет 0, если в словаре есть 100% совпадение со словом из input
            if (dtos[0].numberOfSteps == 0)
            {
                pairWords.Add(new PairWord { OriginalWord = inputToList[i], NewWord = inputToList[i] });
                continue;
            }
            if (dtos.Count == 1)
            {
                //проверяю на 2 удаления или добавления подряд
                if (dtos[0].numberOfSteps == 2)
                {
                    string original = inputToList[i].ToString();
                    string replace = dtos[0].word.ToString();

                    bool shouldBeReplace = Service.check(original, replace);

                    if (!shouldBeReplace)
                    {
                        pairWords.Add(new PairWord { OriginalWord = inputToList[i], NewWord = "{" + inputToList[i] + "?}" });
                        continue;
                    }

                    pairWords.Add(new PairWord { OriginalWord = inputToList[i], NewWord = dtos[0].word });
                    continue;
                }

                pairWords.Add(new PairWord { OriginalWord = inputToList[i], NewWord = dtos[0].word });
                continue;
            }

            if (dtos.Count > 1)
            {
                string str = "{";
                foreach (DTO d in dtos)
                {
                    str = str + $"{d.word} ";
                }
                str = str.TrimEnd(' ');
                str += "}";

                pairWords.Add(new PairWord { OriginalWord = inputToList[i], NewWord = str });

            }

        }

        foreach (PairWord s in pairWords)
        {
            output += $"{s.NewWord} ";
        }
        Console.WriteLine("Input: " + input);
        Console.WriteLine();
        Console.WriteLine("Output: " + output);

    }

    int Minimum(int a, int b, int c)
    {
        if (a > b)
        {
            a = b;
        }

        if (a > c)
        {
            a = c;
        }

        return a;
    }

    //Стандартная реализация алгоритма Вагнера-Фишера (взял в гугле)
    int LevenshteinDistance(string firstWord, string secondWord)
    {
        int n = firstWord.Length + 1;
        int m = secondWord.Length + 1;
        var matrixD = new int[n, m];

        int deletioncost = 1;
        int insertionCost = 1;

        for (int i = 0; i < n; i++)
        {
            matrixD[i, 0] = i;
        }
        for (int j = 0; j < m; j++)
        {
            matrixD[0, j] = j;
        }


        for (int i = 1; i < n; i++)
        {
            for (int j = 1; j < m; j++)
            {
                int substitutionCost = firstWord[i - 1] == secondWord[j - 1] ? 0 : 2;

                matrixD[i, j] = Minimum(
                                        matrixD[i - 1, j] + deletioncost,
                                        matrixD[i, j - 1] + insertionCost,
                                        matrixD[i - 1, j - 1] + substitutionCost);

            }
        }

        return matrixD[n - 1, m - 1];
    }
}
