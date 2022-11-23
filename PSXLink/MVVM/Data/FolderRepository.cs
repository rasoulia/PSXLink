using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;

namespace PSXLink.MVVM.Data
{
    public static class FolderRepository
    {
        public static void CreateFolder()
        {
            string[] folderName =
            {
                @"Log\PS4\1. Game",
                @"Log\PS4\2. Update",
                @"Backup\PS4\",
                @"Settings"
            };

            foreach (string folder in folderName)
            {
                CreateFolder(folder);
            }
        }

        public static string CreateLogFolder(string title, int region, string titileID)
        {
            title = Regex.Replace(title, @"[^a-zA-Z0-9_ ]+", "", RegexOptions.Compiled);
            string folderName = $"Log\\PS4\\1. Game\\{title}\\{region}. {titileID}";
            CreateFolder(folderName);
            return folderName;
        }

        private static void CreateFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static void OpenFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                ProcessStartInfo startInfo = new()
                {
                    Arguments = folderPath,
                    FileName = "explorer.exe"
                };
                Process.Start(startInfo);
            }
            else
            {
                MessageBox.Show($"{folderPath} Directory does not exist!");
            }
        }
    }
}
