namespace Bia;

public static class GenomUtils
{
    private static char[] nukleotids = new[] { 'A', 'C', 'G', 'T' };
    private static string start = "ATG";
    private static string[] end = new[] { "TAA", "TAG", "TGA" };

    private static Dictionary<string, string> aminoAcidsDict = new()
    {
        ["TTT"] = "F",
        ["TTC"] = "F",
        ["TTA"] = "L",
        ["TTG"] = "L",
        
        ["CTT"] = "L",
        ["CTC"] = "L",
        ["CTA"] = "L",
        ["CTG"] = "L",
        
        ["ATT"] = "I",
        ["ATC"] = "I",
        ["ATA"] = "I",
        ["ATG"] = "M",
        
        ["GTT"] = "V",
        ["GTC"] = "V",
        ["GTA"] = "V",
        ["GTG"] = "V",
        
        ["TCT"] = "S",
        ["TCC"] = "S",
        ["TCA"] = "S",
        ["TCG"] = "S",
        
        ["CCT"] = "P",
        ["CCC"] = "P",
        ["CCA"] = "P",
        ["CCG"] = "P",
        
        ["ACT"] = "T",
        ["ACC"] = "T",
        ["ACA"] = "T",
        ["ACG"] = "T",
        
        ["GCT"] = "A",
        ["GCC"] = "A",
        ["GCA"] = "A",
        ["GCG"] = "A",
        
        ["TAT"] = "Y",
        ["TAC"] = "Y",
        
        ["CAT"] = "H",
        ["CAC"] = "H",
        ["CAA"] = "Q",
        ["CAG"] = "Q",
        
        ["AAT"] = "N",
        ["AAC"] = "N",
        ["AAA"] = "K",
        ["AAG"] = "K",
        
        ["GAT"] = "D",
        ["GAC"] = "D",
        ["GAA"] = "E",
        ["GAG"] = "E",
        
        ["TGT"] = "C",
        ["TGC"] = "C",
        ["TGG"] = "W",
        
        ["CGT"] = "R",
        ["CGC"] = "R",
        ["CGA"] = "R",
        ["CGG"] = "R",
        
        ["AGT"] = "S",
        ["AGC"] = "S",
        ["AGA"] = "R",
        ["AGG"] = "R",
        
        ["GGT"] = "G",
        ["GGC"] = "G",
        ["GGA"] = "G",
        ["GGG"] = "G",
    };
    public static string MakeComplementary(string s, bool orderMirror = true)
    {
        string complementary = "";

        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            var nukIndex = nukleotids.Length  - 1 - Array.IndexOf(nukleotids, c);
            complementary += nukleotids[nukIndex];
        }

        return orderMirror ? complementary : complementary.Reverse().ToString()!;
    }

    public static List<string> GetAminoAcids(string[] genes)
    {
        List<string> acids = new();

        foreach (var gene in genes)
        {
            if (gene.Length % 3 != 0)
            {
                throw new Exception("Gene incorrect length");
            }
            acids.Add(GetAminoAcid(gene));
        }
        
        return acids;
    }
    public static string GetAminoAcids(string genom)
    {
        List<string> acids = new();

        var triplet = "";
        for (int i = 0; i < genom.Length; i++)
        {
            triplet += genom[i];

            if (triplet.Length == 3)
            {
                if (aminoAcidsDict.TryGetValue(triplet, out var val))
                {
                    acids.Add(aminoAcidsDict[triplet]);
                }
                triplet = "";
            }
        }
        return string.Join("", acids);
    }
    public static string GetAminoAcid(string gene)
    {
        if (gene.Length % 3 != 0)
        {
            throw new Exception("Gene incorrect length");
        }
        var aminoAc = "";
        var triplet = "";
        for (int i = 0; i < gene.Length; i++)
        {
            triplet += gene[i];

            if (triplet.Length == 3)
            {
                if (aminoAcidsDict.TryGetValue(triplet, out var val))
                {
                    aminoAc += aminoAcidsDict[triplet];
                }
                triplet = "";
            }
        }

        return aminoAc;
    }
    public static List<(int, int, string)> FindGenes(string genom)
    {
        List<(int, int, string)> genes = new();
        var triplet = "";

        for (int i = 0; i < genom.Length; i++)
        {
            if (genom[i] == '\n')
            {
                continue;
            }
            
            if (triplet.Length == 3)
            {
                triplet = triplet.Substring(1);
            }
            
            triplet += genom[i];

            if (triplet.Length == 3)
            {
                if (triplet == start)
                {
                    (int, int, string) g = FindGene(genom, i + 1);
                    if (!string.IsNullOrEmpty(g.Item3) && !string.IsNullOrWhiteSpace(g.Item3))
                    {
                        genes.Add(g);
                    }
                    else
                    {
                        Console.WriteLine("non ending gene found on the end");
                    }
                }
            }
        }

        return genes;
    }
    private static (int, int, string) FindGene(string genom, int offset)
    {
        var gene = "ATG";
        var triplet = "";

        for (int i = offset; i < genom.Length; i++)
        {
            if (genom[i] == '\n')
            {
                continue;
            }
            
            triplet += genom[i];

            if (triplet.Length == 3)
            {
                if (end.Contains(triplet))
                {
                    return (offset, i, gene);
                }

                gene += triplet;
                triplet = "";
            }
        }

        return (0, 0, "");
    }
}