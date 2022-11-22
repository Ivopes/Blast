using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuffixArrayBIO
{
    public class SuffixArray
    {
        Dictionary<string, int> prefixDict;
        Dictionary<string, int> prefixOffsetDict;
        string[] OrderedPrefixes;
        public string genom;
        public int[] _suffixArray;
        int maxThreadCount = -1;
        int threadCount = 0;

        public int this[int index]
        {
            get { return _suffixArray[index]; }
        }
        //pro test bez souboru
        public SuffixArray()
        {
            genom = "CTTTCCACTTGATAAGAGGTCCCANNNAGACTTAGTACCTGGAGGGTGAAATATTCTCCATCCAGTGGTTTCTTCTTTNNNGGCTGGGGAGAGGAGCTGGTGTTGTTGGGCAGTGCTAGGAAAGAGGCAAGGAAAGGTGATAAAAGTGAATCTGAGG$"; // 12,11,8,5,2,9,6,3,0,10,7,4,1
            //genom = "ACAACA$";
            int length = genom.Length;

            if (length < 3)
            {
                throw new Exception("kratky retezec");
            }
            
            string prefix = "$" + genom[0].ToString() + genom[1].ToString();

            prefixDict = new Dictionary<string, int>();

            int endIndex = 2;
            do
            {
                prefix = prefix.Substring(1);
                prefix += genom[endIndex];

                if (!prefixDict.ContainsKey(prefix))
                {
                    prefixDict.Add(prefix, 0);
                }

                prefixDict[prefix]++;

                endIndex++;
            } while (endIndex < length - 1);

            //pridat znaky na konci
            prefixDict.Add(genom.Substring(length - 2 - 1), 1);
            prefixDict.Add(genom.Substring(length - 1 - 1), 1);

            prefixOffsetDict = new(prefixDict);
            foreach (var entry in prefixOffsetDict)
            {
                prefixOffsetDict[entry.Key] = 0;
            }

            string[] prefixes = prefixDict.Keys.ToArray();

            //prefixes = new[] { "ABA", "BAA", "BBB", "AAB", "AAA", "BBA", "ABB", "BAB" };
            //QuickSortArray(prefixes, 0, prefixes.Length-1);


            // setridit prihradky prefixu
            RadixSort(prefixes, 3);
            OrderedPrefixes = prefixes;
            //vytvor pole pro prihradky - vysledek
            int elementCount = prefixDict.Values.Sum() + 1;

            int[] suffArray = new int[elementCount];

            suffArray[0] = genom.Length - 1; // posledni znak - konec 

            prefix = "$" + genom[0].ToString() + genom[1].ToString();
            endIndex = 2;
            do
            {
                prefix = prefix.Substring(1);
                prefix += genom[endIndex];

                
                int prefixIndex = OffsetToInsert(prefix);

                suffArray[prefixIndex] = endIndex - 2;
                
                endIndex++;

            } while (endIndex < length - 1);
            // konecne pripady mimo loop
            prefix = genom.Substring(length - 2 - 1);

            suffArray[OffsetToInsert(prefix)] = endIndex++ - 2;

            prefix = genom.Substring(length - 1 - 1);

            suffArray[OffsetToInsert(prefix)] = endIndex++ - 2;


            //a ted jeste seradit prihradky
            int i = 0;
            foreach (var entry in prefixDict)
            {
                int start = PrefixOffset(entry.Key);
                int end = start + entry.Value - 1;

                QuickSortArray(suffArray, start, end);
                /*var res = QuickSortArray(suffArray, start, end);

                for (int j = 0; j < res.Length; j++)
                {
                    suffArray[start + j] = res[j];
                }*/
                i++;
            }
            // dostat retezec se dvema symboli na spravne misto
            //QuickSortArray(suffArray, 1, suffArray.Length-1);

            _suffixArray = suffArray;
        }
        public SuffixArray(string filePath, bool save)
        {
            genom = File.ReadAllText(filePath); 

            genom = genom.Substring(6); // ignore prvni radek
            genom = genom.Replace("\n", "").Replace("\r", "").ToUpper();
            //genom = new string(genom.Where(c => c == 'A' || c == 'C' || c == 'G' || c == 'T').ToArray());
            //genom = genom.Substring(genom.Length / 100 * 99); pro zkraceni
            genom += "$";

            File.WriteAllText("./genom.txt", genom);

            int length = genom.Length;

            /*genom = "ACAACAACAACA$";
            int[] arr = new int[] { 11, 8, 6, 7, 4, 0, 1, 2, 3, 5, 12, 9, 10 };
            Quicksort(arr, 0, arr.Length);
            */
            if (length < 3)
            {
                throw new Exception("kratky retezec");
            }

            string prefix = "$" + genom[0].ToString() + genom[1].ToString();

            prefixDict = new Dictionary<string, int>();

            Console.WriteLine("Ctu trojice...");
            
            int endIndex = 2;
            // zpocitam pocet vyskytu jednotlivych trojic
            do
            {
                prefix = prefix.Substring(1);
                prefix += genom[endIndex];

                if (!prefixDict.ContainsKey(prefix))
                {
                    prefixDict.Add(prefix, 0);
                }

                prefixDict[prefix]++;

                endIndex++;
            } while (endIndex < length - 1);

            //pridat znaky na konci o delce 2 a 1
            prefixDict.Add(genom.Substring(length - 2 - 1), 1);
            prefixDict.Add(genom.Substring(length - 1 - 1), 1);
            Console.WriteLine("HOTOVO");

            Console.WriteLine("Vytvarim offsety pro prefixy...");
            // dict at pozdeji vim, jaky offset ma ktera trojice (kam mam ukladat pri plneni prihradek)
            prefixOffsetDict = new(prefixDict);
            foreach (var entry in prefixOffsetDict)
            {
                prefixOffsetDict[entry.Key] = 0;
            }
            Console.WriteLine("HOTOVO");

            string[] prefixes = prefixDict.Keys.ToArray();

            //prefixes = new[] { "ABA", "BAA", "BBB", "AAB", "AAA", "BBA", "ABB", "BAB" };
            //QuickSortArray(prefixes, 0, prefixes.Length - 1); // pro test

            Console.WriteLine("Spoustim radix sort...");
            // setridit prihradky prefixu
            // RadixSort(prefixes, 3);
            QuickSortArray(prefixes, 0, prefixes.Length - 1);
            Console.WriteLine("HOTOVO");

            OrderedPrefixes = prefixes;

            //vytvorim pole pro prihradky - vysledek, ale prihradky jeste nesetridene
            Console.WriteLine("Vytvarim nesetridene sufixArray...");
            int elementCount = prefixDict.Values.Sum() + 1;

            int[] suffArray = new int[elementCount];

            suffArray[0] = genom.Length - 1; // posledni znak - konec 

            prefix = "$" + genom[0].ToString() + genom[1].ToString();
            endIndex = 2;
            do
            {
                prefix = prefix.Substring(1);
                prefix += genom[endIndex];
                
                int prefixIndex = OffsetToInsert(prefix);
                
                suffArray[prefixIndex] = endIndex - 2;

                endIndex++;

            } while (endIndex < length - 1);
            // konecne pripady mimo loop
            prefix = genom.Substring(length - 2 - 1);

            suffArray[OffsetToInsert(prefix)] = endIndex++ - 2;

            prefix = genom.Substring(length - 1 - 1);

            suffArray[OffsetToInsert(prefix)] = endIndex++ - 2;
            
            Console.WriteLine("HOTOVO");

            //a ted jeste seradit jednotlive prihradky
            Console.WriteLine("Quicksort na sufixArray...");
            /*for (int i = 0; i < OrderedPrefixes.Length; i++)
            {
                var pref = OrderedPrefixes[i];
                var prefEnd = prefixDict[pref];
                int start = PrefixOffset(pref);
                int end = start + prefEnd - 1;
                Console.WriteLine($"prefix: {pref}...");
                QuickSortArray(suffArray, start, end);
                //Quicksort(suffArray, start, end + 1);
                //RadixSort(suffArray, start, end);
                Console.WriteLine($"end");
            }*/
            

            foreach (var entry in prefixDict)
            {
                if (entry.Key.StartsWith("N")) continue;
                int start = PrefixOffset(entry.Key);
                int end = start + entry.Value - 1;
                Console.WriteLine($"prefix: {entry.Key}...");
                QuickSortArray(suffArray, start, end);
                //Quicksort(suffArray, start, end + 1);
                //RadixSort(suffArray, start, end);
                Console.WriteLine($"end");
            }
            Console.WriteLine("HOTOVO");
            // a jeste N
            Console.WriteLine($"KONEC");
            //QuickSortArray(suffArray, 1, suffArray.Length - 1);
            Console.WriteLine("HOTOVO");
            
            _suffixArray = suffArray;

            if (save)
                SaveToFile("./output");
        }
        public SuffixArray(string filePathGenom, string filePath)
        {
            Console.WriteLine("Nacitam ze souboru...");
            LoadGenom(filePathGenom);

            File.WriteAllText("./genom.txt", genom);

            LoadFromFile(filePath);
            Console.WriteLine("Hotovo");
        }

        private void LoadGenom(string filePath)
        {
            genom = File.ReadAllText(filePath);

            genom = genom.Substring(6); // ignore prvni radek
            genom = genom.Replace("\n", "").Replace("\r", "").ToUpper();
            //genom = new string(genom.Where(c => c == 'A' || c == 'C' || c == 'G' || c == 'T').ToArray());
            //genom = genom.Substring(genom.Length / 100 * 99); pro zkraceni
            genom += "$";
        }

        public List<int> Find(string toFind)
        {
            List<int> result = new List<int>();


            int index = Find(_suffixArray, 0, _suffixArray.Length - 1, toFind);

            if (index == -1) return result;

            result.Add(index);
            int l, r;
            l = r = index;
            while(l >= 0)
            {
                if (BinaryCompare(genom, l, toFind) == 0)
                {
                    if (!result.Contains(l))
                        result.Add(l);
                }
                l--;
            }
            while (r < genom.Length)
            {
                if (BinaryCompare(genom, r, toFind) == 0)
                {
                    if (!result.Contains(r))
                        result.Add(r);
                }
                r++;
            }

            return result;
        }
        private int Find(int[] arr, int l, int r, string toFind)
        {
            if (r >= l)
            {
                int mid = l + (r - l) / 2;

                int result = SuffixArray.BinaryCompare(genom, arr[mid], toFind);
                if (result == 0)
                    return arr[mid];

                if (result == 1)
                    return Find(arr, l, mid - 1, toFind);

                return Find(arr, mid + 1, r, toFind);
            }

            return -1;
        }
        public void RadixSort(string[] arr, int n)
        {
            for (int i = 0; i < n; i++)
            {
                RadixSortStep(arr, i);
            }
        }
        public void RadixSort(int[] arr, int startIndex, int endIndex)
        {
            /*
            string[] array = new string[endIndex - startIndex + 1];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = GetString(arr[i + startIndex]);
            }*/
            int longest = 0; //82920205
            for (int i = startIndex; i <= endIndex; i++)
            {
                if (genom.Length - arr[i] > longest)
                {
                    longest = genom.Length - arr[i];
                }
            }
            for (int i = 0; i < longest; i++)
            {
                //if (RadixSortStep(arr, i, startIndex, endIndex)) return;
                RadixSortStep(arr, i, startIndex, endIndex);
            }
        }
        private void RadixSortStep(string[] toSort, int index)
        {
            int[] count = new int[6];

            // spocitej vyskyty
            for (int i = toSort.Length - 1; i >= 0; i--)
            {
                if (toSort[i].Length < index + 1)
                {
                    count[0]++;
                    continue;
                }

                int indexFromEnd = toSort[i].Length - index - 1;

                char evaluatedChar = toSort[i][indexFromEnd];
                count[CharToInt(evaluatedChar)]++;
            }

            //uprav  offset indexy podle algoritmu
            for (int i = 1; i < count.Length; i++)
            {
                count[i] += count[i-1];
            }

            string[] output = new string[toSort.Length];
            
            // vytvor vysledek
            for (int i = toSort.Length - 1; i >= 0; i--)
            {
                if (toSort[i].Length < index + 1)
                {
                    output[count[0] - 1] = toSort[i];
                    count[0]--;
                    continue;
                }
                int indexFromEnd = toSort[i].Length - index - 1;
                char evaluatedChar = toSort[i][indexFromEnd];
                output[count[CharToInt(evaluatedChar)] - 1] = toSort[i];
                count[CharToInt(evaluatedChar)]--;
            }

            // dewj vysledky do input pole
            for (int i = 0; i < toSort.Length; i++)
            {
                toSort[i] = output[i];
            }
        }
        
        private bool RadixSortStep(int[] toSort, int index, int startIndex, int endIndex)
        {
            int[] count = new int[6];

            // spocitej vyskyty
            for (int i = endIndex; i >= startIndex; i--)
            {
                int sLength = genom.Length - toSort[i];
                if (sLength < index + 1)
                {
                    count[0]++;
                    continue;
                }

                int indexFromEnd = genom.Length - index - 1;

                char evaluatedChar = genom[indexFromEnd];
                count[CharToInt(evaluatedChar)]++;
            }

            //uprav  offset indexy podle algoritmu
            for (int i = 1; i < count.Length; i++)
            {
                count[i] += count[i - 1];
            }

            int[] output = new int[endIndex - startIndex + 1];

            // vytvor vysledek
            for (int i = endIndex; i >= startIndex; i--)
            {
                int sLength = genom.Length - toSort[i];

                if (sLength < index + 1)
                {
                    output[count[0] - 1] = toSort[i];
                    count[0]--;
                    continue;
                }
                int indexFromEnd = genom.Length - index - 1;
                char evaluatedChar = genom[indexFromEnd];
                output[count[CharToInt(evaluatedChar)] - 1] = toSort[i];
                count[CharToInt(evaluatedChar)]--;
            }
            bool shouldEnd = true;
            // dewj vysledky do input pole
            for (int i = endIndex; i >= startIndex; i--)
            {
                if (toSort[i] != output[endIndex - i])
                {
                    shouldEnd = false;
                }
                toSort[i] = output[endIndex - i];
            }
            return shouldEnd;
        }
        private int CharToInt(char c)
        {
            switch (c)
            {
                case '$': return 0;
                case 'A': return 1;
                case 'C': return 2;
                case 'G': return 3;
                case 'N': return 4;
                case 'T': return 5;
                default:
                    return -1;
            }
        }
        private int OffsetToInsert(string prefix)
        {
            int i = 0;
            int offset = 1;
            string oPrefix;
            do
            {
                oPrefix = OrderedPrefixes[i];

                offset += prefixDict[oPrefix];

                i++;
            } while (oPrefix != prefix);

            offset -= prefixDict[oPrefix];
            offset += prefixOffsetDict[oPrefix];
            prefixOffsetDict[oPrefix]++;

            return offset;
        }
        private int PrefixOffset(string prefix)
        {
            int i = 0;
            int offset = 1;
            string oPrefix;
            do
            {
                oPrefix = OrderedPrefixes[i];

                offset += prefixDict[oPrefix];

                i++;
            } while (oPrefix != prefix);

            offset -= prefixDict[oPrefix];

            return offset;
        }
        //pro test
        private void QuickSortArray(string[] array, int leftIndex, int rightIndex)
        {
            var i = leftIndex;
            var j = rightIndex;
            var pivot = array[leftIndex];
            while (i <= j)
            {
                int compareResult = string.Compare(array[i], pivot);
                while (compareResult < 0)
                {
                    i++;
                    compareResult = string.Compare(array[i], pivot);
                }

                compareResult = string.Compare(array[j], pivot);
                while (compareResult > 0)
                {
                    j--;
                    compareResult = string.Compare(array[j], pivot);
                }
                if (i <= j)
                {
                    string temp = array[i];
                    array[i] = array[j];
                    array[j] = temp;
                    i++;
                    j--;
                }
            }

            if (leftIndex < j)
                QuickSortArray(array, leftIndex, j);
            if (i < rightIndex)
                QuickSortArray(array, i, rightIndex);
        }
       
        private void QuickSortArray(int[] array, int leftIndex, int rightIndex)
        {
            var i = leftIndex;
            var j = rightIndex;
            //22041587
            /*[] array = new string[rightIndex - leftIndex];
            for (int k = 0; k < array.Length; k++)
            {

            }*/

            //string pivot = GetString(array[leftIndex]);
            int pivot = array[(rightIndex + leftIndex) / 2] ;
            //int pivot = array[rightIndex] ;

            while (i <= j)
            {
                int compareResult = ComprareString(genom, array[i], pivot);
                //int compareResult = string.Compare(GetString(array[i]), pivot);
                //int compareResult = -1;
                //int ll = 0;
                while (compareResult < 0)
                {
                    i++;
                    compareResult = ComprareString(genom, array[i], pivot);
                    //compareResult = -1;
                    //if (ll++ > 5000) compareResult = 1;
                }

                compareResult = ComprareString(genom, array[j], pivot);
                //compareResult = 1;
                //ll = 0;
                while (compareResult > 0)
                {
                    j--;
                    compareResult = ComprareString(genom, array[j], pivot);
                    //compareResult = 1;
                    //if (ll++ > 5000) compareResult = -1;
                }
                if (i <= j)
                {
                    int temp = array[i];
                    array[i] = array[j];
                    array[j] = temp;
                    i++;
                    j--;
                }
            }

            if (leftIndex < j)
            {
                QuickSortArray(array, leftIndex, j);
            }
            if (i < rightIndex)
            {
                QuickSortArray(array, i, rightIndex);
            }
        }
        public string GetString(int index)
        {
            return genom.Substring(index);
        }
        public string GetString(int startIndex, int lastIndex)
        {
            return genom.Substring(startIndex, lastIndex);
        }
        public char GetChar(int index)
        {
            return genom[index];
        }
        public char GetCharFromArrayIndex(int index)
        {
            return genom[_suffixArray[index]];
        }
        public static int ComprareString(string whole, int s1Start, int s1Length, int s2Start, int s2Length)
        {
            int index = 0;
            while(index < s1Length && index < s2Length)
            {
                if (whole[s1Start + index] < whole[s2Start + index]) return -1;
                if (whole[s1Start + index] > whole[s2Start + index]) return 1;
                index++;
            }

            if (s1Length < s2Length) return -1;
            if (s1Length > s2Length) return 1;

            return 0;
        }
        /// <summary>
        /// Delka je az do konce
        /// </summary>
        public static int ComprareString(string whole, int s1Start, int s2Start)
        {
            if (s1Start == s2Start) return 0;
            int s1Length = whole.Length - s1Start;
            int s2Length = whole.Length - s2Start;
            int index = 0;
            while (index < s1Length && index < s2Length)
            {
                if (whole[s1Start + index] < whole[s2Start + index]) return -1;
                if (whole[s1Start + index] > whole[s2Start + index]) return 1;
                index++;
            }

            if (s1Length < s2Length) return -1;
            if (s1Length > s2Length) return 1;

            return 0;
        }
        public static int BinaryCompare(string refWhole, int refStart, string s2)
        {
            int s1Length = refWhole.Length - refStart;
            int s2Length = s2.Length;

            int index = 0;
            while (index < s1Length && index < s2Length)
            {
                if (refWhole[refStart + index] < s2[index]) return -1;
                if (refWhole[refStart + index] > s2[index]) return 1;
                index++;
            }

            if (s1Length < s2Length) return 1;

            return 0;
        }
        public void SaveToFile(string file)
        {
            byte[] bytes = new byte[_suffixArray.Length * sizeof(int)];
            Buffer.BlockCopy(_suffixArray, 0, bytes, 0, bytes.Length);

            File.WriteAllBytes(file, bytes);
        }
        public void LoadFromFile(string file)
        {
            var bytes = File.ReadAllBytes("./output");
            int k = 0;
            _suffixArray = new int[bytes.Length / 4];
            for (int j = 0; j < bytes.Length; j += 4, k++)
            {
                _suffixArray[k] = BitConverter.ToInt32(bytes, j);
            }
        }
        public int ArraySize()
        {
            return _suffixArray.Length;
        }
        public void ReduceSize(int keepEvery)
        {
            for (int i = 0; i < ArraySize(); i++)
            {
                if (i % keepEvery != 0)
                    _suffixArray[i] = 0;
            }
        }
    }
}
