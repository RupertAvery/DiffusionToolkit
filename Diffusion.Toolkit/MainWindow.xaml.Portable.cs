using Diffusion.Common;
using Diffusion.Database;
using Diffusion.Toolkit.Services;
using System;
using System.IO;
using System.Windows;

namespace Diffusion.Toolkit
{
    public partial class MainWindow
    {

        private string AppDir
        {
            get
            {
                var appDir = AppDomain.CurrentDomain.BaseDirectory;

                if (appDir.EndsWith("\\"))
                {
                    appDir = appDir.Substring(0, appDir.Length - 1);
                }

                return appDir;
            }
        }

        private void GoPortable()
        {
            SwitchConfig("portable", AppInfo.AppDataPath, AppDir);
        }

        private void GoLocal()
        {
            SwitchConfig("application settings", AppDir, AppInfo.AppDataPath);
        }

        private void SwitchConfig(string target, string sourcePath, string targetPath)
        {
            string sourceSettingsPath = Path.Combine(sourcePath, "config.json");
            string sourceDbPath = Path.Combine(sourcePath, "diffusion-toolkit.db");

            string targetSettingsPath = Path.Combine(targetPath, "config.json");
            string targetDbPath = Path.Combine(targetPath, "diffusion-toolkit.db");


            if (!File.Exists(targetSettingsPath) && !File.Exists(targetDbPath))
            {
                File.Copy(sourceSettingsPath, targetSettingsPath);
                File.Copy(sourceDbPath, targetDbPath);

                File.Delete(sourceSettingsPath);
                File.Delete(sourceDbPath);
            }
            else
            {
                var existsDialogResult = MessageBox.Show(this, $"A configuration or database file was found in the {target} folder. Do you want to use it?", "Diffusion Toolkit", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel);

                if (existsDialogResult == MessageBoxResult.No)
                {
                    var confirmResult = MessageBox.Show(this, $"Are you sure you want to overwrite the files in the {target} folder?", "Diffusion Toolkit", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (confirmResult == MessageBoxResult.Yes)
                    {
                        if (File.Exists(sourceSettingsPath))
                        {
                            File.Copy(sourceSettingsPath, targetSettingsPath, true);
                        }
                        if (File.Exists(sourceDbPath))
                        {
                            File.Copy(sourceDbPath, targetDbPath, true);
                        }
                    }
                }

                if (existsDialogResult == MessageBoxResult.Cancel)
                {
                    return;
                }



                if (existsDialogResult == MessageBoxResult.Yes)
                {
                    if (target == "application settings")
                    {
                        // rename portable files so that DT doesn't try to load them on startup

                        var bSettingsPath = Path.Combine(sourcePath, "config.backup");
                        var bDbPath = Path.Combine(sourcePath, "diffusion-toolkit.backup");

                        var moved = false;

                        if (File.Exists(sourceSettingsPath))
                        {
                            File.Move(sourceSettingsPath, bSettingsPath, true);
                            moved = true;
                        }

                        if (File.Exists(sourceDbPath))
                        {
                            File.Move(sourceDbPath, bDbPath, true);
                            moved = true;
                        }

                        if (moved)
                        {
                            MessageBox.Show(this, "Your portable files have been renamed to .backup", "Diffusion Toolkit", MessageBoxButton.OK);
                        }
                    }

                }

            }



            _configuration = new Configuration<Settings>(targetSettingsPath, false);

            if (_configuration.TryLoad(out var settings))
            {
                TypeHelpers.Copy(settings, _settings);
                _settings.PortableMode = _configuration.Portable;
                _settings.SetPristine();
            }

            Logger.Log($"Opening database at {targetDbPath}");

            var dataStore = new DataStore(targetDbPath);

            ServiceLocator.SetDataStore(dataStore);
        }

    }
}
