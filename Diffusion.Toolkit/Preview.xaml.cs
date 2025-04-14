using Diffusion.Database;
using Diffusion.Toolkit.Classes;
using Diffusion.Toolkit.Controls;
using Diffusion.Toolkit.Models;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Diffusion.Toolkit.Services;

namespace Diffusion.Toolkit
{
    /// <summary>
    /// Interaction logic for Preview.xaml
    /// </summary>
    public partial class PreviewWindow : BorderlessWindow
    {
        private DataStore _dataStore => ServiceLocator.DataStore;
        private PreviewModel _model;
        private Action _onNext;

        public Action<string> OnDrop { get; set; }

        //public Action<int> Changed { get; set; }
        public Action AdvanceSlideShow { get; set; }

        protected override void OnSourceInitialized(EventArgs e)
        {
            PreviewPane.SetFocus();
            base.OnSourceInitialized(e);
        }

        public PreviewWindow()
        {
            _model = new PreviewModel();
            InitializeComponent();
            DataContext = _model;

            _model.Close = new RelayCommand<object>(o =>
            {
                Close();
            });

            PreviewPane.IsPopout = true;

            ServiceLocator.TaggingService.TagUpdated += (sender, arguments) =>
            {
                //Changed?.Invoke(arguments.Id);
            };

            var mainModel = ServiceLocator.MainModel;

            _model.ToggleFitToPreview = mainModel.ToggleFitToPreview;
            _model.ToggleActualSize = mainModel.ToggleActualSize;
            _model.ToggleAutoAdvance = mainModel.ToggleAutoAdvance;
            _model.ToggleInfo = mainModel.ToggleInfoCommand;

            //_slideShowDelay = mainModel.Settings.SlideShowDelay;
            _model.ToggleFullScreen = new RelayCommand<object>((o) => ToggleFullScreen());
            _model.StartStopSlideShow = new RelayCommand<object>((o) => StartStopSlideShow());

            Closing += OnClosing;
        }

        private void RestartSlideShowTimer()
        {
            if (_slideShowTimer != null && _model.SlideShowActive)
            {
                _slideShowTimer.Change(TimeSpan.FromSeconds(_slideShowDelay), TimeSpan.FromSeconds(_slideShowDelay));
            }
        }

        private void OnClosing(object? sender, CancelEventArgs e)
        {
            _slideShowTimer?.Dispose();
        }

        private Timer? _slideShowTimer = null;
        private int _slideShowDelay => ServiceLocator.Settings.SlideShowDelay;

        private void SlideShowAdvance(object? state)
        {
            Dispatcher.Invoke(() =>
            {
                AdvanceSlideShow?.Invoke();
            });
        }

        private void StartStopSlideShow()
        {
            if (_slideShowTimer == null)
            {
                _slideShowTimer = new Timer(SlideShowAdvance, null, TimeSpan.FromSeconds(_slideShowDelay), TimeSpan.FromSeconds(_slideShowDelay));
                _model.SlideShowActive = true;

            }
            else
            {
                if (_model.SlideShowActive)
                {
                    _slideShowTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    _model.SlideShowActive = false;
                }
                else
                {
                    _slideShowTimer.Change(TimeSpan.FromSeconds(_slideShowDelay), TimeSpan.FromSeconds(_slideShowDelay));
                    _model.SlideShowActive = true;
                }
            }
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
            if (e.Data.GetDataPresent(DataFormats.FileDrop) && !e.Data.GetDataPresent("DTCustomDragSource"))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                OnDrop?.Invoke(files[0]);
            }
        }

        private void PreviewPane_OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            //OnPreviewKeyUp(e);

            SetFocus();
        }

        public void SetFocus()
        {
            PreviewPane.SetFocus();
        }

        private void PreviewPane_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            RestartSlideShowTimer();

            //OnPreviewKeyDown(e);


            SetFocus();
        }

        public void LoadImage(ThumbnailViewModel thumbnail)
        {
            _model.CurrentImage = thumbnail.CurrentImage;
        }

        private void PreviewPane_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void Play_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            StartStopSlideShow();
        }

        private void Star_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            ServiceLocator.MainModel.ShowTags = !ServiceLocator.MainModel.ShowTags;
        }

        private void AutoAdvance_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            ServiceLocator.MainModel.AutoAdvance = !ServiceLocator.MainModel.AutoAdvance;
        }

    }


}