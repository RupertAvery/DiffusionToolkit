using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using Diffusion.Toolkit.Models;

namespace Diffusion.Toolkit.Pages
{
    /// <summary>
    /// Interaction logic for Prompts.xaml
    /// </summary>
    public partial class Prompts : Page
    {
        private readonly DataStore _dataStore;
        private PromptsModel _model;

        public Prompts(DataStore dataStore, Settings settings)
        {
            _dataStore = dataStore;
            InitializeComponent();

            _model = new PromptsModel();

            DataContext = _model;

            _model.Prompts = new ObservableCollection<UsedPrompt>(_dataStore.SearchPrompts(_model.PromptQuery, _model.FullTextPrompt, _model.PromptDistance));
            _model.NegativePrompts = new ObservableCollection<UsedPrompt>(_dataStore.SearchNegativePrompts(_model.NegativePromptQuery, _model.NegativeFullTextPrompt, _model.NegativePromptDistance));

            _model.PropertyChanged += ModelOnPropertyChanged;
        }

        private void ModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PromptsModel.FullTextPrompt) || e.PropertyName == nameof(PromptsModel.PromptDistance))
            {
                _model.Prompts = new ObservableCollection<UsedPrompt>(_dataStore.SearchPrompts(_model.PromptQuery, _model.FullTextPrompt, _model.PromptDistance));
            }
            if (e.PropertyName == nameof(PromptsModel.NegativeFullTextPrompt) || e.PropertyName == nameof(PromptsModel.NegativePromptDistance))
            {
                _model.NegativePrompts = new ObservableCollection<UsedPrompt>(_dataStore.SearchNegativePrompts(_model.NegativePromptQuery, _model.NegativeFullTextPrompt, _model.NegativePromptDistance));
            }
        }

        private void PromptQuery_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _model.Prompts = new ObservableCollection<UsedPrompt>(_dataStore.SearchPrompts(_model.PromptQuery, _model.FullTextPrompt, _model.PromptDistance));
            }
        }

        private void NegativePromptQuery_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _model.NegativePrompts = new ObservableCollection<UsedPrompt>(_dataStore.SearchNegativePrompts(_model.NegativePromptQuery, _model.NegativeFullTextPrompt, _model.NegativePromptDistance));
            }
        }
    }
}
