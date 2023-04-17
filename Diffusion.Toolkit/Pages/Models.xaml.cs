using System;
using Diffusion.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Diffusion.Database;
using Model = Diffusion.Common.Model;
using Path = System.IO.Path;
using System.Security.Policy;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Diffusion.Toolkit.Pages
{
    /// <summary>
    /// Interaction logic for Models.xaml
    /// </summary>
    public partial class Models : Page
    {
        private readonly Settings _settings;

        private ModelsModel _model;

        public Models(IOptions<DataStore> dataStoreOptions, Settings settings)
        {
            _settings = settings;

            InitializeComponent();

            _model = new ModelsModel();
            _model.PropertyChanged += ModelOnPropertyChanged;

            DataContext = _model;
        }

        public Action<Model> OnModelUpdated { get; set; }

        private void ModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ModelsModel.Search))
            {
                if (!string.IsNullOrEmpty(_model.Search))
                {
                    var query = _model.Search.ToLower();
                    _model.FilteredModels = _model.Models.Where(m =>
                        m.Filename.ToLower().Contains(query) ||
                        m.Hash.ToLower().Contains(query) ||
                        (!string.IsNullOrEmpty(m.SHA256) && m.SHA256.ToLower().Contains(query))
                        );
                }
                else
                {
                    _model.FilteredModels = _model.Models.ToList();
                }
            }
        }

        public void SetModels(ICollection<Model> modelsCollection)
        {
            _model.Models = modelsCollection.Select(m => new ModelViewModel
            {
                Path = m.Path,
                Filename = m.Filename,
                Hash = m.Hash,
                SHA256 = m.SHA256,
            }).ToList();

            _model.FilteredModels = _model.Models;
        }


        private void ComputeSHA256_Click(object sender, RoutedEventArgs e)
        {
            var model = (ModelViewModel)((MenuItem)sender).DataContext;
            var path = Path.Combine(_settings.ModelRootPath, model.Path);
            model.SHA256 = "Calculating...";

            Task.Run(() =>
            {
                var hash = HashFunctions.CalculateSHA256(path);
                Dispatcher.Invoke(() =>
                {
                    model.SHA256 = hash;
                    OnModelUpdated?.Invoke(new Model() { Path = model.Path, SHA256 = hash });
                    if (!string.IsNullOrEmpty(_settings.HashCache) && File.Exists(_settings.HashCache))
                    {
                        var hashes = JsonSerializer.Deserialize<Hashes>(File.ReadAllText(_settings.HashCache));

                        var info = new FileInfo(path);
                        var baseTime = new DateTime(1970, 1, 1, 0, 0, 0);

                        var mTime = info.LastWriteTime - baseTime;

                        var key = "checkpoint/" + model.Path;

                        if (hashes.hashes.TryGetValue(key, out var hashInfo))
                        {
                            hashInfo.sha256 = hash;
                            hashInfo.mtime = mTime.TotalSeconds;
                        }
                        else
                        {
                            hashes.hashes.Add(key, new HashInfo()
                            {
                                sha256 = hash,
                                mtime = mTime.TotalSeconds
                            });
                        }

                        var json = JsonSerializer.Serialize(hashes, new JsonSerializerOptions()
                        {
                            WriteIndented = true,
                            
                        });

                        File.WriteAllText(_settings.HashCache, json);
                    }

                });
            });
        }

    }
}
