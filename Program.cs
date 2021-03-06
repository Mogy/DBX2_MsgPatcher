﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace DBX2_MsgPatcher
{
    class Program
    {
        const string DIR_EN_MSG = "PcEnMsg";
        const string DIR_OUTPUT = "output\\data\\msg";
        const string DIR_TEMP = "tmp";
        const string MSG_TOOL = "Dragon_Ball_Xenoverse_2_MSG_Tool.exe";
        const string EN_MSG = "enMsg.txt";
        const string JA_MSG = "jaMsg.txt";
        const string ERROR = "error.log";
        static void Main(string[] args)
        {
            var fromProcess = args.Length > 0;
            var dirEnMsg = DIR_EN_MSG;
            var dirOutput = DIR_OUTPUT;

            if (args.Length == 2)
            {
                dirEnMsg = args[0];
                dirOutput = args[1];
            }

            // フォルダ生成
            Directory.CreateDirectory(dirEnMsg);
            Directory.CreateDirectory(dirOutput);

            // msgTool存在チェック
            if (!File.Exists(MSG_TOOL))
            {
                Console.WriteLine($"{MSG_TOOL} is not found.");
                if (!fromProcess)
                {
                    Console.WriteLine();
                    Console.WriteLine("-- please push any key --");
                    Console.ReadKey();
                }
                Environment.Exit(1);
            }

            // jaMsg存在チェック
            if (!File.Exists(JA_MSG))
            {
                Console.WriteLine($"{JA_MSG} is not found.");
                if (!fromProcess)
                {
                    Console.WriteLine();
                    Console.WriteLine("-- please push any key --");
                    Console.ReadKey();
                }
                Environment.Exit(1);
            }

            // tempフォルダを再生成
            if (Directory.Exists(DIR_TEMP))
            {
                Directory.Delete(DIR_TEMP, true);
                // 実行完了待ち
                while (Directory.Exists(DIR_TEMP))
                {
                    Thread.Sleep(1);
                }
            }
            Directory.CreateDirectory(DIR_TEMP);

            // errorログ削除
            if (File.Exists(ERROR))
            {
                File.Delete(ERROR);
            }

            // msgTool設定
            var msgTool = new ProcessStartInfo();
            msgTool.FileName = MSG_TOOL;
            msgTool.CreateNoWindow = true;
            msgTool.UseShellExecute = false;

            createImportFiles(EN_MSG);
            createImportFiles(JA_MSG);

            var txtFiles = Directory.GetFiles(DIR_TEMP, "*_ja.txt");
            var currents = 1;
            var errors = 0;
            var hasEnMsg = File.Exists(EN_MSG);

            foreach (string txtPath in txtFiles)
            {
                Console.SetCursorPosition(0, 0);
                Console.WriteLine($"{currents++}/{txtFiles.Length}");

                // ファイルサイズチェック
                if (new FileInfo(txtPath).Length == 3) continue;

                // 英語メッセージ存在チェック
                var jaMsg = Path.GetFileName(Path.ChangeExtension(txtPath, "msg"));
                var enMsg = jaMsg.Replace("_ja", "_en");
                var enPath = Path.Combine(dirEnMsg, enMsg);
                if (!File.Exists(enPath))
                {
                    continue;
                }

                // メッセージ更新(旧Ver対応)
                if (hasEnMsg) {
                    var path = Path.ChangeExtension(enPath, "txt");
                    // msgTool実行(Export)
                    msgTool.Arguments = createArguments("-e", enPath, path);
                    Process.Start(msgTool).WaitForExit();

                    updateMessage(path, txtPath);
                }

                // msgTool実行(Import)
                msgTool.Arguments = createArguments("-i", enPath, txtPath);
                Process.Start(msgTool).WaitForExit();

                // 出力ファイルを移動
                var newPath = Path.Combine(dirEnMsg, enMsg + ".NEW");
                var resultPath = Path.Combine(dirOutput, enMsg);
                if (File.Exists(newPath))
                {
                    File.Copy(newPath, resultPath, true);
                    File.Delete(newPath);
                }
                else
                {
                    errors++;
                    // エラーログ出力
                    using (var sw = new StreamWriter(ERROR, true))
                    {
                        sw.WriteLine(enMsg);
                    }
                }
            }

            // tempフォルダを削除
            Directory.Delete(DIR_TEMP, true);

            Console.SetCursorPosition(0, 0);
            Console.WriteLine($"{txtFiles.Length - errors} files created.");
            if (errors > 0)
            {
                Console.WriteLine($"{errors} files failed to create.");
            }
            if (!fromProcess)
            {
                Console.WriteLine();
                Console.WriteLine("-- please push any key --");
                Console.ReadKey();
            }
        }

        static void updateMessage(string path, string jaPath)
        {
            var enPath = jaPath.Replace("_ja", "_en");
            var dic = new List<(string en, string ja)>();

            // 辞書データを作成
            using (var enSr = new StreamReader(enPath))
            using (var jaSr = new StreamReader(jaPath))
            {
                while (enSr.Peek() > -1 || jaSr.Peek() > -1)
                {
                    var en = enSr.ReadLine();
                    var ja = jaSr.ReadLine();
                    dic.Add((en, ja));
                }
            }

            // 日本語訳を更新
            using (var sr = new StreamReader(path))
            using (var sw = new StreamWriter(jaPath, false, Encoding.UTF8))
            {
                while (sr.Peek() > -1)
                {
                    var line = sr.ReadLine();
                    var result = "";

                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        var trans = dic.Find(d => d.en.Contains(line));

                        if (trans.ja == null)
                        {
                            // 辞書に無い時は英訳を使う
                            result = "▲" + line;
                        }
                        else
                        {
                            result = trans.ja;
                            dic.Remove(trans);
                        }
                    }

                    sw.WriteLine(result);
                }
            }
        }

        static void createImportFiles(string fileName)
        {
            if (!File.Exists(fileName)) return;

            Console.WriteLine("create import files...");
            StreamWriter sw = null;
            using (var sr = new StreamReader(fileName))
            {
                while (sr.Peek() > -1)
                {
                    var line = sr.ReadLine();
                    if (line.StartsWith("■■■"))
                    {
                        if (sw != null)
                        {
                            sw.Close();
                            sw = null;
                        }
                        var msg = Path.GetFileName(line.Replace("■■■", ""));
                        var txtPath = Path.Combine(DIR_TEMP, Path.ChangeExtension(msg, "txt"));
                        sw = new StreamWriter(txtPath, true, Encoding.UTF8);
                        continue;
                    }

                    if (sw == null) continue;

                    sw.WriteLine(line);
                }
                if (sw != null)
                {
                    sw.Close();
                    sw = null;
                }
            }
            Console.Clear();
        }

        private static string createArguments(params string[] args)
        {
            return string.Join(" ", args.Select(x => '"' + x + '"'));
        }
    }
}
