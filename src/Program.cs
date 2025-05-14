// Program.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CsvTfIdfCalculator
{
    class Program
    {
        public static int DISPLAY_NUM = 200000;
        static void Main(string[] args)
        {
            Console.WriteLine("CSVファイル群からのTF-IDF計算プログラム");
            Console.WriteLine("------------------------------------");

            Console.Write("CSVファイルが格納されているフォルダのフルパスを入力してください: ");
            string folderPath = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            {
                Console.Error.WriteLine("エラー: 無効なフォルダパスです。プログラムを終了します。");
                return;
            }

            var csvProcessor = new CsvProcessor();
            List<DocumentData> documents = csvProcessor.LoadDocumentsFromFolder(folderPath);

            if (documents == null || !documents.Any())
            {
                Console.Error.WriteLine("処理対象のドキュメントが見つかりませんでした。プログラムを終了します。");
                return;
            }

            var analyzer = new TextAnalyzer(documents);

            // 1. 語彙素IDごとの総出現回数をカウントし表示
            DisplayGlobalFrequencies(analyzer);

            // 2. TF-IDFを計算
            analyzer.CalculateTfIdf();

            // 3. TF-IDFの結果を表示
            DisplayTfIdfResults(documents, analyzer);

            Console.WriteLine("\n処理が完了しました。何かキーを押すと終了します。");
            Console.ReadKey();
        }

        static void DisplayGlobalFrequencies(TextAnalyzer analyzer)
        {
            var globalFrequencies = analyzer.GetGlobalLexemeFrequencies();
            if (!globalFrequencies.Any())
            {
                Console.Error.WriteLine("\n語彙素の出現頻度データがありません。");
                return;
            }

            Console.WriteLine("\n--- 全体での語彙素出現回数 ---");
            var sortedFrequencies = globalFrequencies.OrderByDescending(kvp => kvp.Value);

            Console.WriteLine("\n語彙素ID | 語彙素 (代表)      | 出現回数");
            Console.WriteLine("------------------------------------------");
            foreach (var pair in sortedFrequencies.Take(DISPLAY_NUM)) // 上位200件のみ表示 (多すぎる場合のため)
            {
                string lemma = analyzer.GetLemmaForId(pair.Key);
                Console.WriteLine($"{pair.Key,-8} | {lemma,-20} | {pair.Value}");
            }
            if (sortedFrequencies.Count() > DISPLAY_NUM)
            {
                Console.WriteLine($"... (他 {sortedFrequencies.Count() - DISPLAY_NUM} 件)");
            }
        }

        static void DisplayTfIdfResults(List<DocumentData> documents, TextAnalyzer analyzer)
        {
            Console.WriteLine("\n--- TF-IDF 計算結果 ---");
            if (!documents.Any(doc => doc.TfIdfScores.Any()))
            {
                Console.Error.WriteLine("TF-IDFスコアが計算されていません。");
                return;
            }

            foreach (var doc in documents)
            {
                Console.WriteLine($"\nドキュメント: {doc.FilePath} (総語彙素数: {doc.TotalLexemesInDocument})");
                if (!doc.TfIdfScores.Any())
                {
                    Console.Error.WriteLine("  このドキュメントにはTF-IDFスコアが計算された語彙素がありません。");
                    continue;
                }
                var sortedTfIdf = doc.TfIdfScores.OrderByDescending(kvp => kvp.Value);
                Console.WriteLine("  語彙素ID | 語彙素 (代表)      |   TF   |  Count  | TF-IDF");
                Console.WriteLine("  -----------------------------------------------------------");
                foreach (var pair in sortedTfIdf.Take(DISPLAY_NUM))
                {
                    int lexemeId = pair.Key;
                    double tfIdfScore = pair.Value;
                    int countInDoc = doc.TermCounts.TryGetValue(lexemeId, out int c) ? c : 0;
                    double tf = doc.TotalLexemesInDocument > 0 ? (double)countInDoc / doc.TotalLexemesInDocument : 0;
                    string lemma = analyzer.GetLemmaForId(lexemeId);

                    Console.WriteLine($"  {lexemeId,-8} | {lemma,-20} | {tf,6:F4} | {countInDoc,5}   | {tfIdfScore,6:F4}");
                }
                 if (sortedTfIdf.Count() > DISPLAY_NUM)
                {
                    Console.WriteLine($"  ... (他 {sortedTfIdf.Count() - DISPLAY_NUM} 件)");
                }
            }
        }
    }
}