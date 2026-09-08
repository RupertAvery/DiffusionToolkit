using Diffusion.Toolkit.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Diffusion.Toolkit.Models;

namespace Diffusion.Toolkit.Controls
{
    /// <summary>
    /// Interaction logic for ThumbnailPane.xaml
    /// </summary>
    public partial class ThumbnailPane : UserControl
    {
        public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(
            nameof(Model), 
            typeof(ThumbnailPaneModel), 
            typeof(ThumbnailPane), 
            new PropertyMetadata(default(ThumbnailPaneModel))
            );

        public static readonly DependencyProperty SelectedImageEntryProperty = DependencyProperty.Register(
            nameof(SelectedImageEntry), 
            typeof(ImageEntry), 
            typeof(ThumbnailPane), 
            new PropertyMetadata(default(ImageEntry))
            );

        public ThumbnailPaneModel Model
        {
            get => (ThumbnailPaneModel)GetValue(ModelProperty);
            set => SetValue(ModelProperty, value);
        }

        public ImageEntry SelectedImageEntry
        {
            get => (ImageEntry)GetValue(SelectedImageEntryProperty);
            set => SetValue(SelectedImageEntryProperty, value);
        }

        public Action ClearQuery { get; set; }

        public Action SearchImages { get; set; }
        
        public ThumbnailPane()
        {
            InitializeComponent();

            //Model.DataStore = _dataStore;
            Model.Page = 0;
            Model.Pages = 0;
            Model.TotalFiles = 100;
            Model.Images = new ObservableCollection<ImageEntry>();
            Model.SearchCommand = new RelayCommand<object>((o) => SearchImages());
            Model.ClearCommand = new RelayCommand<object>((o) => ClearQuery());

            //Model.Refresh = new RelayCommand<object>((o) => ReloadMatches());
            //Model.ToggleParameters = new RelayCommand<object>((o) => ToggleInfo());
            //Model.CopyFiles = new RelayCommand<object>((o) => CopyFiles());


            Model.FocusSearch = new RelayCommand<object>((o) => SearchTermTextBox.Focus());
            Model.ShowDropDown = new RelayCommand<object>((o) => SearchTermTextBox.IsDropDownOpen = true);
            Model.HideDropDown = new RelayCommand<object>((o) => SearchTermTextBox.IsDropDownOpen = false);

        }

        private Random r = new Random();
        private readonly string[] _searchHints = File.ReadAllLines("hints.txt").Where(s => !string.IsNullOrEmpty(s.Trim())).ToArray();

        private void GetRandomHint()
        {
            var randomHint = _searchHints[r.Next(_searchHints.Length)];
            Model.SearchHint = $"Search for {randomHint}";
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
    }
}
