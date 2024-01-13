using System;
using System.Collections.Generic;
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

namespace Diffusion.Toolkit
{
    public partial class MainWindow
    {
        public void LoadImageModels()
        {
            var imageModels = _dataStore.GetImageModels();

            _model.ImageModels = imageModels.Select(m => new ModelViewModel()
            {
                Name = m.Name ?? ResolveModelName(m.Hash),
                Hash = m.Hash,
                ImageCount = m.ImageCount
            }).Where(m=> !string.IsNullOrEmpty(m.Name) && !string.IsNullOrEmpty(m.Hash)).OrderBy(x => x.Name);

            _model.ImageModelNames = imageModels.Where(m => !string.IsNullOrEmpty(m.Name)).Select(m=>m.Name).OrderBy(x => x);

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

            var message = "This will download Civitai model info.\r\n\r\n" + "Are you sure you want to continue?";

            var result = await _messagePopupManager.ShowCustom(message, "Download Civitai models", PopupButtons.YesNo, 500, 250);

            if (result == PopupResult.Yes)
            {
                _progressCancellationTokenSource = new CancellationTokenSource();

                _model.IsBusy = true;

                try
                {
                    var collection = await FetchCivitaiModels(_progressCancellationTokenSource.Token);


                    if (_progressCancellationTokenSource.Token.IsCancellationRequested)
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

                    _model.IsBusy = false;
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
                _model.TotalProgress = 100;
                _model.CurrentProgress = 1;
                _model.Status = $"Downloading models from Civitai...";
            });

            var results = await GetPage(civitai, 1, token);

            if (results != null)
            {
                collection.Models.AddRange(results.Items);

                while (results.Metadata.CurrentPage < results.Metadata.TotalPages)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    var nextPage = results.Metadata.CurrentPage + 1;
                    var totalPages = results.Metadata.TotalPages;

                    Dispatcher.Invoke(() =>
                    {
                        _model.TotalProgress = totalPages;
                        _model.CurrentProgress = nextPage;
                        _model.Status = $"Downloading set {nextPage} of {totalPages:#,###,###}...";
                    });

                    results = await GetPage(civitai, nextPage, token);

                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    if (results != null)
                    {
                        collection.Models.AddRange(results.Items);
                    }
                }
            }


            return collection;
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
