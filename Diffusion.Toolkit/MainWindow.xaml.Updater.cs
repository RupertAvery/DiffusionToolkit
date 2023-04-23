using System.Diagnostics;
using System.IO;
using System.Windows;

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

            var appDir = AppInfo.AppDir;

            var temp = Path.Combine(appDir, "Updater");

            if (!Directory.Exists(temp))
            {
                Directory.CreateDirectory(temp);
            }

            FileCopy(appDir, "Diffusion.Updater.exe", temp);
            FileCopy(appDir, "Diffusion.Updater.deps.json", temp);
            FileCopy(appDir, "Diffusion.Updater.dll", temp);
            FileCopy(appDir, "Diffusion.Updater.runtimeconfig.json", temp);

            var pi = new ProcessStartInfo()
            {
                FileName = Path.Combine(temp, "Diffusion.Updater.exe"),
                Arguments = $"\"{appDir}\""
            };


            Process.Start(pi);
        }

    }
}