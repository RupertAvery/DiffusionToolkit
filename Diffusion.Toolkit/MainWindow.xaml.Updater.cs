using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Diffusion.Common;

namespace Diffusion.Toolkit
{
    public partial class MainWindow
    {

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            CallUpdater();
        }

        private void FileCopy(string source, string filename, string target)
        {
            File.Copy(Path.Combine(source, filename), Path.Combine(target, filename), true);
        }

        private void CallUpdater()
        {
            Logger.Log($"Calling updater...");
            try
            {
                var appDir = AppInfo.AppDir;

                var temp = Path.Combine(appDir, "Updater");

                if (!Directory.Exists(temp))
                {
                    Directory.CreateDirectory(temp);
                }

                FileCopy(appDir, "Diffusion.Updater.exe", temp);

                var pi = new ProcessStartInfo()
                {
                    FileName = Path.Combine(temp, "Diffusion.Updater.exe"),
                    Arguments = $"\"{appDir}\"",
                    UseShellExecute = true
                };


                Process.Start(pi);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Updater Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

    }
}