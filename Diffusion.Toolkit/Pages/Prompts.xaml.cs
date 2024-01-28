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
using Diffusion.Analysis;
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
        private readonly Settings _settings;
        private PromptsModel _model;
        private BitsetDatabase _bitsetDatabase;
        private List<ImagePrompt> _imagePrompts;

        public Prompts(IOptions<DataStore> dataStoreOptions, MessagePopupManager messagePopupManager, MainModel mainModel, Settings settings)
        {
            _dataStore = dataStoreOptions.Value;
            _settings = settings;
            InitializeComponent();

            _model = new PromptsModel(mainModel);

            _model.PromptsResults.PageChangedCommand = new RelayCommand<PageChangedEventArgs>((o) =>
            {
                ReloadMatches(true, o.GotoEnd);
            });

            DataContext = _model;

            ThumbnailListView.DataStoreOptions = dataStoreOptions;

            ThumbnailListView.MessagePopupManager = messagePopupManager;

            _bitsetDatabase = new BitsetDatabase(_dataStore);

            _imagePrompts = _dataStore.GetImagePrompts().ToList();

            Task.Run(() =>
            {
                _bitsetDatabase.Rebuild();
                _bitsetDatabase.ProcessPrompts();
            });

            ReloadPrompts();

            _model.PropertyChanged += ModelOnPropertyChanged;
        }

        public Settings Settings { get; set; }

        private void ModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PromptsModel.FullTextPrompt) || e.PropertyName == nameof(PromptsModel.PromptDistance))
            {
                LoadPrompts();
            }
            if (e.PropertyName == nameof(PromptsModel.NegativeFullTextPrompt) || e.PropertyName == nameof(PromptsModel.NegativePromptDistance))
            {
                LoadNegativePrompts();
            }
        }

        private void PromptQuery_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoadPrompts();
            }
        }

        private void NegativePromptQuery_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoadNegativePrompts();
            }
        }

        private void LoadPrompts()
        {
            IEnumerable<UsedPrompt> usedPrompts;

            if (string.IsNullOrEmpty(_model.PromptQuery))
            {
                usedPrompts = _imagePrompts.GroupBy(p => p.Prompt).Select(u => new UsedPrompt()
                {
                    Prompt = u.Key,
                    Usage = u.Count()
                }).OrderByDescending(u => u.Usage);
            }
            else
            {
                var scores = _bitsetDatabase.GetSimilarPrompts(_model.PromptQuery, 1f, 0.0f);

                var similarPrompts = _imagePrompts.Join(scores, prompt => prompt.Id, score => score.Id, (prompt, score) => new { prompt, score });

                usedPrompts = similarPrompts.GroupBy(p => p.prompt.Prompt).Select(u => new UsedPrompt()
                {
                    Prompt = u.Key,
                    Usage = u.Count(),
                    Score = u.Select(a => a.score).Average(a => a.Score)
                }).OrderByDescending(u => u.Score).ThenByDescending(u => u.Usage);
            }

            //_dataStore.SearchPrompts(_model.PromptQuery, _model.FullTextPrompt, _model.PromptDistance);

            _model.Prompts = new ObservableCollection<UsedPrompt>(usedPrompts);
        }

        private void LoadNegativePrompts()
        {
            _model.NegativePrompts = new ObservableCollection<UsedPrompt>(_dataStore.SearchNegativePrompts(_model.NegativePromptQuery, _model.NegativeFullTextPrompt, _model.NegativePromptDistance));
        }

        public void ReloadPrompts()
        {
            LoadPrompts();
            LoadNegativePrompts();
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

        public void ReloadMatches(bool focus = true, bool gotoEnd = false)
        {
            Task.Run(() =>
            {
                LoadMatches();
                ThumbnailListView.ResetView(focus, gotoEnd);
            });
        }

        private Random r = new Random();

        private void LoadMatches()
        {
            Dispatcher.Invoke(() =>
            {
                _model.PromptsResults.Images?.Clear();
            });

            var images = new List<ImageEntry>();

            var rId = r.NextInt64();
            ThumbnailLoader.Instance.SetCurrentRequestId(rId);


            IEnumerable<Image> matches = Enumerable.Empty<Image>();


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
                RefreshThumbnails();

            });

            sw.Stop();


            Debug.WriteLine($"Loaded in {sw.ElapsedMilliseconds:#,###,##0}ms");



        }

        public void SetThumbnailSize(int thumbnailSize)
        {
            ThumbnailListView.SetThumbnailSize(thumbnailSize);
            RefreshThumbnails();
        }


        public void SetPageSize(int pageSize)
        {
            ThumbnailListView.PageSize = pageSize;
        }

        public void RefreshThumbnails()
        {
            if (_model.PromptsResults.Images != null)
            {
                foreach (var image in _model.PromptsResults.Images)
                {
                    image.LoadThumbnail();
                }
            }
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Task.Run(LoadImages);
        }
    }
}
