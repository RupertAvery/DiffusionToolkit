using Avalonia.Controls;
using Avalonia.Interactivity;
using Diffusion.Database;
using DiffusionToolkit.AvaloniaApp.Common;
using DiffusionToolkit.AvaloniaApp.ViewModels;
using System;
using System.IO;
using Avalonia;
using Avalonia.Threading;
using Diffusion.Common;

namespace DiffusionToolkit.AvaloniaApp
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _viewModel;
        private NavigationManager _navigationManager;
        private ScanManager _scanManager;

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            _configuration.Save(ServiceLocator.Settings);
        }

        private Configuration<Settings> _configuration;

        protected override async void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);

            var configFile = Path.Combine(AppInfo.AppDataPath, "settings-xp.json");

            _configuration = new Configuration<Settings>(configFile);

            Settings? settings = null;

            if (File.Exists(configFile))
            {
                _configuration.Load(out settings);
            }
            else
            {
                settings = new Settings();
            }

            ServiceLocator.SetSettings(settings);

            // DataStore needs to be created and set before any of the controls/pages are created because of the
            // ServiceLocator pattern

            var appPath = AppInfo.AppDir;

            var databasePath = Path.Combine(AppInfo.AppDataPath, "diffusion-toolkit.db");
            var extensionsPath = Path.Combine(AppInfo.AppDir, "extensions");
            var altExtensionsPath = Path.Combine(AppInfo.AppDataPath, "extensions");

            Logger.Log($"App path: {appPath}");
            Logger.Log($"Database path: {databasePath}");

            var dataStore = new DataStore(databasePath, extensionsPath, altExtensionsPath);
            dataStore.BeforeMigrate += DataStoreOnBeforeMigrate;
            dataStore.AfterMigrate += DataStoreOnAfterMigrate;

            ServiceLocator.SetDataStore(dataStore);

            await dataStore.Create();

            _navigationManager = ServiceLocator.NavigationManager;
            _scanManager = ServiceLocator.ScanManager;

            _scanManager.ScanStart += OnScanStart;
            _scanManager.ScanProgress += OnScanProgress;
            _scanManager.ScanEnd += OnScanEnd;

            ServiceLocator.PreviewManager.SetOwner(this);

            _viewModel = new MainWindowViewModel();
            DataContext = _viewModel;
        }

        private void OnScanEnd(object? sender, EventArgs e)
        {
            Dispatcher.UIThread.Post(() => { _viewModel.IsBusy = false; });
        }

        private void OnScanStart(object? sender, EventArgs e)
        {
            Dispatcher.UIThread.Post(() => { _viewModel.IsBusy = true; });
        }

        private void OnScanProgress(object? sender, ScanProgressEventArgs e)
        {
            Dispatcher.UIThread.Post(() =>
            {
                _viewModel.CurrentProgress = e.Progress;
                _viewModel.TotalProgress = e.Total;
            });
        }

        private void DataStoreOnAfterMigrate(object? sender, MigrationEventArgs e)
        {
        }

        private void DataStoreOnBeforeMigrate(object? sender, MigrationEventArgs e)
        {
        }


  
        private void Control_OnSizeChanged(object? sender, SizeChangedEventArgs e)
        {

        }

        private void AvaloniaObject_OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property.Name == nameof(Window.WindowState))
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    Border mainBorder = MainBorder;
                    mainBorder.BorderThickness = new Thickness(10);
                }
                else if (this.WindowState == WindowState.Normal)
                {
                    Border mainBorder = MainBorder;
                    mainBorder.BorderThickness = new Thickness(0);
                }
            }
        }
    }
}