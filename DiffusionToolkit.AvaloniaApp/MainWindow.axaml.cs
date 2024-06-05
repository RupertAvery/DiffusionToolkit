using Avalonia.Controls;
using Avalonia.Interactivity;
using Diffusion.Database;
using DiffusionToolkit.AvaloniaApp.Common;
using DiffusionToolkit.AvaloniaApp.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using Diffusion.Common;

namespace DiffusionToolkit.AvaloniaApp
{
    public static class AppInfo
    {
        private const string AppName = "DiffusionToolkit";
        public static string AppDir { get; }
        public static SemanticVersion Version => SemanticVersionHelper.GetLocalVersion();
        public static string AppDataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DiffusionToolkit");


        static AppInfo()
        {
            AppDir = AppDomain.CurrentDomain.BaseDirectory;

            if (AppDir.EndsWith("\\"))
            {
                AppDir = AppDir.Substring(0, AppDir.Length - 1);
            }
        }


    }

    public class Settings
    {
        public IEnumerable<string> IncludedFolders { get; set; }
        public IEnumerable<string> ExcludedFolders { get; set; }
        public int IconSize { get; set; } = 256;
        public bool HideNSFW { get; set; } = true;
    }

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

            var configFile = Path.Combine(AppInfo.AppDir, "settings.json");

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

            var path = Path.Combine(AppInfo.AppDataPath, "diffusion-toolkit.db");


            var dataStore = new DataStore(path);
            dataStore.BeforeMigrate += DataStoreOnBeforeMigrate;
            dataStore.AfterMigrate += DataStoreOnAfterMigrate;

            ServiceLocator.SetDataStore(dataStore);

            await dataStore.Create();

            _navigationManager = ServiceLocator.NavigationManager;
            _scanManager = ServiceLocator.ScanManager;

            _scanManager.ScanProgress += OnScanProgress;

            _viewModel = new MainWindowViewModel();
            DataContext = _viewModel;
        }

        private void DataStoreOnAfterMigrate(object? sender, MigrationEventArgs e)
        {
        }

        private void DataStoreOnBeforeMigrate(object? sender, MigrationEventArgs e)
        {
        }


        private void OnScanProgress(object? sender, ScanProgressEventArgs e)
        {

        }
    }
}