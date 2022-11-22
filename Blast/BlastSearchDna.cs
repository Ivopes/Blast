using SuffixArrayBIO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace Blast
{
    internal class BlastSearchDna
    {
        private int[,] _blosumVals;
        private int _w;
        private int _T;
        private string[] _wMers;
        private string _toFind;
        //private Dictionary<string, Dictionary<string, int>> _blosumMutScore = new();
        private Dictionary<string, int>[] _blosumMutScore;
        private string _refer = "DAPCQEHKRGWPNDC";
        private FMIndex _fmIndex;

        public BlastSearchDna(string toFind, int w, int T)
        {
            _blosumVals = new int[4, 4];
            ReadBlossum("../../../files/BLOSUM62Dna.txt");

            _w = w;
            _T = T;
            _toFind = toFind;

            _wMers = new string[toFind.Length - _w + 1];
            _blosumMutScore = new Dictionary<string, int>[_wMers.Length];

            GenerateWmers(_w);

            double length = Math.Pow(4, _w);
            for (int i = 0; i < _wMers.Length; i++)
            {
                string wMer = _wMers[i];
                //_blosumMutScore.Add(wMer, new());
                _blosumMutScore[i] = new();

                var dict = _blosumMutScore[i];

                for (int j = 0; j < length; j++)
                {
                    string comb = GenerateComb(j, _w);

                    int score = GetBlossumScore(wMer, comb);

                    if (score >= T)
                    {
                        dict.Add(comb, score);
                    }
                }
            }

            if (File.Exists("./output"))
            {
                _fmIndex= new FMIndex("../../../files/chr17.fa", "./output");
            }
            else
            {
                _fmIndex= new FMIndex("../../../files/chr17.fa");
            }

        }

        private int GetBlossumScore(string s1, string s2)
        {
            int score = 0;

            for (int i = 0; i < s1.Length; i++)
            {
                score += GetBlossumScore(s1[i], s2[i]);
            }
            return score;
        }
        private int GetBlossumScore(char c1, char c2)
        {
            return _blosumVals[CtoI(c1), CtoI(c2)];
        }
        private void GenerateWmers(int length)
        {
            for (int i = 0; i < _wMers.Length; i++)
            {
                _wMers[i] = _toFind.Substring(i, length);
            }
        }
        private void ReadBlossum(string path)
        {
            var lines = File.ReadAllLines(path);

            char[] row = lines[0].Split(' ').Where(s => !string.IsNullOrEmpty(s)).Select(s => s[0]).ToArray();
            char[] column = lines.Select(l => l[0]).ToArray()[1..];

            var valueMatrix = lines[1..].Select(l => l[1..].Split(' ').Where(c => !string.IsNullOrEmpty(c)).ToArray()).ToArray();

            for (int i = 0; i < valueMatrix.Length; i++)
            {
                string[] line = valueMatrix[i];
                for (int j = 0; j < line.Length; j++)
                {
                    string c = line[j];
                    _blosumVals[i, j] = int.Parse(c);
                }
            }
        }
        private int CtoI(char c)
        {
            return c switch
            {
                'A' => 0,
                'C' => 1,
                'G' => 2,
                'T' => 3,
            };
        }
        private char ItoC(int i)
        {
            return i switch
            {
               0 => 'A',
               1 => 'C',
               2 => 'G',
               3 => 'T',
            };
        }
        private string GenerateComb(int index, int length)
        {
            char[] result = new char[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = ItoC(0);
            }

            for (int i = 0; i < length; i++)
            {
                result[result.Length - i - 1] = ItoC(index % 4);
                index /= 4;
            }

            return new string(result);
        }
        public void StartSearch(out List<string> references,out List<int> scores, out List<int> positions)
        {
            Console.WriteLine("Hledam...");
            references = new();
            scores = new();
            positions = new();
            for (int i = 0; i < _blosumMutScore.Length; i++)
            {
                Dictionary<string, int>? table = _blosumMutScore[i];
                foreach (var option in table)
                {
                    string mut = option.Key;
                    int mutScore = option.Value;
                    string mutToFind = _toFind[..i] + mut + _toFind[(i + _w)..];

                    // pro test
                    //List<int> pozice = AllIndexesOf(_refer, mut);
                    List<int> pozice = _fmIndex.Find(mut);

                    foreach (var poz in pozice)
                    {
                        int end = ExpandRight(mutToFind, poz, i, out int score1);
                        int start = ExpandLeft(mutToFind, poz + mut.Length, i + mut.Length, out int score2);

                        string foundMut = mutToFind[(i + mut.Length + start)..(i + end)];
                        //string foundReff = _refer[(poz + mut.Length + start)..(poz + end)];
                        string foundReff = _fmIndex._suffixArray.genom[(poz + mut.Length + start)..(poz + end)];

                        int endScore = score1 + score2 - GetBlossumScore(mut, mut);

                        if (endScore >= 0)
                        {
                            if (!references.Contains(foundReff))
                            {
                                references.Add(foundReff);
                                scores.Add(endScore);
                                positions.Add(poz + mut.Length + start);
                            }
                            else if (endScore > scores[references.IndexOf(foundReff)])
                            {
                                scores[references.IndexOf(foundReff)] = endScore;
                                positions.Add(poz + mut.Length + start);
                            }
                        }
                    }
                }
            }
        }
        // Vraci pravy index zastaveni
        private int ExpandRight(string toFind, int posLeftCornerRef, int posLeftFind, out int score)
        {
            int i = 0;
            score = 0;
            do
            {
                int s = GetBlossumScore(toFind[posLeftFind + i], _fmIndex._suffixArray.GetChar(posLeftCornerRef + i));
                //int s = GetBlossumScore(toFind[posLeftFind + i], _refer[posLeftCornerRef + i]);
                if (s < -2)
                {
                    break;
                }
                score += s;
            } while ((++i + posLeftFind) < toFind.Length);

            return i;
        }
        //vraci levy index zastaveni
        private int ExpandLeft(string toFind, int posRightCornerRef, int posRightFind, out int score)
        {
            int i = 0;
            score = 0;
            do
            {
                int s = GetBlossumScore(toFind[posRightFind + i - 1], _fmIndex._suffixArray.GetChar(posRightCornerRef + i - 1));
                //int s = GetBlossumScore(toFind[posRightFind + i - 1], _refer[posRightCornerRef + i - 1]);
                if (s < -2)
                {
                    break;
                }
                score += s;
            } while ((posRightFind + --i - 1) >= 0);

            return i;
        }
        private static List<int> AllIndexesOf(string str, string value)
        {
            if (String.IsNullOrEmpty(value))
                throw new ArgumentException("the string to find may not be empty", "value");
            List<int> indexes = new List<int>();
            for (int index = 0; ; index += value.Length)
            {
                index = str.IndexOf(value, index);
                if (index == -1)
                    return indexes;
                indexes.Add(index);
            }
        }
    }
}
