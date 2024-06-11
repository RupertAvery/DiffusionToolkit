using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Diffusion.Common;
using Diffusion.Database;
using DiffusionToolkit.AvaloniaApp.Common;
using DiffusionToolkit.AvaloniaApp.Services;
using DiffusionToolkit.AvaloniaApp.ViewModels;

namespace DiffusionToolkit.AvaloniaApp
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            QueryBuilder.HideNSFW = true;

            var configuration = LoadSettings();


            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow(configuration);
            }
            base.OnFrameworkInitializationCompleted();
        }

        private Configuration<Settings> LoadSettings()
        {
            var configFile = Path.Combine(AppInfo.AppDataPath, "settings-xp.json");

            var configuration = new Configuration<Settings>(configFile);

            Settings? settings = null;

            if (File.Exists(configFile))
            {
                configuration.Load(out settings);
            }
            else
            {
                settings = new Settings();
            }

            this.RequestedThemeVariant = settings.Theme switch
            {
                "Dark" => ThemeVariant.Dark,
                "Light" => ThemeVariant.Light,
                "Default" => ThemeVariant.Default,
                _ => this.RequestedThemeVariant
            };

            ServiceLocator.SetSettings(settings);

            return configuration;
        }
    }
}