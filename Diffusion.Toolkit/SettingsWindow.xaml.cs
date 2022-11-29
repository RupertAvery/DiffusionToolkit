using Diffusion.Database;
using Diffusion.IO;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace Diffusion.Toolkit
{
    public class SettingsModel : BaseNotify
    {
        private string _modelRootPath;
        private ObservableCollection<string> _imagePaths;
        private int _selectedIndex;

        public SettingsModel()
        {
            _imagePaths = new ObservableCollection<string>();
        }

        public ObservableCollection<string> ImagePaths
        {
            get => _imagePaths;
            set => SetField(ref _imagePaths, value);
        }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set => SetField(ref _selectedIndex, value);
        }

        public string ModelRootPath
        {
            get => _modelRootPath;
            set => SetField(ref _modelRootPath, value);
        }
    }

    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly DataStore _dataStore;
        private readonly SettingsModel _model;

        public SettingsWindow()
        {
            InitializeComponent();

        }

        public SettingsWindow(DataStore dataStore, Settings settings)  :this()
        {
            _dataStore = dataStore;
            
            _model = new SettingsModel();
            _model.ImagePaths = new ObservableCollection<string>(settings.ImagePaths);
            _model.ModelRootPath = settings.ModelRootPath;

            DataContext = _model;
            
            Closing += (sender, args) =>
            {
                settings.ImagePaths = _model.ImagePaths.ToList();
                settings.ModelRootPath = _model.ModelRootPath;
            };
        }

        private void AddFolder_OnClick(object sender, RoutedEventArgs e)
        {
            using var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            dialog.ShowDialog(this);
            if (dialog.ShowDialog(this) == CommonFileDialogResult.Ok)
            {
                _model.ImagePaths.Add(dialog.FileName);
            }

        }

        private void RemoveFolder_OnClick(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(this,
                "Are you sure you want to remove this folder? This will also remove images in the database belonging to this folder.",
                "Remove folder", MessageBoxButton.YesNo,
                MessageBoxImage.Question, MessageBoxResult.No);

            if (result == MessageBoxResult.Yes)
            {
                _model.ImagePaths.RemoveAt(_model.SelectedIndex);
            }
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BrowseModelPath_OnClick(object sender, RoutedEventArgs e)
        {
            using var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if(dialog.ShowDialog(this) == CommonFileDialogResult.Ok)
            {
                _model.ModelRootPath = dialog.FileName;
            }
        }
    }
}
