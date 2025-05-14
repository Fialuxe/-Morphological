// CsvProcessor.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CsvTfIdfCalculator
{
    public class CsvProcessor
    {
        public List<DocumentData> LoadDocumentsFromFolder(string folderPath)
        {
            var documents = new List<DocumentData>();
            if (!Directory.Exists(folderPath))
            {
                Console.Error.WriteLine($"エラー: 指定されたフォルダ '{folderPath}' が見つかりません。");
                return documents; // 空のリストを返す
            }

            string[] csvFiles = Directory.GetFiles(folderPath, "*.csv");
            if (csvFiles.Length == 0)
            {
                Console.Error.WriteLine($"エラー: 指定されたフォルダ '{folderPath}' にCSVファイルが見つかりません。");
                return documents; // 空のリストを返す
            }

            Console.WriteLine($"{csvFiles.Length} 個のCSVファイルを処理します...");

            foreach (var filePath in csvFiles)
            {
                try
                {
                    var lexemeEntries = new List<LexemeEntry>();
                    var lines = File.ReadAllLines(filePath);
                    
                    // ヘッダー行をスキップ (1行目)
                    for (int i = 1; i < lines.Length; i++)
                    {
                        var entry = LexemeEntry.FromCsvLine(lines[i], i + 1, Path.GetFileName(filePath));
                        if (entry != null)
                        {
                            lexemeEntries.Add(entry);
                        }
                    }

                    if (lexemeEntries.Any())
                    {
                        documents.Add(new DocumentData(Path.GetFileName(filePath), lexemeEntries));
                        Console.WriteLine($"処理完了: {Path.GetFileName(filePath)} ({lexemeEntries.Count} 語彙素)");
                    }
                    else
                    {
                        Console.Error.WriteLine($"警告: {Path.GetFileName(filePath)} から有効な語彙素が読み込めませんでした。");
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"エラー: ファイル '{Path.GetFileName(filePath)}' の処理中にエラーが発生しました。詳細: {ex.Message}");
                }
            }
            return documents;
        }
    }
}