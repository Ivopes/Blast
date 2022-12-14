using Blast;
using SuffixArrayBIO;

//var find = "TAAAAGGCTTCAGAGAAAAGTTCAGGCTCAGGAGAAATCTCTGGAAACAGGAAAAATGCCACAGAAGGAACCCTTGGCAGTAAGCCTGGCAGGGAGGAGGGGACAGTGAAAAGGATGACCTCCAGGGAAGTGGTTGGTGGGGGGTGTGCTCTTTAGGCAGGAAGGTGGGGAGTAGCCCCATGCCCCAGCCCGGGCCTCTCCGGGCATGTTTTGGGGGAAGGAAGGAAGGAAGGTCAGAGCTGGTGAGTCAATGGCACAGGCTGTGCCTGAGGCAGTGAACTCACTCACTTGGGTAGCTTGTCATCCCCTCAACCCTGGGGTAAGGGAGGGGCAGGAGGGCTGTGGGGGAACAACTGTGGGCTGGGCTGGACCTGGGCAGAGGAGCTTCAACCCTCAGGGAGTGAGAACAGCAACCTCTT";
//var find = "YANCLEHKMGS";
var find = "GGGGACCCACACGTCT";
//var arr = new SuffixArrayAcid("../../../files/chr17.fa", true);
    
var b = new BlastSearchDna(find, 11, 46);

b.StartSearch(out var s, out var scores, out var positions);
Console.WriteLine("Hledam: " + find);
for (int i = 0; i < s.Count; i++)
{
    Console.WriteLine($"Pos: {positions[i]}\n\tRef: {s[i]}\n\tScore: {scores[i]}");
}