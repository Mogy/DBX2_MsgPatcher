# DBX2_MsgPatcher

Dragon Ball Xenoverse 2 MSG Tool By Delutto を使ってメッセージデータを作成するための補助ツール

実行には[.Net Core Runtime 3.1](https://www.ipentec.com/document/windows-install-dotnet-core-31-runtime)が必要

変換元のメッセージデータは各自で吸い出して用意する

# 必要なもの

* [Dragon Ball Xenoverse 2 MSG Tool By Delutto](https://zenhax.com/viewtopic.php?t=4052#p35491)
* PC版の英語メッセージデータ(*_en.msg)
* [DBX2_MsgExtractor](https://github.com/Mogy/DBX2_MsgExtractor)で作成した日本語訳一覧データ(jaMsg.txt)

# 作業手順

1. Dragon Ball Xenoverse 2 MSG Tool By Delutto をダウンロードして実行ファイルを同ディレクトリに置く
3. 日本語訳一覧データ(jaMsg.txt)を同ディレクトリに置く
2. PC版の英語メッセージデータ(*_en.msg)を「PcEnMsgフォルダ」に追加する
4. DBX2_MsgPatcher.exe を実行
5. 変換後のメッセージデータが「outputフォルダ」に出力される

変換に失敗したメッセージデータの一覧は「error.log」で確認できる

メッセージデータの反映は XV2Patcher を使うと簡単
