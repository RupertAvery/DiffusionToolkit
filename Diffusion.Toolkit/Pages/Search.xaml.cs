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
using System.Threading;
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


        private ScrollDragger _scrollDragger;

        public Search(NavigatorService navigatorService, DataStore dataStore, MessagePopupManager messagePopupManager, Settings settings) : this()
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

            _model.DataStore = _dataStore;
            _model.Page = 0;
            _model.Pages = 0;
            _model.TotalFiles = 100;
            _model.Images = new ObservableCollection<ImageEntry>();
            _model.PropertyChanged += ModelOnPropertyChanged;
            _model.SearchCommand = new RelayCommand<object>(SearchImages);

            _model.Refresh = new RelayCommand<object>((o) => ReloadMatches());
            _model.ToggleParameters = new RelayCommand<object>((o) => ToggleInfo());
            _model.CopyFiles = new RelayCommand<object>((o) => CopyFiles());

            _model.FocusSearch = new RelayCommand<object>((o) => SearchTermTextBox.Focus());
            _model.ShowDropDown = new RelayCommand<object>((o) => SearchTermTextBox.IsDropDownOpen = true);
            _model.HideDropDown = new RelayCommand<object>((o) => SearchTermTextBox.IsDropDownOpen = false);
            SetMode("search");

            DataContext = _model;

            ThumbnailListView.DataStore = dataStore;
            ThumbnailListView.MessagePopupManager = messagePopupManager;

            _scrollDragger = new ScrollDragger(Preview, ScrollViewer);
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



        private void WidthChanged(object? sender, EventArgs e)
        {
            _settings.MainGridWidth = MainGrid.ColumnDefinitions[0].Width.ToString();
        }

        private void WidthChanged2(object? sender, EventArgs e)
        {
            _settings.MainGridWidth2 = MainGrid.ColumnDefinitions[2].Width.ToString();
        }

        public Settings Settings
        {
            get => _settings;
            set => _settings = value;
        }

        public Action<IList<ImageEntry>> MoveFiles
        {
            get => ThumbnailListView.MoveFiles;
            set => ThumbnailListView.MoveFiles = value;

        }

        private void ShowInExplorer(object obj)
        {
            if (_model.CurrentImage == null) return;
            var p = _model.CurrentImage.Path;
            Process.Start("explorer.exe", $"/select,\"{p}\"");
        }


        //private void CopyPath(object obj)
        //{
        //    if (_model.CurrentImage == null) return;
        //    var p = _model.CurrentImage.Path;
        //    Clipboard.SetText(p);
        //}

        //private void CopyPrompt(object obj)
        //{
        //    if (_model.CurrentImage == null) return;
        //    var p = _model.CurrentImage.Prompt;
        //    Clipboard.SetText(p);
        //}

        //private void CopyNegative(object obj)
        //{
        //    if (_model.CurrentImage == null) return;
        //    var p = _model.CurrentImage.NegativePrompt;
        //    Clipboard.SetText(p);
        //}

        //private void CopySeed(object obj)
        //{
        //    if (_model.CurrentImage == null) return;
        //    var p = _model.CurrentImage.Seed.ToString();
        //    Clipboard.SetText(p);
        //}

        //private void CopyHash(object obj)
        //{
        //    if (_model.CurrentImage == null) return;
        //    var p = _model.CurrentImage.ModelHash;
        //    Clipboard.SetText(p);
        //}

        //private void CopyParameters(object obj)
        //{
        //    if (_model.CurrentImage == null) return;

        //    var p = _model.CurrentImage.Prompt;
        //    var n = _model.CurrentImage.NegativePrompt;
        //    var o = _model.CurrentImage.OtherParameters;
        //    var parameters = $"{p}\r\n\r\nNegative prompt: {n}\r\n{o}";

        //    Clipboard.SetText(parameters);
        //}

        public void SearchImages()
        {
            SearchImages(null);
        }

        static T Time<T>(Func<T> action)
        {
            Stopwatch t = new Stopwatch();
            T result = action();
            t.Start();
            Debug.WriteLine($"{t.ElapsedMilliseconds}ms");
            t.Stop();
            return result;
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
                    //_model.Images!.Clear();

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

                    ThumbnailListView.SetPagingEnabled();
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
                        _zoomValue = 1;

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

                        ZoomPreview();
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
            //else if (e.PropertyName == nameof(SearchModel.Page))
            //{
            //    ReloadMatches(true);
            //}
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

        public void SetOpacityView(bool value)
        {
            _model.ImageOpacity = value ? 0.35f : 1.0f;
        }

        public void SetIconVisibility(bool value)
        {
            _model.HideIcons = value;
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
                            ThumbnailListView.ResetView(focus);
                        });
                    }
                });
        }


        private void LoadMatchesOnThread()
        {
            var rId = r.NextInt64();

            ThumbnailLoader.Instance.SetCurrentRequestId(rId);

            var query = _model.SearchText + " " + _currentModeSettings.ExtraQuery;

            var matches = Time(() => _dataStore
                .Search(query, _settings.PageSize,
                    _settings.PageSize * (_model.Page - 1)));

            Dispatcher.Invoke(() =>
            {
                //_model.Images.Clear();
                _model.Images = new ObservableCollection<ImageEntry>();
            });

            var images = new List<ImageEntry>();

            var sw = new Stopwatch();
            sw.Start();


            var count = 0;
            foreach (var file in matches)
            {
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

                if (images.Count == 30)
                {
                    foreach (var image in images)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            _model.Images.Add(image);

                        });
                    }
                    images.Clear();
                }

                count++;
            }

            Dispatcher.Invoke(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    foreach (var image in images)
                    {
                        _model.Images.Add(image);
                    }
                    images.Clear();
                });
                //_model.Images.Clear();
                //_model.Images = new ObservableCollection<ImageEntry>(images);
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

        private void ThumbnailListView_OnPageChangedEvent(object? sender, int e)
        {
            ReloadMatches(true);
        }
        private double _zoomValue = 1.0;

        private void UIElement_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (e.Delta > 0)
                {
                    _zoomValue += 0.1;
                }
                else
                {
                    _zoomValue -= 0.1;
                }


                ZoomPreview();

                e.Handled = true;
            }
        }

        private void ZoomPreview()
        {
            if (_zoomValue < 0.1)
            {
                _zoomValue = 0.1;
            }
            if (_zoomValue > 3)
            {
                _zoomValue = 3;
            }

            ScaleTransform scale = new ScaleTransform(_zoomValue, _zoomValue);
            Preview.LayoutTransform = scale;
        }

        private void UIElement_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.D0 && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                _zoomValue = 1;
                ZoomPreview();
            }
            if (e.Key == Key.OemPlus && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                _zoomValue += 0.1;
                ZoomPreview();
            }
            if (e.Key == Key.OemMinus && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                _zoomValue -= 0.1;
                ZoomPreview();
            }


            e.Handled = true;
        }
    }
}
public class ScrollDragger
{
    private readonly ScrollViewer _scrollViewer;
    private readonly UIElement _content;
    private readonly Cursor _dragCursor = Cursors.Hand;
    private double _scrollMouseX;
    private double _scrollMouseY;
    private int _updateCounter = 0;

    public ScrollDragger(UIElement content, ScrollViewer scrollViewer)
    {
        _scrollViewer = scrollViewer;
        _content = content;

        content.MouseLeftButtonDown += scrollViewer_MouseLeftButtonDown;
        content.PreviewMouseMove += scrollViewer_PreviewMouseMove;
        content.PreviewMouseLeftButtonUp += scrollViewer_PreviewMouseLeftButtonUp;
    }

    private void scrollViewer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Capture the mouse, reset counter, switch to hand cursor to indicate dragging
        _content.CaptureMouse();
        _updateCounter = 0;
        _scrollViewer.Cursor = _dragCursor;
    }

    private void scrollViewer_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (_content.IsMouseCaptured)
        {
            _updateCounter++;

            // Skip dragging on the first PreviewMouseMove event after the left mouse button goes down. It actually triggers two of these and this ignores both, preventing jumping.
            if (_updateCounter <= 1)
            {
                // Grab starting mouse offset relative to scroll viewer, used to calculate first delta
                _scrollMouseY = e.GetPosition(_scrollViewer).Y;
                _scrollMouseX = e.GetPosition(_scrollViewer).X;
                return;
            }

            // Calculate new vertical offset then scroll to it
            var newVOff = HandleMouseMoveAxisUpdateScroll(_scrollViewer.VerticalOffset, ref _scrollMouseY, e.GetPosition(_scrollViewer).Y, _scrollViewer.ScrollableHeight);
            _scrollViewer.ScrollToVerticalOffset(newVOff);

            // Calculate new horizontal offset and scroll to it
            var newHOff = HandleMouseMoveAxisUpdateScroll(_scrollViewer.HorizontalOffset, ref _scrollMouseX, e.GetPosition(_scrollViewer).X, _scrollViewer.ScrollableWidth);
            _scrollViewer.ScrollToHorizontalOffset(newHOff);
        }
    }

    private double HandleMouseMoveAxisUpdateScroll(double offsetStart, ref double oldScrollMouse, double newScrollMouse, double scrollableMax)
    {
        // How far does the user want to drag since the last update?
        var mouseDelta = oldScrollMouse - newScrollMouse;

        // Add mouse delta to current scroll offset to get the new expected scroll offset
        var newScrollOffset = offsetStart + mouseDelta;

        // Keep the scroll offset from going off the screen
        var newScrollOffsetClamped = newScrollOffset.Clamp(0, scrollableMax);

        // Save the current mouse position in scroll coordinates so that we'll have it for next update
        oldScrollMouse = newScrollMouse;

        return newScrollOffsetClamped;
    }

    private void scrollViewer_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _content.ReleaseMouseCapture();
        _updateCounter = 0; // Reset counter, used to prevent jumping at start of drag
        _scrollViewer.Cursor = null;
    }

    public void Unload()
    {
        _content.MouseLeftButtonDown -= scrollViewer_MouseLeftButtonDown;
        _content.PreviewMouseMove -= scrollViewer_PreviewMouseMove;
        _content.PreviewMouseLeftButtonUp -= scrollViewer_PreviewMouseLeftButtonUp;
    }
}

public static class MathExtensions
{
    // Clamp the value between the min and max. Value returned will be min or max if it's below min or above max
    public static double Clamp(this Double value, double min, double max)
    {
        return Math.Min(Math.Max(value, min), max);
    }
}