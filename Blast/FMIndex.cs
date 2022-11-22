using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuffixArrayBIO
{
    public class FMIndex
    {
        public SuffixArray _suffixArray;
        char[] _fArray;
        char[] _lArray;
        int[,] _rankArray;
        int[] _countArray;
        int _rankStep;
        int _suffStep;
        const int _symbolsCount = 6;
        public FMIndex()
        {
            _suffStep = 2;
            
            _suffixArray = new SuffixArray();
            
            BuildFLArray(_suffixArray);

            //_suffixArray.ReduceSize(_suffStep);
            BuildRankArray(_suffixArray.ArraySize());


            //var r = Find("AC");
            //r.ForEach(x => Console.WriteLine(x));
        }
        public FMIndex(string filePath, bool save = true)
        {
            Console.WriteLine("Vytvarim FMindex...");
            _suffixArray = new SuffixArray(filePath, save);

            BuildFLArray(_suffixArray);

            //_suffixArray.ReduceSize(_suffStep);
            BuildRankArray(_suffixArray.ArraySize());
            Console.WriteLine("Hotovo");
        }
        public FMIndex(string filePathGenom, string filePath)
        {
            Console.WriteLine("Vytvarim FMindex...");
            _suffixArray = new SuffixArray(filePathGenom, filePath);

            BuildFLArray(_suffixArray);

            //_suffixArray.ReduceSize(_suffStep);
            BuildRankArray(_suffixArray.ArraySize());
            Console.WriteLine("Hotovo");
        }
        public List<int> Find(string s)
        {

            bool found = false;
            int index = s.Length;
            int res1;
            int res2;
            char c = s[--index];
            FindInF(c, out res1, out res2);
            do
            {
                c = s[--index];

                if (!FindInL(c, res1, res2, out res1, out res2)) return new List<int>();

                FindInF(c, res1, res2 - res1 + 1, out res1, out res2);
                
            } while (index > 0);

            List<int> result = new();
            for (int i = res1; i <= res2; i++)
            {
                result.Add(FindSuffValue(i));
            }

            return result;
        }
        private void BuildFLArray(SuffixArray suffixArray, int takeEvery = 1)
        {
            int length = suffixArray.ArraySize();

            _fArray = new char[length];
            _lArray = new char[length];

            _countArray = new int[_symbolsCount];

            for (int i = 0; i < length; i++)
            {
                if (i % takeEvery != 0) continue;

                int arrayValue = suffixArray[i];
                _fArray[i] = suffixArray.GetChar(arrayValue);
                if (arrayValue == 0)
                {
                    _lArray[i] = '$';
                }
                else
                {
                    _lArray[i] = suffixArray.GetChar(arrayValue - 1);
                }
                _countArray[CharToInt(_lArray[i])]++;
            }
        }
        private void BuildRankArray(int length, int takeEvery = 1)
        {
            _rankArray = new int[length, _symbolsCount];
            _rankStep = takeEvery;
            char c;

            //fill $
            int counter = 0;
            for (int i = 0; i < _rankArray.GetLength(0); i++)
            {
                c = _lArray[i];
                if (c == '$')
                {
                    counter++;
                }
                if (i % _rankStep == 0)
                    _rankArray[i, 0] = counter;
            }
            //fill A
            counter = 0;
            for (int i = 0; i < _rankArray.GetLength(0); i++)
            {
                c = _lArray[i];
                if (c == 'A')
                {
                    counter++;
                }
                if (i % _rankStep == 0)
                    _rankArray[i, 1] = counter;
            }
            //fill C
            counter = 0;
            for (int i = 0; i < _rankArray.GetLength(0); i++)
            {
                c = _lArray[i];
                if (c == 'C')
                {
                    counter++;
                }
                if (i % _rankStep == 0)
                    _rankArray[i, 2] = counter;
            }
            //fill G
            counter = 0;
            for (int i = 0; i < _rankArray.GetLength(0); i++)
            {
                c = _lArray[i];
                if (c == 'G')
                {
                    counter++;
                }
                if (i % _rankStep == 0)
                    _rankArray[i, 3] = counter;
            }
            //fill N
            counter = 0;
            for (int i = 0; i < _rankArray.GetLength(0); i++)
            {
                c = _lArray[i];
                if (c == 'N')
                {
                    counter++;
                }
                if (i % _rankStep == 0)
                    _rankArray[i, 4] = counter;
            }
            //fill T
            counter = 0;
            for (int i = 0; i < _rankArray.GetLength(0); i++)
            {
                c = _lArray[i];
                if (c == 'T')
                {
                    counter++;
                }
                if (i % _rankStep == 0)
                    _rankArray[i, 5] = counter;
            }
        }
        private int CharToInt(char c)
        {
            return c switch
            {
                '$' => 0,
                'A' => 1,
                'C' => 2,
                'G' => 3,
                'N' => 4,
                'T' => 5,
                _ => -1,
            };
        }
        /// <summary>
        /// Najde rozsah vsech znaku c v F
        /// </summary>
        private void FindInF(char c,out int rStart,out int rStop)
        {
            int stop = CharToInt(c);

            int start = 0;
            for (int i = 0; i < stop; i++)
            {
                start += _countArray[i];
            }


            rStart = start;

            rStop = start + _countArray[stop]-1;
        }
        /// <summary>
        /// Najde rozsah od rank indexu cStart do cStart+count kde se nachazi znak c v F. Vraci rozsah radku
        /// </summary>
        private void FindInF(char c, int cStart, int count, out int rStart, out int rStop)
        {
            FindInF(c,out rStart,out rStop);

            rStart += cStart;
            rStop = rStart + count - 1;
        }
        /// <summary>
        /// Najde rozsah od radku cStart do cStop kde se nachazi znak c v L - vraci Rank indexy
        /// </summary>
        private bool FindInL(char c, int cStart, int cStop, out int rStart, out int rStop)
        {
            List<int> occurences = new();
            
            for (int i = cStart; i <= cStop; i++)
            {
                if (_lArray[i] == c)
                {
                    occurences.Add(i);
                }
            }
            rStart = -1;
            rStop = -1;

            if (occurences.Count == 0) return false;
            
            rStart = RankChar(c, occurences[0]) - 1;
            // asi staci + count
            rStop = RankChar(c, occurences[occurences.Count-1]) - 1;

            return true;
        }
        private int RankChar(char c, int index)
        {
            int cId = CharToInt(c);

            int remains = index % _rankStep;

            if (remains == 0)
            {
                return _rankArray[index, cId];
            }

            int half = _rankStep / 2;
            int counter = 0;
            if (remains <= half)
            {
                counter = _rankArray[index - remains, cId];
                for (int i = index - remains+1; i <= index; i++)
                {
                    if (_lArray[i] == c)
                    {
                        counter++;
                    }
                }
            }
            else
            {
                counter = _rankArray[index + (_rankStep - remains), cId];
                for (int i = index + (_rankStep - remains); i > index; i--)
                {
                    if (_lArray[i] == c)
                    {
                        counter--;
                    }
                }
            }

            return counter;
        }
        private int FindSuffValue(int row)
        {
            char c = _lArray[row];

            if (c == '$') return 0;

            int i = row;
            int counter = 0;
            int start, stop;
            while(_suffixArray[i] == 0)
            {
                var rank = RankChar(c, i)-1;
                FindInF(c, rank, 1, out start, out stop);

                i = start;

                c = _lArray[i];
                counter++;
            }

            return _suffixArray[i] + counter;
        }
    }
}
