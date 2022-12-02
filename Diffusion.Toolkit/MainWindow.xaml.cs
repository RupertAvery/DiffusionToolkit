using Diffusion.Common;
using Diffusion.Database;
using Diffusion.Toolkit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Path = System.IO.Path;
using Diffusion.Toolkit.Classes;
using Search = Diffusion.Toolkit.Pages.Search;

namespace Diffusion.Toolkit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainModel _model;
        private readonly DataStore _dataStore;
        private NavigatorService _navigatorService;
        private string AppDataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DiffusionToolkit");
        private Configuration<Settings> _configuration = new("DiffusionToolkit");
        private Settings? _settings;

        private Search _search;

        public MainWindow()
        {
            InitializeComponent();

            _navigatorService = new NavigatorService(this)
            {
                OnNavigate = OnNavigate
            };


            _dataStore = new DataStore(Path.Combine(AppDataPath, "diffusion-toolkit.db"));

            _model = new MainModel();
            _model.Rescan = new RelayCommand<object>(Rescan);


            // Ugh, need to make settings more... deterministic

            if (!_configuration.TryLoad(out _settings) || _settings.ImagePaths.Count == 0)
            {
                this.Loaded += (sender, args) =>
                {
                    _settings = new Settings();
                    var settings = new SettingsWindow(_dataStore, _settings);
                    settings.Owner = this;
                    settings.ShowDialog();

                    if (_settings.IsDirty)
                    {
                        _configuration.Save(_settings);
                        _settings.SetPristine();
                    }

                    _search.Settings = _settings;

                    if (_settings.ImagePaths.Any())
                    {
                        _search.Scan();
                    }
                };
            }

            if (_settings.FileExtensions == null ||
                _settings.FileExtensions.Length
                == 0)
            {
                _settings.FileExtensions = ".png, .jpg";
            }

            _search = new Search(_navigatorService, _dataStore, _settings);


            _model.Settings = new RelayCommand<object>(o =>
            {
                var settings = new SettingsWindow(_dataStore, _settings);
                settings.Owner = this;
                settings.ShowDialog();

                if (_settings.IsDirty)
                {
                    _configuration.Save(_settings);

                    _search.Settings = _settings;

                    if (_settings.IsPropertyDirty(nameof(Settings.PageSize)))
                    {
                        _search.SearchImages();
                    }

                    _settings.SetPristine();
                }
            });

            _model.Close = new RelayCommand<object>(o =>
            {
                this.Close();
            });


            DataContext = _model;

            var pages = new Dictionary<string, Page>()
            {
                { "search", _search },
                //{ "config", _configPage},
                //{ "setup", new SetupPage(_navigatorService) },
            };

            _navigatorService.SetPages(pages);

            _navigatorService.Goto("search");

        }

        private void Rescan(object obj)
        {
            if (_settings.ImagePaths.Any())
            {
                _search.Scan();
            }
            else
            {
                MessageBox.Show("No image paths configured!", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OnNavigate(Page page)
        {
            _model.Page = page;
        }
    }
}
