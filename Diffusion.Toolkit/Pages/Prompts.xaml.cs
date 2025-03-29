using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Diffusion.Database;
using Diffusion.Toolkit.Classes;
using Diffusion.Toolkit.Common;
using Diffusion.Toolkit.Controls;
using Diffusion.Toolkit.Models;
using Diffusion.Toolkit.Thumbnails;
using Microsoft.Extensions.Options;
using Image = Diffusion.Database.Image;

namespace Diffusion.Toolkit.Pages
{
    /// <summary>
    /// Interaction logic for Prompts.xaml
    /// </summary>
    public partial class Prompts : Page
    {
        private readonly DataStore _dataStore;
        private readonly Toolkit.Settings _settings;
        private PromptsModel _model;
        private bool _isLoaded;
        public Prompts(NavigatorService navigatorService, IOptions<DataStore> dataStoreOptions, MessagePopupManager messagePopupManager, MainModel mainModel, Toolkit.Settings settings)
        {
            _dataStore = dataStoreOptions.Value;
            _settings = settings;
            InitializeComponent();

            navigatorService.OnNavigate += (sender, args) =>
            {
                if (this == args.TargetPage && !_isLoaded)
                {
                    ReloadPrompts();
                    _isLoaded = true;
                }
            };

            _model = new PromptsModel(mainModel);

            _model.PromptsResults.PageChangedCommand = new RelayCommand<PageChangedEventArgs>((o) =>
            {
                ReloadMatches(new ReloadOptions() { Focus = true, CursorPosition = o.CursorPosition });
            });

            DataContext = _model;

            ThumbnailListView.DataStoreOptions = dataStoreOptions;

            ThumbnailListView.MessagePopupManager = messagePopupManager;

            _model.PropertyChanged += ModelOnPropertyChanged;
        }

        public Toolkit.Settings Settings { get; set; }

        private void ModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PromptsModel.FullTextPrompt) || e.PropertyName == nameof(PromptsModel.PromptDistance))
            {
                Dispatcher.Invoke(() => { LoadPrompts(); });
            }
            if (e.PropertyName == nameof(PromptsModel.NegativeFullTextPrompt) || e.PropertyName == nameof(PromptsModel.NegativePromptDistance))
            {
                Dispatcher.Invoke(() => { LoadNegativePrompts(); });
            }
        }

        private void PromptQuery_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Dispatcher.Invoke(() => { LoadPrompts(); });
            }
        }

        private void NegativePromptQuery_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Dispatcher.Invoke(() => { LoadNegativePrompts(); });
            }
        }

        private void LoadPrompts()
        {
            _model.Prompts = new ObservableCollection<UsedPrompt>(_dataStore.SearchPrompts(_model.PromptQuery, _model.FullTextPrompt, _model.PromptDistance));
        }

        private void LoadNegativePrompts()
        {
            _model.NegativePrompts = new ObservableCollection<UsedPrompt>(_dataStore.SearchNegativePrompts(_model.NegativePromptQuery, _model.NegativeFullTextPrompt, _model.NegativePromptDistance));
        }

        public void ReloadPrompts()
        {
            Task.Run(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    LoadPrompts();
                    LoadNegativePrompts();
                });
            });
        }

        public void LoadImages()
        {
            if (_model.SelectedPrompt == null) return;

            var query = _model.SelectedPrompt.Prompt;

            _model.PromptsResults.ResultStatus = "Loading...";

            var count = _dataStore.CountPrompt(query);
            var size = _dataStore.CountPromptFileSize(query);

            _model.PromptsResults.IsEmpty = count == 0;

            if (_model.PromptsResults.IsEmpty)
            {
                //_model.CurrentImage.;
            }

            _model.PromptsResults.Pages = count / _settings.PageSize + (count % _settings.PageSize > 1 ? 1 : 0);

            float fsize = size;

            var ssize = $"{fsize:n0} B";

            if (fsize > 1073741824)
            {
                fsize /= 1073741824;
                ssize = $"{fsize:n2} GiB";
            }
            else if (fsize > 1048576)
            {
                fsize /= 1048576;
                ssize = $"{fsize:n2} MiB";
            }
            else if (fsize > 1024)
            {
                fsize /= 1024;
                ssize = $"{fsize:n2} KiB";
            }

            _model.PromptsResults.Results = $"{count:n0} results found ({ssize})";

            if (_model.PromptsResults.IsEmpty)
            {
                _model.PromptsResults.Page = 0;
                _model.PromptsResults.ResultStatus = "No results found";
                ReloadMatches();
                return;
            }

            _model.PromptsResults.Page = 1;

            ReloadMatches();
        }

        public void ReloadMatches(ReloadOptions? reloadOptions = null)
        {
            Task.Run(() =>
            {
                LoadMatches();
                ThumbnailListView.ResetView(reloadOptions);
            });
        }

        private Random r = new Random();

        private void LoadMatches()
        {
            Dispatcher.Invoke(() =>
            {
                _model.IsBusy = true;
                //_model.PromptsResults.Images?.Clear();
            });

            var images = new List<ImageEntry>();

            ThumbnailLoader.Instance.StopCurrentBatch();

            var rId = ThumbnailLoader.Instance.StartBatch();


            IEnumerable<ImageView> matches = Enumerable.Empty<ImageView>();


            var query = _model.SelectedPrompt.Prompt;
            bool showImages = true;


            if (showImages)
            {
                matches = _dataStore
                    .SearchPrompt(query, _settings.PageSize,
                        _settings.PageSize * (_model.PromptsResults.Page - 1),
                        _model.PromptsResults.SortBy,
                        _model.PromptsResults.SortDirection
                    );
            }



            //var images = new List<ImageEntry>();

            var sw = new Stopwatch();
            sw.Start();


            var count = 0;


            foreach (var file in matches)
            {
                var imageEntry = new ImageEntry(rId)
                {
                    Id = file.Id,
                    Favorite = file.Favorite,
                    ForDeletion = file.ForDeletion,
                    Rating = file.Rating,
                    Score = $"{file.AestheticScore:0.0}",
                    Path = file.Path,
                    CreatedDate = file.CreatedDate,
                    FileName = Path.GetFileName(file.Path),
                    NSFW = file.NSFW,
                    AlbumCount = file.AlbumCount,
                    EntryType = EntryType.File
                };

                images.Add(imageEntry);

                //Dispatcher.Invoke(() =>
                //{
                //    _model.Images.Add(imageEntry);
                //});

                //images.Add(imageEntry);


                count++;
            }

            //_model.Images = new ObservableCollection<ImageEntry>(images);
            //RefreshThumbnails();

            _model.PromptsResults.ResultStatus = "";

            Dispatcher.Invoke(() =>
            {
                _model.PromptsResults.Images = new ObservableCollection<ImageEntry>(images);
                //RefreshThumbnails();
            });


            Dispatcher.Invoke(() =>
            {
                if (_model.PromptsResults.Images == null || _model.PromptsResults.Images.Count != images.Count)
                {
                    _model.PromptsResults.Images = new ObservableCollection<ImageEntry>(images);
                }
                else
                {
                    for (var i = 0; i < images.Count; i++)
                    {
                        var dest = _model.PromptsResults.Images[i];
                        var src = images[i];

                        dest.BatchId = src.BatchId;
                        dest.Id = src.Id;
                        dest.EntryType = src.EntryType;
                        dest.Name = src.Name;
                        dest.ForDeletion = src.ForDeletion;
                        dest.Favorite = src.Favorite;
                        dest.Rating = src.Rating;
                        dest.Score = src.Score;
                        dest.NSFW = src.NSFW;
                        dest.FileName = src.FileName;
                        dest.Unavailable = src.Unavailable;
                        dest.Height = src.Height;
                        dest.Width = src.Width;
                        dest.Path = src.Path;
                        dest.CreatedDate = src.CreatedDate;
                        dest.AlbumCount = src.AlbumCount;
                        dest.Albums = src.Albums;
                        dest.HasError = src.HasError;

                        dest.LoadState = LoadState.Unloaded;
                        dest.Dispatcher = Dispatcher;
                        dest.Thumbnail = null;
                    }
                }

                ThumbnailListView.ReloadThumbnailsView();

                _model.IsBusy = false;
            });

            sw.Stop();


            Debug.WriteLine($"Loaded in {sw.ElapsedMilliseconds:#,###,##0}ms");



        }

        public void SetThumbnailSize(int thumbnailSize)
        {
            ThumbnailListView.SetThumbnailSize(thumbnailSize);
            //RefreshThumbnails();
        }


        public void SetPageSize(int pageSize)
        {
            ThumbnailListView.PageSize = pageSize;
        }

        //public void RefreshThumbnails()
        //{
        //    if (_model.PromptsResults.Images != null)
        //    {
        //        foreach (var image in _model.PromptsResults.Images)
        //        {
        //            image.QueueLoadThumbnail();
        //        }
        //    }
        //}

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Task.Run(LoadImages);
        }
    }
}
