using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blast
{
    internal class BlastSearch
    {
        private int[,] _blosumVals;
        private int _w;
        private int _T;
        private string[] _wMers;
        private string _toFind;
        private Dictionary<string, Dictionary<string, int>> _blosumMutScore = new();
        private string _refer = "DAPCQEHKRGWPNDC";
        public BlastSearch(string toFind, int w, int T)
        {
            _blosumVals = new int[20, 20];
            ReadBlossum("../../../files/BLOSUM62.txt");

            _w = w;
            _T = T;
            _toFind = toFind;

            _wMers = new string[toFind.Length - _w + 1];

            GenerateWmers();

            double length = Math.Pow(20, _w);

            foreach (var wMer in _wMers)
            {
                _blosumMutScore.Add(wMer, new());
                var dict = _blosumMutScore[wMer];
                for (int i = 0; i < length; i++)
                {
                    string comb = GenerateComb(i, _w);

                    int score = GetBlossumScore(wMer, comb);

                    if (score >= T)
                    {
                        dict.Add(comb, score);
                    }
                }
            }
        }

        private int GetBlossumScore(string wMer, string comb)
        {
            int score = 0;

            for (int i = 0; i < wMer.Length; i++)
            {
                score += _blosumVals[CtoI(wMer[i]), CtoI(comb[i])];
            }
            return score;
        }

        private void GenerateWmers()
        {
            for (int i = 0; i < _wMers.Length; i++)
            {
                _wMers[i] = _toFind.Substring(i, 3);
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
                'R' => 1,
                'N' => 2,
                'D' => 3,
                'C' => 4,
                'Q' => 5,
                'E' => 6,
                'G' => 7,
                'H' => 8,
                'I' => 9,
                'L' => 10,
                'K' => 11,
                'M' => 12,
                'F' => 13,
                'P' => 14,
                'S' => 15,
                'T' => 16,
                'W' => 17,
                'Y' => 18,
                'V' => 19,
            };
        }
        private char ItoC(int i)
        {
            return i switch
            {
               0 => 'A',
               1 => 'R',
               2 => 'N',
               3 => 'D',
               4 => 'C',
               5 => 'Q',
               6 => 'E',
               7 => 'G',
               8 => 'H',
               9  => 'I',
               10 => 'L',
               11 => 'K',
               12 => 'M',
               13 => 'F',
               14 => 'P',
               15 => 'S',
               16 => 'T',
               17 => 'W',
               18 => 'Y',
               19 => 'V',
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
                result[result.Length - i - 1] = ItoC(index % 20);
                index /= 20;
            }

            return new string(result);
        }
    }
}
