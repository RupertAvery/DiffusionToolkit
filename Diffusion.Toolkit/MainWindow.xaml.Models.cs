using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using Diffusion.Civitai;
using Diffusion.Civitai.Models;
using System.Diagnostics;
using System.Threading;
using Diffusion.Toolkit.Models;
using Diffusion.Toolkit.Services;

namespace Diffusion.Toolkit
{
    public partial class MainWindow
    {
        public void LoadImageModels()
        {
            var existingModels = _model.ImageModels == null ? Enumerable.Empty<ModelViewModel>() : _model.ImageModels.ToList();

            var imageModels = _dataStore.GetImageModels();

            _model.ImageModels = imageModels.Select(m => new ModelViewModel()
            {
                IsTicked = existingModels.FirstOrDefault(d => d.Name == m.Name || d.Hash == m.Hash)?.IsTicked ?? false,
                Name = m.Name ?? ResolveModelName(m.Hash),
                Hash = m.Hash,
                ImageCount = m.ImageCount
            }).Where(m => !string.IsNullOrEmpty(m.Name) && !string.IsNullOrEmpty(m.Hash)).OrderBy(x => x.Name).ToList();

            foreach (var model in _model.ImageModels)
            {
                model.PropertyChanged += ImageModelOnPropertyChanged;
            }

            _model.ImageModelNames = imageModels.Where(m => !string.IsNullOrEmpty(m.Name)).Select(m => m.Name).OrderBy(x => x);

        }

        private void ImageModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ModelViewModel.IsTicked))
            {
                var selectedModels = _model.ImageModels.Where(d => d.IsTicked).ToList();
                _model.SelectedModelsCount = selectedModels.Count;
                _model.HasSelectedModels = selectedModels.Any();
                _search.SearchImages();
            }
        }

        private string ResolveModelName(string hash)
        {
            var matches = _allModels.Where(m =>
                !string.IsNullOrEmpty(hash) &&
                (String.Equals(m.Hash, hash, StringComparison.CurrentCultureIgnoreCase)
                 ||
                 (m.SHA256 != null && string.Equals(m.SHA256.Substring(0, hash.Length), hash, StringComparison.CurrentCultureIgnoreCase))
                )).ToList();

            if (matches.Any())
            {
                return matches[0].Filename;
            }
            else
            {
                return hash;
            }
        }

        public async void DownloadCivitaiModels()
        {
            if (_model.IsBusy)
            {
                return;
            }

            // TODO: Fix    
            // TODO: Localize

            var message = "This will download Civitai model info.\r\n\r\n" + "Are you sure you want to continue?";

            var result = await _messagePopupManager.ShowCustom(message, "Download Civitai models", PopupButtons.YesNo, 500, 250);

            if (result == PopupResult.Yes)
            {
                if (await ServiceLocator.ProgressService.TryStartTask())
                {
                    try
                    {


                        var collection = await FetchCivitaiModels(ServiceLocator.ProgressService.CancellationToken);

                        if (ServiceLocator.ProgressService.CancellationToken.IsCancellationRequested)
                        {
                            return;
                        }

                        var options = new JsonSerializerOptions()
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                            Converters = { new JsonStringEnumConverter() }
                        };

                        var baseTime = new DateTime(1970, 1, 1, 0, 0, 0);

                        var mTime = DateTime.Now - baseTime;

                        collection.Date = mTime.TotalSeconds;

                        var json = JsonSerializer.Serialize(collection, options);

                        File.WriteAllText(Path.Combine(AppDir, "models.json"), json);

                        message = $"{collection.Models.Count} models were retrieved";

                        await _messagePopupManager.Show(message, "Download Civitai models", PopupButtons.OK);

                        LoadModels();
                    }
                    finally
                    {
                        Dispatcher.Invoke(() =>
                        {
                            _model.TotalProgress = 100;
                            _model.CurrentProgress = 0;
                            _model.Status = "Download Complete";
                        });

                        ServiceLocator.ProgressService.CompleteTask();

                    }
                }



            }
        }


        public async Task<LiteModelCollection?> LoadCivitaiModels()
        {
            var path = Path.Combine(AppDir, "models.json");

            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);

                var options = new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Converters = { new JsonStringEnumConverter() }
                };

                return JsonSerializer.Deserialize<LiteModelCollection>(json, options);
            }

            return new LiteModelCollection();
        }


        public async Task<LiteModelCollection> FetchCivitaiModels(CancellationToken token)
        {
            using var civitai = new CivitaiClient();

            var collection = new LiteModelCollection();

            Dispatcher.Invoke(() =>
            {
                _model.Status = $"Downloading models from Civitai...";
            });

            var totalModels = 0;

            var results = await GetNextPage(civitai, civitai.BaseUrl + "/models?limit=100&page=1&types=Checkpoint", token);

            if (results != null)
            {
                collection.Models.AddRange(results.Items);

                while (!string.IsNullOrEmpty(results.Metadata.NextPage))
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    //var nextPage = results.Metadata.CurrentPage + 1;
                    //var totalPages = results.Metadata.TotalPages;

                    Dispatcher.Invoke(() =>
                    {
                        _model.Status = $"Downloading models from Civitai... {collection.Models.Count()}";
                    });

                    results = await GetNextPage(civitai, results.Metadata.NextPage, token);

                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    if (results != null)
                    {
                        collection.Models.AddRange(results.Items);
                    }
                }

                //while (results.Metadata.CurrentPage < results.Metadata.TotalPages)
                //{
                //    if (token.IsCancellationRequested)
                //    {
                //        break;
                //    }

                //    var nextPage = results.Metadata.CurrentPage + 1;
                //    var totalPages = results.Metadata.TotalPages;

                //    Dispatcher.Invoke(() =>
                //    {
                //        _model.TotalProgress = totalPages;
                //        _model.CurrentProgress = nextPage;
                //        _model.Status = $"Downloading set {nextPage} of {totalPages:n0}...";
                //    });

                //    results = await GetPage(civitai, nextPage, token);

                //    if (token.IsCancellationRequested)
                //    {
                //        break;
                //    }

                //    if (results != null)
                //    {
                //        collection.Models.AddRange(results.Items);
                //    }
                //}
            }


            return collection;
        }

        private async Task<Results<LiteModel>?> GetNextPage(CivitaiClient client, string nextPageUrl, CancellationToken token)
        {
            return await client.GetLiteModels(nextPageUrl, token);
        }


        private async Task<Results<LiteModel>?> GetPage(CivitaiClient client, int page, CancellationToken token)
        {
            return await client.GetLiteModelsAsync(new ModelSearchParameters()
            {
                Page = page,
                Limit = 100,
                Types = new List<ModelType>() { ModelType.Checkpoint }
            }, token);
        }
    }
}
