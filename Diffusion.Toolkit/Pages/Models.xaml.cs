using Diffusion.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
using Diffusion.Database;

namespace Diffusion.Toolkit.Pages
{
    public class ModelViewModel
    {
        public string Path { get; set; }
        public string Filename { get; set; }
        public string Hash { get; set; }
        public string Hashv2 { get; set; }
        public string DisplayName => $"{Filename} ({Hash.ToLower()})";
        public string HashInfo => $"Hash: {Hash.ToLower()} CRC32 Hash Sum: ({Hashv2.ToLower()})";
    }

    public class ModelsModel : BaseNotify
    {
        private IEnumerable<ModelViewModel> _models;
        private ModelViewModel _selectedModel;

        public IEnumerable<ModelViewModel> Models
        {
            get => _models;
            set => SetField(ref _models, value);
        }

        public ModelViewModel SelectedModel
        {
            get => _selectedModel;
            set => SetField(ref _selectedModel, value);
        }
    }

    /// <summary>
    /// Interaction logic for Models.xaml
    /// </summary>
    public partial class Models : Page
    {
        private readonly Settings _settings;

        private ModelsModel _model;

        public Models(DataStore dataStore, Settings settings)
        {
            _settings = settings;

            InitializeComponent();

            _model = new ModelsModel();

            LoadModels();

            DataContext = _model;
        }

        public void LoadModels()
        {
            if (_settings.ModelRootPath != null && Directory.Exists(_settings.ModelRootPath))
            {
                _model.Models = ModelScanner.Scan(_settings.ModelRootPath).OrderBy(m => m.Filename).Select(m => new ModelViewModel
                {
                    Hash = m.Hash,
                    Filename = m.Filename,
                    Hashv2 = m.Hashv2,
                    Path = m.Path
                }).ToList();
            }
        }
    }
}
