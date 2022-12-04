using Diffusion.Common;
using Diffusion.Database;
using Diffusion.Toolkit.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Path = System.IO.Path;
using Diffusion.Toolkit.Classes;
using Search = Diffusion.Toolkit.Pages.Search;
using Diffusion.Toolkit.Thumbnails;

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

            QueryBuilder.Samplers = File.ReadAllLines("samplers.txt").ToList();

            ThumbnailLoader.CreateInstance(Dispatcher);

            _navigatorService = new NavigatorService(this)
            {
                OnNavigate = OnNavigate
            };


            _dataStore = new DataStore(Path.Combine(AppDataPath, "diffusion-toolkit.db"));

            _model = new MainModel();
            _model.Rescan = new RelayCommand<object>(Rescan);
            _model.Rebuild = new RelayCommand<object>(Rebuild);
            _model.RemoveMarked = new RelayCommand<object>(RemoveMarked);
            _model.PropertyChanged += ModelOnPropertyChanged;

            // Ugh, need to make settings more... deterministic
            this.Loaded += OnLoaded;


            _settings = new Settings();
            _search = new Search(_navigatorService, _dataStore, _settings);

            _model.ShowFavorite = new RelayCommand<object>((o) => _search.ShowFavorite());
            _model.ShowMarked = new RelayCommand<object>((o) => _search.ShowMarked());

            _model.Settings = new RelayCommand<object>(ShowSettings);

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

        private void RemoveMarked(object obj)
        {
            if (MessageBox.Show(this, "This will delete the images marked for deletion! Are you sure you want to continue?", "Delete images", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                var files = _dataStore.GetMarkedImagePaths().ToList();
                var count = 0;
                foreach (var imagePath in files)
                {
                    try
                    {
                        File.Delete(imagePath.Path);
                        _dataStore.DeleteImage(imagePath.Id);
                        count++;
                    }
                    catch (Exception e)
                    {
                        if (MessageBox.Show(this, $"Failed to delete {imagePath.Path}", "Error", MessageBoxButton.OKCancel, MessageBoxImage.Error) == MessageBoxResult.Cancel)
                        {
                            break;
                        } 
                    }
                }
                MessageBox.Show($"{count} images were deleted", "Delete images", MessageBoxButton.OK, MessageBoxImage.Information);

                _search.ReloadMatches();
            }   
        }

        private void ModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainModel.ShowIcons))
            {
                _search.SetOpacityView(_model.ShowIcons);
            }
            else if (e.PropertyName == nameof(MainModel.HideIcons))
            {
                _search.SetIconVisibility(_model.HideIcons);
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!_configuration.TryLoad(out _settings))
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

                ThumbnailCache.CreateInstance(_settings.PageSize * 5, _settings.PageSize * 2);

                _search.Settings = _settings;

                if (_settings.ImagePaths.Any())
                {
                    _search.Scan();
                }
            }
            else
            {
                ThumbnailCache.CreateInstance(_settings.PageSize * 5, _settings.PageSize * 2);

                if (_settings.FileExtensions == null ||
                    _settings.FileExtensions.Length
                    == 0)
                {
                    _settings.FileExtensions = ".png, .jpg";
                }

                _search.Settings = _settings;
            }

        }

        private void ShowSettings(object obj)
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
                    ThumbnailCache.CreateInstance(_settings.PageSize * 5, _settings.PageSize * 2);
                    _search.SearchImages();
                }

                _settings.SetPristine();

            }
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


        private void Rebuild(object obj)
        {
            if (_settings.ImagePaths.Any())
            {
                _search.Rebuild();
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
