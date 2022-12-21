using Diffusion.Database;
using Diffusion.IO;
using Diffusion.Toolkit.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Diffusion.Toolkit.Thumbnails;
using File = System.IO.File;
using Path = System.IO.Path;
using Diffusion.Toolkit.Classes;
using Diffusion.Toolkit.Controls;
using Model = Diffusion.IO.Model;
using Task = System.Threading.Tasks.Task;
using System.Windows.Media;

namespace Diffusion.Toolkit.Pages
{
    public class ModeSettings
    {
        public ModeSettings()
        {
            History = new List<string?>();
        }

        public string LastQuery { get; set; }
        public List<string?> History { get; set; }
        public int LastPage { get; set; }
        public string ExtraQuery { get; set; }
        public string Name { get; set; }
    }

    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class Search : Page
    {
        private readonly SearchModel _model;
        private NavigatorService _navigatorService;
        private DataStore _dataStore;
        private Settings _settings;

        private ModeSettings _currentModeSettings;

        private ICollection<Model>? _modelLookup;

        public Search()
        {
            InitializeComponent();


            Task.Run(() =>
            {
                _ = ThumbnailLoader.Instance.StartRun();
            });



            //var str = new System.Text.StringBuilder();
            //using (var writer = new System.IO.StringWriter(str))
            //    System.Windows.Markup.XamlWriter.Save(MyContextMenu.Template, writer);
            //System.Diagnostics.Debug.Write(str);

        }


        private Random r = new Random();
        private readonly string[] _searchHints = File.ReadAllLines("hints.txt").Where(s => !string.IsNullOrEmpty(s.Trim())).ToArray();

        private void GetRandomHint()
        {
            var randomHint = _searchHints[r.Next(_searchHints.Length)];
            _model.SearchHint = $"Search for {randomHint}";
        }

        private Regex _gridLengthRegex = new Regex("Auto|(?<value>\\d+(?:\\.\\d+)?)(?<star>\\*)?");

        public GridLength GetGridLength(string? value)
        {
            if (string.IsNullOrEmpty(value)) return new GridLength(0, GridUnitType.Auto);

            if (value == "*") return new GridLength(0, GridUnitType.Star);

            var match = _gridLengthRegex.Match(value);

            if (match.Groups[0].Value == "Auto")
            {
                return new GridLength();
            }
            else if (match.Groups["star"].Success)
            {
                return new GridLength(double.Parse(match.Groups["value"].Value), GridUnitType.Star);
            }
            else
            {
                return new GridLength(double.Parse(match.Groups["value"].Value), GridUnitType.Pixel);
            }
        }

        public Search(NavigatorService navigatorService, DataStore dataStore, Settings settings) : this()
        {
            this._navigatorService = navigatorService;
            this._dataStore = dataStore;
            _settings = settings;

            navigatorService.Host.Closed += async (sender, args) =>
            {
                ThumbnailLoader.Instance.Stop();
            };

            _modeSettings = new Dictionary<string, ModeSettings>()
            {
                { "search", new ModeSettings() { Name="Diffusions", ExtraQuery = "" } },
                { "favorites", new ModeSettings() { Name="Favorites", ExtraQuery = "favorite: true" } },
                { "deleted", new ModeSettings() { Name="Recycle Bin", ExtraQuery = "delete: true" } },
            };

            if (_settings.MainGridWidth != null)
            {
                MainGrid.ColumnDefinitions[0].Width = GetGridLength(_settings.MainGridWidth);
                MainGrid.ColumnDefinitions[2].Width = GetGridLength(_settings.MainGridWidth2);
            }
            //if (_settings.PreviewGridHeight != null)
            //{
            //    PreviewGrid.RowDefinitions[0].Height = GetGridLength(_settings.PreviewGridHeight);
            //    PreviewGrid.RowDefinitions[2].Height = GetGridLength(_settings.PreviewGridHeight2);
            //}

            var widthDescriptor = DependencyPropertyDescriptor.FromProperty(ColumnDefinition.WidthProperty, typeof(ItemsControl));
            widthDescriptor.AddValueChanged(MainGrid.ColumnDefinitions[0], WidthChanged);
            widthDescriptor.AddValueChanged(MainGrid.ColumnDefinitions[2], WidthChanged2);

            //var heightDescriptor = DependencyPropertyDescriptor.FromProperty(RowDefinition.HeightProperty, typeof(ItemsControl));
            //heightDescriptor.AddValueChanged(PreviewGrid.RowDefinitions[0], HeightChanged);
            //heightDescriptor.AddValueChanged(PreviewGrid.RowDefinitions[2], HeightChanged2);

            _model = new SearchModel();


            _model.Page = 0;
            _model.Pages = 0;
            _model.TotalFiles = 100;

            _model.PropertyChanged += ModelOnPropertyChanged;
            _model.SearchCommand = new RelayCommand<object>(SearchImages);
            _model.CurrentImage.CopyPathCommand = new RelayCommand<object>(CopyPath);
            _model.CurrentImage.CopyPromptCommand = new RelayCommand<object>(CopyPrompt);
            _model.CurrentImage.CopyNegativePromptCommand = new RelayCommand<object>(CopyNegative);
            //_model.CurrentImage.CopySeed = new RelayCommand<object>(CopySeed);
            //_model.CurrentImage.CopyHash = new RelayCommand<object>(CopyHash);
            _model.CurrentImage.CopyParametersCommand = new RelayCommand<object>(CopyParameters);
            _model.CurrentImage.ShowInExplorerCommand = new RelayCommand<object>(ShowInExplorer);
            _model.CurrentImage.ShowInThumbnails = new RelayCommand<object>(ShowInThumbnails);
            _model.CurrentImage.DeleteCommand = new RelayCommand<object>(o => DeleteSelected());
            _model.CurrentImage.FavoriteCommand = new RelayCommand<object>(o => FavoriteSelected());

            _model.CopyPathCommand = new RelayCommand<object>(CopyPath);
            _model.CopyPromptCommand = new RelayCommand<object>(CopyPrompt);
            _model.CopyNegativePromptCommand = new RelayCommand<object>(CopyNegative);
            _model.CopySeedCommand = new RelayCommand<object>(CopySeed);
            _model.CopyHashCommand = new RelayCommand<object>(CopyHash);
            _model.CopyParametersCommand = new RelayCommand<object>(CopyParameters);
            _model.ShowInExplorerCommand = new RelayCommand<object>(ShowInExplorer);
            _model.DeleteCommand = new RelayCommand<object>(o => DeleteSelected());
            _model.FavoriteCommand = new RelayCommand<object>(o => FavoriteSelected());
            _model.RatingCommand = new RelayCommand<object>(o => RateSelected(int.Parse((string)o)));

            _model.NextPage = new RelayCommand<object>((o) => GoNextPage());
            _model.PrevPage = new RelayCommand<object>((o) => GoPrevPage());
            _model.FirstPage = new RelayCommand<object>((o) => GoFirstPage());
            _model.LastPage = new RelayCommand<object>((o) => GoLastPage());
            _model.Refresh = new RelayCommand<object>((o) => ReloadMatches());
            _model.ToggleParameters = new RelayCommand<object>((o) => ToggleInfo());
            _model.CopyFiles = new RelayCommand<object>((o) => CopyFiles());

            _model.FocusSearch = new RelayCommand<object>((o) => SearchTermTextBox.Focus());
            _model.ShowDropDown = new RelayCommand<object>((o) => SearchTermTextBox.IsDropDownOpen = true);
            _model.HideDropDown = new RelayCommand<object>((o) => SearchTermTextBox.IsDropDownOpen = false);
            SetMode("search");

            DataContext = _model;
        }

        private void CopyFiles()
        {
            //foreach (ImageEntry selectedItem in ThumbnailListView.SelectedItems)
            //{

            //}

            //DataObject dataObject = new DataObject();
            //dataObject.SetData(DataFormats.Bitmap, _selItems.Select(t => t.Path).ToArray());
            //DragDrop.DoDragDrop(source, dataObject, DragDropEffects.Copy);
        }

        private void ShowInThumbnails(object obj)
        {
            ThumbnailListView.ScrollIntoView(ThumbnailListView.SelectedItem);
            var index = ThumbnailListView.Items.IndexOf(ThumbnailListView.SelectedItem);
            if (ThumbnailListView.ItemContainerGenerator.ContainerFromIndex(index) is ListViewItem item)
            {
                item.Focus();
            }
        }


        private void WidthChanged(object? sender, EventArgs e)
        {
            _settings.MainGridWidth = MainGrid.ColumnDefinitions[0].Width.ToString();
        }

        private void HeightChanged(object? sender, EventArgs e)
        {
            _settings.PreviewGridHeight = PreviewGrid.RowDefinitions[0].Height.ToString();
        }


        private void WidthChanged2(object? sender, EventArgs e)
        {
            _settings.MainGridWidth2 = MainGrid.ColumnDefinitions[2].Width.ToString();
        }

        private void HeightChanged2(object? sender, EventArgs e)
        {
            _settings.PreviewGridHeight2 = PreviewGrid.RowDefinitions[2].Height.ToString();
        }

        public Settings Settings
        {
            get => _settings;
            set => _settings = value;
        }

        private void ShowInExplorer(object obj)
        {
            if (_model.CurrentImage == null) return;
            var p = _model.CurrentImage.Path;
            Process.Start("explorer.exe", $"/select,\"{p}\"");
        }


        private void CopyPath(object obj)
        {
            if (_model.CurrentImage?.Path == null) return;
            var p = _model.CurrentImage.Path;
            Clipboard.SetDataObject(p);
        }

        private void CopyPrompt(object obj)
        {
            if (_model.CurrentImage?.Prompt == null) return;
            var p = _model.CurrentImage.Prompt;
            Clipboard.SetDataObject(p);
        }

        private void CopyNegative(object obj)
        {
            if (_model.CurrentImage?.NegativePrompt == null) return;
            var p = _model.CurrentImage.NegativePrompt;
            Clipboard.SetDataObject(p);
        }

        private void CopySeed(object obj)
        {
            if (_model.CurrentImage?.Seed == null) return;
            var p = _model.CurrentImage.Seed.ToString();
            Clipboard.SetDataObject(p);
        }

        private void CopyHash(object obj)
        {
            if (_model.CurrentImage?.ModelHash == null) return;
            var p = _model.CurrentImage.ModelHash;
            Clipboard.SetDataObject(p);
        }

        private void CopyParameters(object obj)
        {
            if (_model.CurrentImage == null) return;

            var p = _model.CurrentImage.Prompt;
            var n = _model.CurrentImage.NegativePrompt;
            var o = _model.CurrentImage.OtherParameters;
            var parameters = $"{p}\r\n\r\nNegative prompt: {n}\r\n{o}";

            Clipboard.SetDataObject(parameters);
        }

        public void SearchImages()
        {
            SearchImages(null);
        }

        public void SearchImages(object obj)
        {
            if (!_settings.ImagePaths.Any())
            {
                MessageBox.Show("No image paths configured!", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            try
            {
                Dispatcher.Invoke(() =>
                {
                    _model.Images!.Clear();

                    if (!string.IsNullOrEmpty(_model.SearchText))
                    {
                        if (_model.SearchHistory.Count == 0 || (_model.SearchHistory.Count > 0 && _model.SearchHistory[0] != _model.SearchText))
                        {
                            if (_model.SearchHistory.Count + 1 > 25)
                            {
                                _model.SearchHistory.RemoveAt(_model.SearchHistory.Count - 1);
                            }
                            _model.SearchHistory.Insert(0, _model.SearchText);

                            _currentModeSettings.History = _model.SearchHistory.ToList();
                        }
                    }

                    _currentModeSettings.LastQuery = _model.SearchText;

                    // need a better way to do this... property?
                    var query = _model.SearchText + " " + _currentModeSettings.ExtraQuery;

                    var count = _dataStore.Count(query);
                    var size = _dataStore.CountFileSize(query);

                    //_model.FileSize = size;

                    _model.IsEmpty = count == 0;

                    _model.Pages = count / _settings.PageSize + (count % _settings.PageSize > 1 ? 1 : 0);

                    float fsize = size;

                    var ssize = $"{fsize:#,##0} B";

                    if (fsize > 1073741824)
                    {
                        fsize /= 1073741824;
                        ssize = $"{fsize:#,##0.00} GiB";
                    }
                    else if (fsize > 1048576)
                    {
                        fsize /= 1048576;
                        ssize = $"{fsize:#,##0.00} MiB";
                    }
                    else if (fsize > 1024)
                    {
                        fsize /= 1024;
                        ssize = $"{fsize:#,##0.00} KiB";
                    }

                    _model.Results = $"{count:###,###,##0} results found ({ssize})";

                    if (_model.IsEmpty)
                    {
                        _model.Page = 0;
                        _model.ResultStatus = "No results found";
                        //MessageBox.Show(_navigatorService.Host, "The search term yielded no results", "No results found",
                        //    MessageBoxButton.OK,
                        //    MessageBoxImage.Information);
                        return;
                    }
                    _model.Page = 1;
                });



                ReloadMatches((string)obj != "ManualSearch");
            }
            catch (Exception e)
            {
                MessageBox.Show(_navigatorService.Host, e.Message, "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SearchModel.SelectedImageEntry))
            {
                if (_model.SelectedImageEntry != null)
                {
                    var parameters = Metadata.ReadFromFile(_model.SelectedImageEntry.Path);

                    try
                    {
                        _model.CurrentImage.Image = _model.SelectedImageEntry == null ? null : GetBitmapImage(_model.SelectedImageEntry.Path);
                        _model.CurrentImage.Path = parameters.Path;
                        _model.CurrentImage.Prompt = parameters.Prompt;
                        _model.CurrentImage.NegativePrompt = parameters.NegativePrompt;
                        _model.CurrentImage.OtherParameters = parameters.OtherParameters;
                        _model.CurrentImage.Favorite = _model.SelectedImageEntry.Favorite;
                        _model.CurrentImage.Date = _model.SelectedImageEntry.CreatedDate.ToString();
                        _model.CurrentImage.Rating = _model.SelectedImageEntry.Rating;
                        _model.CurrentImage.NSFW = _model.SelectedImageEntry.NSFW;
                        _model.CurrentImage.ModelHash = parameters.ModelHash;
                        _model.CurrentImage.Seed = parameters.Seed;

                        if (_modelLookup != null)
                        {
                            var models = _modelLookup.Where(m => String.Equals(m.Hash, parameters.ModelHash, StringComparison.CurrentCultureIgnoreCase));

                            if (models.Any())
                            {
                                _model.CurrentImage.ModelName = string.Join(", ", models.Select(m => m.Filename)) + $" ({parameters.ModelHash})";
                            }
                            else
                            {
                                _model.CurrentImage.ModelName = $"Not found ({parameters.ModelHash})";
                            }
                        }
                        else
                        {
                            _model.CurrentImage.ModelName = $"Not found ({parameters.ModelHash})";
                        }

                    }
                    catch (FileNotFoundException)
                    {
                        MessageBox.Show(_navigatorService.Host, "The source image could not be located. This can happen when you move or rename the file outside of Diffusion Toolkit.", "Load image failed", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(_navigatorService.Host, $"{ex.Message}", "An error occured", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                }
            }
            else if (e.PropertyName == nameof(SearchModel.SearchText))
            {
                if (string.IsNullOrEmpty(_model.SearchText))
                {
                    GetRandomHint();
                }

            }
        }

        public static BitmapImage GetBitmapImage(string path)
        {
            BitmapImage bitmap;
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            {
                bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
            }
            bitmap.Freeze();
            return bitmap;
        }


        private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenSelected();
        }

        private void ThumbnailListView_OnKeyDown(object sender, KeyEventArgs e)
        {
            var ratings = new[]
            {
                Key.D1,
                Key.D2,
                Key.D3,
                Key.D4,
                Key.D5,
                Key.D6,
                Key.D7,
                Key.D8,
                Key.D9,
                Key.D0,
            };

            if (e.Key == Key.Enter)
            {
                OpenSelected();
            }
            else if (e.Key == Key.Delete || e.Key == Key.X)
            {
                DeleteSelected();
            }
            else if (e.Key == Key.F)
            {
                FavoriteSelected();
            }
            else if (e.Key == Key.N)
            {
                NSFWSelected();
            }
            else if (ratings.Contains(e.Key))
            {
                var rating = e.Key switch
                {
                    Key.D1 => 1,
                    Key.D2 => 2,
                    Key.D3 => 3,
                    Key.D4 => 4,
                    Key.D5 => 5,
                    Key.D6 => 6,
                    Key.D7 => 7,
                    Key.D8 => 8,
                    Key.D9 => 9,
                    Key.D0 => 10,
                };

                RateSelected(rating);

            }
        }

        private void RateSelected(int rating)
        {
            if (ThumbnailListView.SelectedItems != null)
            {
                var imageEntries = ThumbnailListView.SelectedItems.Cast<ImageEntry>().ToList();

                foreach (var entry in imageEntries)
                {

                    if (entry.Rating == rating)
                    {
                        entry.Rating = null;
                    }
                    else
                    {
                        entry.Rating = rating;
                    }
                    if (_model.CurrentImage != null && _model.CurrentImage.Path == entry.Path)
                    {
                        _model.CurrentImage.Rating = entry.Rating;
                    }
                }

                var ids = imageEntries.Select(x => x.Id).ToList();
                _dataStore.SetRating(ids, rating);
            }
        }

        private void UnrateSelected()
        {
            if (ThumbnailListView.SelectedItems != null)
            {
                var imageEntries = ThumbnailListView.SelectedItems.Cast<ImageEntry>().ToList();

                foreach (var entry in imageEntries)
                {

                    entry.Rating = null;

                    if (_model.CurrentImage != null && _model.CurrentImage.Path == entry.Path)
                    {
                        _model.CurrentImage.Rating = entry.Rating;
                    }
                }

                var ids = imageEntries.Select(x => x.Id).ToList();
                _dataStore.SetRating(ids, null);
            }
        }

        private void FavoriteSelected()
        {
            if (ThumbnailListView.SelectedItems != null)
            {
                var imageEntries = ThumbnailListView.SelectedItems.Cast<ImageEntry>().ToList();

                var favorite = !imageEntries.GroupBy(e => e.Favorite).OrderByDescending(g => g.Count()).First().Key;

                foreach (var entry in imageEntries)
                {
                    entry.Favorite = favorite;
                    if (_model.CurrentImage != null && _model.CurrentImage.Path == entry.Path)
                    {
                        _model.CurrentImage.Favorite = favorite;
                    }
                }

                var ids = imageEntries.Select(x => x.Id).ToList();
                _dataStore.SetFavorite(ids, favorite);
            }
        }

        private void NSFWSelected()
        {
            if (ThumbnailListView.SelectedItems != null)
            {
                var imageEntries = ThumbnailListView.SelectedItems.Cast<ImageEntry>().ToList();

                var nsfw = !imageEntries.GroupBy(e => e.NSFW).OrderByDescending(g => g.Count()).First().Key;

                foreach (var entry in imageEntries)
                {
                    entry.NSFW = nsfw;
                    if (_model.CurrentImage != null && _model.CurrentImage.Path == entry.Path)
                    {
                        _model.CurrentImage.NSFW = nsfw;
                    }
                }

                var ids = imageEntries.Select(x => x.Id).ToList();
                _dataStore.SetNSFW(ids, nsfw);
            }
        }

        private void DeleteSelected()
        {
            if (ThumbnailListView.SelectedItems != null)
            {
                if (ThumbnailListView.SelectedItems != null)
                {
                    var imageEntries = ThumbnailListView.SelectedItems.Cast<ImageEntry>().ToList();

                    var delete = !imageEntries.GroupBy(e => e.ForDeletion).OrderByDescending(g => g.Count()).First().Key;

                    foreach (var entry in imageEntries)
                    {
                        entry.ForDeletion = delete;
                        if (_model.CurrentImage != null && _model.CurrentImage.Path == entry.Path)
                        {
                            _model.CurrentImage.ForDeletion = delete;
                        }
                    }

                    var ids = imageEntries.Select(x => x.Id).ToList();
                    _dataStore.SetDeleted(ids, delete);
                }
            }
        }

        public void SetOpacityView(bool value)
        {
            _model.ImageOpacity = value ? 0.35f : 1.0f;
        }

        public void SetIconVisibility(bool value)
        {
            _model.HideIcons = value;
        }

        private void OpenSelected()
        {
            using Process fileopener = new Process();

            if (_model.SelectedImageEntry != null)
            {
                fileopener.StartInfo.FileName = "explorer";
                fileopener.StartInfo.Arguments = "\"" + _model.SelectedImageEntry.Path + "\"";
                fileopener.Start();
            }
        }



        public Task ReloadMatches(bool focus = true)
        {
            return Task.Run(LoadMatchesOnThread)
                .ContinueWith(t =>
                {
                    if (t.IsCompletedSuccessfully)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            ResetView(focus);
                        });
                    }
                });
        }


        private void LoadMatchesOnThread()
        {
            var rId = r.NextInt64();

            ThumbnailLoader.Instance.SetCurrentRequestId(rId);

            var query = _model.SearchText + " " + _currentModeSettings.ExtraQuery;

            var matches = _dataStore
                .Search(query, _settings.PageSize,
                    _settings.PageSize * (_model.Page - 1));


            var images = new List<ImageEntry>();

            Dispatcher.Invoke(() =>
            {
                _model.TotalFiles = _settings.PageSize;
                _model.CurrentPosition = 0;
            });

            var sw = new Stopwatch();
            sw.Start();

            Dispatcher.Invoke(() =>
            {
                _model.Images = new ObservableCollection<ImageEntry>();
            });

            var count = 0;
            foreach (var file in matches)
            {
                //if (token.IsCancellationRequested)
                //{
                //    break;
                //}

                images.Add(new ImageEntry(rId)
                {
                    Id = file.Id,
                    Favorite = file.Favorite,
                    ForDeletion = file.ForDeletion,
                    Rating = file.Rating,
                    Path = file.Path,
                    CreatedDate = file.CreatedDate,
                    FileName = Path.GetFileName(file.Path),
                    NSFW = file.NSFW
                });

                if (count % 10 == 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        foreach (var image in images)
                        {
                            _model.Images.Add(image);
                        }
                        _model.CurrentPosition += images.Count;
                        images.Clear();
                    });
                }


                //Dispatcher.Invoke(() =>
                //{
                //    _model.Images.Add(new ImageEntry(rId)
                //    {
                //        Id = file.Id,
                //        Favorite = file.Favorite,
                //        ForDeletion = file.ForDeletion,
                //        Rating = file.Rating,
                //        Path = file.Path,
                //        FileName = Path.GetFileName(file.Path),
                //    });
                //    _model.CurrentPosition++;
                //});


                count++;
            }


            Dispatcher.Invoke(() =>
            {
                foreach (var image in images)
                {
                    _model.Images.Add(image);
                }

                _model.TotalFiles = Int32.MaxValue;
                _model.CurrentPosition = 0;
            });

            sw.Stop();

            Debug.WriteLine($"Loaded in {sw.ElapsedMilliseconds:#,###,##0}ms");


        }

        private void Page_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ReloadMatches();
                e.Handled = true;
            }
        }

        private void ResetView(bool focus)
        {
            if (_model.Images is { Count: > 0 })
            {
                ThumbnailListView.ScrollIntoView(_model.Images[0]);
                ThumbnailListView.SelectedItem = _model.Images[0];

                if (focus)
                {
                    if (ThumbnailListView.ItemContainerGenerator.ContainerFromIndex(0) is ListViewItem item)
                    {
                        item.Focus();
                    }

                }
            }
        }

        private Dictionary<string, ModeSettings> _modeSettings = new Dictionary<string, ModeSettings>();

        private ModeSettings GetModeSettings(string mode)
        {
            if (!_modeSettings.TryGetValue(mode, out var settings))
            {
                settings = new ModeSettings();
            }
            return settings;
        }

        private void SetMode(string mode)
        {
            _currentModeSettings = GetModeSettings(mode);
            _model.SearchText = _currentModeSettings.LastQuery;
            _model.SearchHistory = new ObservableCollection<string?>(_currentModeSettings.History);
            _model.ModeName = _currentModeSettings.Name;
        }

        public void ShowSearch()
        {
            SetMode("search");
            SearchImages(null);
        }

        public void ShowFavorite()
        {
            SetMode("favorites");
            SearchImages(null);
        }

        public void ShowMarked()
        {
            SetMode("deleted");
            SearchImages(null);
        }

        private void UIElement_OnMouseMove(object sender, MouseEventArgs e)
        {
            //if (e.LeftButton == MouseButtonState.Pressed)
            //{
            //}
        }


        public void GoFirstPage()
        {
            _model.Page = 1;

            ReloadMatches();
        }

        public void GoLastPage()
        {
            _model.Page = _model.Pages;

            ReloadMatches();
        }

        public void GoPrevPage()
        {
            _model.Page--;

            ReloadMatches();
        }

        public void GoNextPage()
        {
            _model.Page++;

            ReloadMatches();
        }


        private void FirstPage_OnClick(object sender, RoutedEventArgs e)
        {
            GoFirstPage();
        }
        private void PrevPage_OnClick(object sender, RoutedEventArgs e)
        {
            GoPrevPage();
        }

        private void NextPage_OnClick(object sender, RoutedEventArgs e)
        {
            GoNextPage();
        }

        private void LastPage_OnClick(object sender, RoutedEventArgs e)
        {
            GoLastPage();
        }

        private List<ImageEntry> _selItems = new List<ImageEntry>();
        private Point _start;
        private bool _restoreSelection;
        private void ThumbnailListView_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //if (ThumbnailListView.SelectedItems.Count == 0)
            //    return;
            ImageEntry? currentShape = ThumbnailListView.SelectedItem as ImageEntry;

            System.Windows.Point pt = e.GetPosition(ThumbnailListView);
            var item = System.Windows.Media.VisualTreeHelper.HitTest(ThumbnailListView, pt);

            var thumbnail = item.VisualHit as Thumbnail;

            this._start = e.GetPosition(null);
            _selItems.Clear();
            _selItems.AddRange(ThumbnailListView.SelectedItems.Cast<ImageEntry>());

            //_restoreSelection = false;

            //if (thumbnail != null && ThumbnailListView.SelectedItems.Contains(thumbnail.DataContext))
            //{
            //    _restoreSelection = true;
            //}
        }

        private void ThumbnailListView_OnMouseMove(object sender, MouseEventArgs e)
        {
            Point mpos = e.GetPosition(null);
            Vector diff = this._start - mpos;

            if (e.LeftButton == MouseButtonState.Pressed && (e.OriginalSource is Thumbnail) &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                if (this.ThumbnailListView.SelectedItems.Count == 0)
                {
                    return;
                }

                if (_selItems.Contains(ThumbnailListView.SelectedItems[0]))
                {
                    foreach (object selItem in _selItems)
                    {
                        if (!ThumbnailListView.SelectedItems.Contains(selItem))
                            ThumbnailListView.SelectedItems.Add(selItem);
                    }
                }
                else
                {
                    _selItems.Clear();
                    _selItems.AddRange(ThumbnailListView.SelectedItems.Cast<ImageEntry>());
                }


                var source = (ListView)sender;
                //var path = ((ImageEntry)source.DataContext).Path;

                DataObject dataObject = new DataObject();
                dataObject.SetData(DataFormats.FileDrop, _selItems.Select(t => t.Path).ToArray());
                DragDrop.DoDragDrop(source, dataObject, DragDropEffects.Copy);

            }

        }

        private void ThumbnailListView_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            //if (e.LeftButton == MouseButtonState.Pressed)
            //{
            //    if (this.ThumbnailListView.SelectedItems.Count == 0)
            //    {
            //        return;
            //    }

            //    foreach (object selItem in _selItems)
            //    {
            //        if (!ThumbnailListView.SelectedItems.Contains(selItem))
            //            ThumbnailListView.SelectedItems.Add(selItem);
            //    }

            //}

        }

        private void SearchTermTextBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //SearchImages(null);
        }

        private void SearchTermTextBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Down:
                    SearchTermTextBox.IsDropDownOpen = true;
                    e.Handled = true;
                    break;
            }
        }

        public void SetModels(ICollection<Model> modelsCollection)
        {
            _modelLookup = modelsCollection;
        }

        public void ToggleInfo()
        {
            _model.CurrentImage.IsParametersVisible = !_model.CurrentImage.IsParametersVisible;
        }

        public void SetNSFWBlur(bool value)
        {
            _model.NSFWBlur = value;
        }

        private void Rate1_OnClick(object sender, RoutedEventArgs e)
        {
            RateSelected(1);
        }

        private void Rate2_OnClick(object sender, RoutedEventArgs e)
        {
            RateSelected(2);
        }

        private void Rate3_OnClick(object sender, RoutedEventArgs e)
        {
            RateSelected(3);
        }
        private void Rate4_OnClick(object sender, RoutedEventArgs e)
        {
            RateSelected(4);
        }

        private void Rate5_OnClick(object sender, RoutedEventArgs e)
        {
            RateSelected(5);
        }

        private void Unrate_OnClick(object sender, RoutedEventArgs e)
        {
            UnrateSelected();
        }

    }
}
