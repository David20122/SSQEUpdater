using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SSQEUpdater
{
    class Program
    {

        public static string currentEditorVersion;
        public static string downloadedVersionString;

        static void Main(string[] args)
        {

            if (File.Exists("Sound Space Quantum Editor.exe"))
            {
                var versionInfo = FileVersionInfo.GetVersionInfo("Sound Space Quantum Editor.exe");
                currentEditorVersion = versionInfo.FileVersion;
            }

            WebClient wc = new WebClient();

            Uri result = null;
            
            try
            {
                string reply = wc.DownloadString("https://raw.githubusercontent.com/David20122/SSQEUpdater/main/version");
                string trimmedReply = reply.TrimEnd();
                downloadedVersionString = trimmedReply;

            } catch
            {
                Write("!", "Failed to create download link");
            }

            string newVersionString = downloadedVersionString.TrimEnd();
            string path = Directory.GetCurrentDirectory();

            if (currentEditorVersion == newVersionString)
            {
                Write("!", "Editor is already up-to-date.");
                Thread.Sleep(1500);
                Environment.Exit(0);
            }


            if (Uri.TryCreate(new Uri("https://github.com/David20122/SSQEUpdater/releases/download/"), newVersionString + "/" + newVersionString + ".zip", out result))
            {
                Write("+", "link created " + result + "\n\n");
                DownloadFileInBackground(result, newVersionString);

                // rest happens in DownloadFileCallback

                var endlessTask = new TaskCompletionSource<bool>().Task;
                endlessTask.Wait();
            }

            void Write(string action, string text)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("\n[");
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write(action);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("] " + text);

            }

            void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e)
            {
                Console.Write("\rDownloading {0} % complete...", e.ProgressPercentage);
            }

            void DownloadFileInBackground(Uri address, string v)
            {

                wc.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadFileCallback);
                wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressCallback);

                wc.DownloadFileAsync(address, v + ".zip");
            }

            void DownloadFileCallback(object sender, AsyncCompletedEventArgs e)
            {
                Console.Write("\n");
                Write("+", "downloaded, extracting\n");
                string file = newVersionString + ".zip";
                ZipFile.ExtractToDirectory(file, path, true);

                Write("+", "editor updated, launching");
                Process.Start("Sound Space Quantum Editor");

                File.Delete(file);

                Thread.Sleep(1500);
                Environment.Exit(0);

                if (e.Error != null)
                {
                    Console.WriteLine(e.Error.ToString());
                }
            }
        }
    }
}

