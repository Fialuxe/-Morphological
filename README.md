# Morphological

Morphological is a simple program to calculate TF-IDF values from preprocessed CSV files containing morphological analysis results.

Morphologicalは、形態細解済みCSVファイルからTF-IDF値を算出するプログラムです。

出力は同一ディレクトリのout.txtのようなものになります。

# 🏦 Requirements / 必要環境

.NET 8 SDK (or above?)

Check if .NET is installed:
```
dotnet --version
```
If a version number like 8.0.401 is shown, you’re ready.   
バージョン番号（例：8.0.401）が表示されればOKです。   

If not, install it from the official .NET site.   
表示されない場合は、公式サイトからインストールしてください。   

# 📥 Setup / セットアップ手順

Download the source code   
ソースコードをダウンロードしてください   

Prepare CSV files   
以下のような形式のCSVファイルを準備してください：   
```
辞書,文境界,書字形（＝表層形）,語彙素,語彙素読み,品詞,語形(基本形),語彙素ID
,B,DeepSeek,,,名詞,,　
,I,（,（,,補助記号-括弧開,,46
,I,ディープ,ディープ-deep,ディープ,名詞-普通名詞-形状詞可能,ディープ,43838
...
```

You can generate such CSVs using morphological analysis tools such as:   
このようなCSVは以下のツールなどで生成できます：   

Web茶まめ（国立国語研究所）   

UniDic-MeCab Webアプリ   

MeCab公式サイト   

**⚠️ Note: Only properly formatted CSVs like the above will work. **   
__ ⚠️ 注意： 上記のような形式のCSVのみ対応しています。__

# ⚙️ How to Run / 実行方法

Move to the source directory:   
```
cd <path-to-Morphological>   
cd src
```
Build and run the project:
```
dotnet build
dotnet run
```
__ ⚠️ Notes / 注意点__   

You may see many warnings during build — that’s expected due to quick prototyping.   
ビルド時に警告が多く出るかもしれませんが、動作には支障ありません。私が見る限りですが。   

Currently supports Japanese text only.   
現時点では日本語テキストのみ対応しています。   
形態素idが登録されていない行は飛ばされるので、Trumpなどの英単語を含んでる場合などは工夫するか注意してご利用ください。   

# 📄 License / ライセンス

This project is released under the MIT License.MITライセンスのもとで公開されています。   

# 🤝 Contributions / コントリビューション

Feel free to fork and submit PRs!お気軽にFork・Pull Requestしてください。
