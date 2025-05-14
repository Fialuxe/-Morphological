// DocumentData.cs
using System.Collections.Generic;
using System.Linq;

namespace CsvTfIdfCalculator
{
    public class DocumentData
    {
        public string FilePath { get; }
        public List<LexemeEntry> Lexemes { get; }
        public Dictionary<int, int> TermCounts { get; } // 語彙素ID -> 文書内での出現回数
        public int TotalLexemesInDocument { get; }

        // TF-IDF計算結果を保持 (語彙素ID -> TF-IDF値)
        public Dictionary<int, double> TfIdfScores { get; set; }

        public DocumentData(string filePath, List<LexemeEntry> lexemes)
        {
            FilePath = filePath;
            Lexemes = lexemes ?? new List<LexemeEntry>();
            TermCounts = new Dictionary<int, int>();
            TfIdfScores = new Dictionary<int, double>();

            foreach (var lexeme in Lexemes)
            {
                if (TermCounts.ContainsKey(lexeme.LexemeId))
                {
                    TermCounts[lexeme.LexemeId]++;
                }
                else
                {
                    TermCounts[lexeme.LexemeId] = 1;
                }
            }
            TotalLexemesInDocument = Lexemes.Count;
        }
    }
}