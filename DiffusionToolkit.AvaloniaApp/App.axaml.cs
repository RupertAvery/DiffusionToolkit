using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Diffusion.Database;
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
            var path = @"C:\Users\ruper\AppData\Roaming\DiffusionToolkit\diffusion-toolkit.db";

            var dataStore = new DataStore(path);



            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow(dataStore);
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}