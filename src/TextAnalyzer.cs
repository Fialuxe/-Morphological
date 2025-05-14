// TextAnalyzer.cs
using System;
using System.Collections.Generic;
using System.Linq;

namespace CsvTfIdfCalculator
{
    public class TextAnalyzer
    {
        private readonly List<DocumentData> _documents;
        private readonly Dictionary<int, string> _lexemeIdToLemmaMap; // 語彙素IDから代表的な語彙素文字列へのマップ（表示用）

        public TextAnalyzer(List<DocumentData> documents)
        {
            _documents = documents ?? throw new ArgumentNullException(nameof(documents));
            _lexemeIdToLemmaMap = new Dictionary<int, string>();
            PopulateLexemeMap();
        }

        private void PopulateLexemeMap()
        {
            foreach (var doc in _documents)
            {
                foreach (var lexeme in doc.Lexemes)
                {
                    if (!string.IsNullOrEmpty(lexeme.Lemma) && !_lexemeIdToLemmaMap.ContainsKey(lexeme.LexemeId))
                    {
                        _lexemeIdToLemmaMap[lexeme.LexemeId] = lexeme.Lemma;
                    }
                    // Lemma が空の場合でも、SurfaceForm を使うなど代替手段を検討できるが、今回はLemma優先
                    else if (string.IsNullOrEmpty(lexeme.Lemma) && 
                             !string.IsNullOrEmpty(lexeme.SurfaceForm) && 
                             !_lexemeIdToLemmaMap.ContainsKey(lexeme.LexemeId))
                    {
                         _lexemeIdToLemmaMap[lexeme.LexemeId] = lexeme.SurfaceForm; // 例: 、。など
                    }
                }
            }
        }
        
        public string GetLemmaForId(int lexemeId)
        {
            return _lexemeIdToLemmaMap.TryGetValue(lexemeId, out var lemma) ? lemma : "N/A";
        }

        public Dictionary<int, int> GetGlobalLexemeFrequencies()
        {
            var globalFrequencies = new Dictionary<int, int>();
            foreach (var doc in _documents)
            {
                foreach (var termCountPair in doc.TermCounts)
                {
                    if (globalFrequencies.ContainsKey(termCountPair.Key))
                    {
                        globalFrequencies[termCountPair.Key] += termCountPair.Value;
                    }
                    else
                    {
                        globalFrequencies[termCountPair.Key] = termCountPair.Value;
                    }
                }
            }
            return globalFrequencies;
        }

        public void CalculateTfIdf()
        {
            if (!_documents.Any()) return;

            int totalDocuments = _documents.Count;
            var documentFrequency = new Dictionary<int, int>(); // 語彙素ID -> それを含む文書数

            // 1. Document Frequency (DF) の計算
            foreach (var doc in _documents)
            {
                foreach (var lexemeId in doc.TermCounts.Keys)
                {
                    if (documentFrequency.ContainsKey(lexemeId))
                    {
                        documentFrequency[lexemeId]++;
                    }
                    else
                    {
                        documentFrequency[lexemeId] = 1;
                    }
                }
            }

            // 2. TF-IDF の計算
            foreach (var doc in _documents)
            {
                doc.TfIdfScores.Clear(); // 既存のスコアをクリア
                if (doc.TotalLexemesInDocument == 0) continue;

                foreach (var termCountPair in doc.TermCounts)
                {
                    int lexemeId = termCountPair.Key;
                    int termCountInDoc = termCountPair.Value;

                    // Term Frequency (TF)
                    double tf = (double)termCountInDoc / doc.TotalLexemesInDocument;

                    // Inverse Document Frequency (IDF)
                    // documentFrequency に lexemeId が存在しないケースは理論上DF計算でカバーされるが念のため
                    double idf = 0;
                    if (documentFrequency.TryGetValue(lexemeId, out int dfValue) && dfValue > 0)
                    {
                        // +1 はゼロ除算を避け、また全ての単語にわずかなIDFを与えるため。
                        // (もし全文書に出現する単語のIDFを0にしたい場合は +1 を除くか、 dfValue == totalDocuments の場合に特別処理)
                        // 一般的には log(N / df) もしくは log(N / (df + 1)) などが使われる
                        idf = Math.Log((double)totalDocuments / (dfValue +1.0)); // +1 を分母に加える
                         // もし Math.Log(totalDocuments / dfValue) を使う場合:
                         // idf = Math.Log((double)totalDocuments / dfValue);
                    }
                    
                    doc.TfIdfScores[lexemeId] = tf * idf;
                }
            }
        }
    }
}