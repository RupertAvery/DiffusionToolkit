using Diffusion.Database;
using Diffusion.Toolkit.Models;
using System;
using System.Windows;

namespace Diffusion.Toolkit
{
    /// <summary>
    /// Interaction logic for Preview.xaml
    /// </summary>
    public partial class PreviewWindow : Window
    {
        private readonly DataStore _dataStore;
        private PreviewModel _model;
        private Action _onNext;

        public Action OnNext
        {
            set => PreviewPane.OnNext = value;
        }

        public Action OnPrev
        {
            set => PreviewPane.OnPrev = value;
        }

        public Action<string> OnDrop { get; set; }

        public Action<int> Changed { get; set; }

        public PreviewWindow(DataStore dataStore, MainModel mainModel)
        {
            _dataStore = dataStore;
            _model = new PreviewModel();
            InitializeComponent();
            DataContext = _model;

            PreviewPane.IsPopout = true;
            PreviewPane.NSFW = (id, v) =>
            {
                _dataStore.SetNSFW(id, v);
                Changed?.Invoke(id);
            };
            PreviewPane.Favorite = (id, v) =>
            {
                _dataStore.SetFavorite(id, v);
                Changed?.Invoke(id);
            };
            PreviewPane.Rate = (id, v) =>
            {
                _dataStore.SetRating(id, v);
                Changed?.Invoke(id);
            }; 
            PreviewPane.Delete = (id, v) =>
            {
                _dataStore.SetDeleted(id, v);
                Changed?.Invoke(id);
            };
            PreviewPane.MainModel = mainModel;
        }

        public void SetNSFWBlur(bool value)
        {
            _model.NSFWBlur = value;
        }

        public void SetCurrentImage(ImageViewModel? value)
        {
            _model.CurrentImage = value;
        }


        private void PreviewPane_OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                OnDrop?.Invoke(files[0]);
            }
        }

    }

    
}
