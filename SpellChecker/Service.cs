using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChecker
{
    public static class Service
    {
        public static bool check(string word1, string word2)
        {
            int num = word1.Length - word2.Length;

            if (num < 0)
            {
                string save = word2;

                word2 = word1;
                word1 = save;
            }

            string res1 = "";
            string res2 = "";
            List<char> list1 = word1.ToList();
            List<char> list2 = word2.ToList();

            foreach (char c in list2)
            {
                for (int i = 0; i < list1.Count; i++)
                {
                    if (list1[i] == c)
                    {
                        list1[i] = '_';
                    }
                }
            }
            foreach (char c in list1)
            {
                if (c != '_')
                {
                    res1 += c;
                }
            }
            for (int i = res1.Length - 1; i >= 0; i--)
            {
                res2 += res1[i];
            }

            if (String.IsNullOrEmpty(res1) || res1.Length==1)
            {
                return true;
            }
            if (word1.Contains(res1) || word1.Contains(res2))
            {
                return false;
            }
            else
            {
                return true;
            }

        }
    }
}
