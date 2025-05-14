// LexemeEntry.cs
using System;

namespace CsvTfIdfCalculator
{
    public class LexemeEntry
    {
        public string DictionarySource { get; set; }
        public string SentenceBoundary { get; set; }
        public string SurfaceForm { get; set; } // 書字形
        public string Lemma { get; set; }       // 語彙素
        public string LemmaReading { get; set; }
        public string PartOfSpeech { get; set; }
        public string BasicForm { get; set; }   // 語形(基本形)
        public int LexemeId { get; set; }

        public static LexemeEntry FromCsvLine(string csvLine, int lineNumber, string filePath)
        {
            if (string.IsNullOrWhiteSpace(csvLine))
            {
                Console.WriteLine($"警告: {filePath} の {lineNumber} 行目は空です。スキップします。");
                return null;
            }

            string[] parts = csvLine.Split(',');
            if (parts.Length < 8) // 期待される列数
            {
                Console.WriteLine($"警告: {filePath} の {lineNumber} 行目の列数が不足しています ({parts.Length}列)。スキップします。期待される列数: 8以上");
                return null;
            }

            if (!int.TryParse(parts[7].Trim(), out int lexemeId))
            {
                Console.WriteLine($"警告: {filePath} の {lineNumber} 行目の語彙素ID '{parts[7]}' が不正です。スキップします。");
                return null;
            }

            return new LexemeEntry
            {
                DictionarySource = parts[0].Trim(),
                SentenceBoundary = parts[1].Trim(),
                SurfaceForm = parts[2].Trim(),
                Lemma = parts[3].Trim(),
                LemmaReading = parts[4].Trim(),
                PartOfSpeech = parts[5].Trim(),
                BasicForm = parts[6].Trim(),
                LexemeId = lexemeId
            };
        }

        public override string ToString()
        {
            return $"ID: {LexemeId}, Lemma: {Lemma}, Surface: {SurfaceForm}, PoS: {PartOfSpeech}";
        }
    }
}