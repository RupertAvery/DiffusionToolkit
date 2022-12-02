using Diffusion.Common;
using Diffusion.Database;
using Diffusion.IO;
using Diffusion.Toolkit.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Diffusion.Toolkit.Pages;
using static Dapper.SqlMapper;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;
using Path = System.IO.Path;
using Diffusion.Toolkit.Classes;

namespace Diffusion.Toolkit
{
    public class Settings
    {
        public Settings()
        {
            ImagePaths = new List<string>();
        }

        public List<string> ImagePaths { get; set; }
        public string ModelRootPath { get; set; }
    }

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
            _search = new Search(_navigatorService, _dataStore);


            _model = new MainModel();
            _model.Rescan = new RelayCommand<object>(Rescan);

            if (!_configuration.TryLoad(out _settings) || _settings.ImagePaths.Count == 0)
            {
                this.Loaded += (sender, args) =>
                {
                    _settings = new Settings();
                    var settings = new SettingsWindow(_dataStore, _settings);
                    settings.Owner = this;
                    settings.ShowDialog();
                    _configuration.Save(_settings);
                    if (_settings.ImagePaths.Any())
                    {
                        _search.Scan(_settings.ImagePaths);
                    }
                };
            }

            _model.Settings = new RelayCommand<object>(o =>
            {
                var settings = new SettingsWindow(_dataStore, _settings);
                settings.Owner = this;
                settings.ShowDialog();
                _configuration.Save(_settings);
            });

            _model.Close = new RelayCommand<object>(o =>
            {
                this.Close();
            });

            
            DataContext = _model;
            
            Loaded += OnLoaded;

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
                _search.Scan(_settings.ImagePaths);
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




       



        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            //Task.Run(async () => { });
        }



        //private static void BitmapOnDownloadFailed(object? sender, ExceptionEventArgs e)
        //{
        //    throw new NotImplementedException();
        //}

        //private static void BitmapOnDownloadProgress(object? sender, DownloadProgressEventArgs e)
        //{
        //    Debug.WriteLine(e.Progress);
        //}


        //public static async Task<BitmapImage> GetNewImageAsync(string path)
        //{
        //    BitmapImage bitmap = null;
        //    var httpClient = new HttpClient();

        //    using (var response = await httpClient.GetAsync(uri))
        //    {
        //        if (response.IsSuccessStatusCode)
        //        {
        //            using (var stream = new MemoryStream())
        //            {
        //                await response.Content.CopyToAsync(stream);
        //                stream.Seek(0, SeekOrigin.Begin);

        //                bitmap = new BitmapImage();
        //                bitmap.BeginInit();
        //                bitmap.CacheOption = BitmapCacheOption.OnLoad;
        //                bitmap.StreamSource = stream;
        //                bitmap.EndInit();
        //                bitmap.Freeze();
        //            }
        //        }
        //    }

        //    return bitmap;
        //}
    }
}
