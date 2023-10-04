using Diffusion.Database;
using Diffusion.Toolkit.Classes;
using Diffusion.Toolkit.Models;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Diffusion.Toolkit
{
    /// <summary>
    /// Interaction logic for Preview.xaml
    /// </summary>
    public partial class PreviewWindow : BorderlessWindow
    {
        private readonly DataStore _dataStore;
        private PreviewModel _model;
        private Action _onNext;

        public Action<string> OnDrop { get; set; }

        public Action<int> Changed { get; set; }

        protected override void OnSourceInitialized(EventArgs e)
        {
            PreviewPane.SetFocus();
            base.OnSourceInitialized(e);
        }

        public PreviewWindow(DataStore dataStore, MainModel mainModel)
        {
            _dataStore = dataStore;
            _model = new PreviewModel();
            InitializeComponent();
            DataContext = _model;

            _model.Close = new RelayCommand<object>(o =>
            {
                Close();
            });
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
            _model.ToggleFitToPreview = PreviewPane.MainModel.ToggleFitToPreview;
            _model.ToggleInfo = PreviewPane.MainModel.ToggleInfo;
            _model.ToggleFullScreen = new RelayCommand<object>((o) => ToggleFullScreen());
        }

        private bool _isFullScreen = false;
        //private WindowState _lastWindowState;
        //private WindowStyle _lastWindowStyle;

        private Brush _background;

        private void ToggleFullScreen()
        {
            _isFullScreen = !_isFullScreen;
            if (_isFullScreen)
            {
                _background = this.BackgroundGrid.Background;
                BackgroundGrid.Background = new SolidColorBrush(Colors.Black);
            }
            else
            {
                BackgroundGrid.Background = _background;
            }
            SetFullScreen(_isFullScreen);
        }

        public void ShowFullScreen()
        {
            Show();
            _isFullScreen = false;
            ToggleFullScreen();
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

        private void PreviewPane_OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            OnPreviewKeyUp(e);
        }

        private void PreviewPane_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            OnPreviewKeyDown(e);
        }
    }

    
}