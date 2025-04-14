using Diffusion.Database;
using Diffusion.Toolkit.Models;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Diffusion.Common;
using System.Windows.Forms;
using System.Windows.Interop;
using WPFLocalizeExtension.Providers;
using System.Collections.ObjectModel;
using Diffusion.Toolkit.Configuration;
using Diffusion.Toolkit.Localization;
using Diffusion.Toolkit.Services;
using Diffusion.Toolkit.Thumbnails;
using WPFLocalizeExtension.Engine;
using MessageBox = System.Windows.MessageBox;

namespace Diffusion.Toolkit
{


    public partial class MainWindow
    {
        static readonly CultureInfo DefaultCulture = CultureInfo.CurrentCulture;

        private async void OnActivated(object? sender, EventArgs e)
        {
            //await Task.Delay(500);

            //if (addedTotal > 0)
            //{
            //    ServiceLocator.ScanningService.Report(addedTotal, 0, 0, false, false, false);
            //    lock (_lock)
            //    {
            //        addedTotal = 0;
            //    }
            //}

        }
        private void ToggleHideNSFW()
        {
            _model.HideNSFW = !_model.HideNSFW;
            QueryBuilder.HideNSFW = _model.HideNSFW;
            _settings.HideNSFW = _model.HideNSFW;
            _search.SearchImages();

            //_prompts.ReloadPrompts();
            //_prompts.LoadImages();
            //LoadImageModels();
        }

        private void ToggleHideDeleted()
        {
            _model.HideDeleted = !_model.HideDeleted;
            QueryBuilder.HideDeleted = _model.HideDeleted;
            _settings.HideDeleted = _model.HideDeleted;
            _search.SearchImages();

            //_prompts.ReloadPrompts();
            //_prompts.LoadImages();
        }

        private void ToggleHideUnavailable()
        {
            _model.HideUnavailable = !_model.HideUnavailable;
            QueryBuilder.HideUnavailable = _model.HideUnavailable;
            _settings.HideUnavailable = _model.HideUnavailable;
            _search.SearchImages();

            //_prompts.ReloadPrompts();
            //_prompts.LoadImages();
        }

        private void ToggleNSFWBlur()
        {
            _model.NSFWBlur = !_model.NSFWBlur;
            _settings.NSFWBlur = _model.NSFWBlur;
        }

        private void ToggleFitToPreview()
        {
            _model.FitToPreview = !_model.FitToPreview;
            _model.ActualSize = false;
            _settings.FitToPreview = _model.FitToPreview;
            _settings.ActualSize = _model.ActualSize;
        }

        private void ToggleActualSize()
        {
            _model.ActualSize = !_model.ActualSize;
            _model.FitToPreview = false;
            _settings.FitToPreview = _model.FitToPreview;
            _settings.ActualSize = _model.ActualSize;
        }

        private void ToggleNotifications()
        {
            _model.ShowNotifications = !_model.ShowNotifications;
            _settings.ShowNotifications = _model.ShowNotifications;
        }

        private void ToggleInfo()
        {
            _search.ToggleInfo();
        }

       private void ShowAbout()
        {
            var welcome = new WelcomeWindow(_settings!);
            welcome.Owner = this;
            welcome.ShowDialog();
        }

        private void ShowTips()
        {
            if (!_tipsOpen)
            {
                var tipsWindow = new TipsWindow();
                tipsWindow.Owner = this;
                tipsWindow.Show();
                _tipsOpen = true;
                tipsWindow.Closed += (sender, args) =>
                {
                    _tipsOpen = false;
                };
            }
        }

        private void OnStateChanged(object? sender, EventArgs e)
        {
            _settings.WindowState = this.WindowState;
            if (this.WindowState == WindowState.Maximized)
            {
                Screen screen = Screen.FromHandle((new WindowInteropHelper(this)).Handle);
                _settings.Top = screen.WorkingArea.Top;
                _settings.Left = screen.WorkingArea.Left;
            }
            else if (this.WindowState == WindowState.Normal)
            {
                _settings.Top = this.Top;
                _settings.Left = this.Left;
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _settings.WindowSize = e.NewSize;
            _settings.Top = this.Top;
            _settings.Left = this.Left;
        }

        private void OnLocationChanged(object? sender, EventArgs e)
        {
            _settings.Top = this.Top;
            _settings.Left = this.Left;
        }

        private bool _isClosing;

        private async void OnClosing(object? sender, CancelEventArgs e)
        {
            if (ServiceLocator.MainModel.IsBusy)
            {
                var d = MessageBox.Show(this, "An operation is currently in progress. Are you sure you want to exit?", "Confirm close", MessageBoxButton.YesNo);
                if (d == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }


                e.Cancel = true;

                await Dispatcher.InvokeAsync(async () =>
                {
                    ServiceLocator.ProgressService.Cancel();
                    await ServiceLocator.ProgressService.WaitForCompletion();
                    Close();
                });
            }
            else
            {
                _isClosing = true;

                ServiceLocator.ThumbnailService.Stop();

                //var changes = _settingsPage.ApplySettings();

                //CleanupFolderChanges(changes);

                if (_settings.IsDirty())
                {
                    _configuration.Save(_settings);
                }
            }
        }


        private string GetLocalizedText(string key)
        {
            return (string)JsonLocalizationProvider.Instance.GetLocalizedObject(key, null, CultureInfo.InvariantCulture);
        }

        private void OnSettingsChanged(PropertyChangedEventArgs args)
        {
            // Don't do any database updates here
            switch (args.PropertyName)
            {
                case nameof(Settings.PermanentlyDelete):
                    _model.PermanentlyDelete = _settings.PermanentlyDelete;
                    break;

                case nameof(Settings.PageSize):
                    ThumbnailCache.CreateInstance(_settings.PageSize * 5, _settings.PageSize * 2);
                    _search.SetPageSize(_settings.PageSize);
                    _prompts.SetPageSize(_settings.PageSize);
                    _search.SearchImages();
                    break;

                case nameof(Settings.ModelRootPath):
                case nameof(Settings.HashCache):
                    LoadModels();
                    break;

                case nameof(Settings.Theme):
                    UpdateTheme(_settings.Theme);
                    break;

                case nameof(Settings.PortableMode):
                    if (_settings.PortableMode)
                    {
                        GoPortable();
                    }
                    else
                    {
                        GoLocal();
                    }
                    break;

                case nameof(Settings.Culture):
                    if (_settings.Culture == "default")
                    {
                        LocalizeDictionary.Instance.Culture = DefaultCulture;
                    }
                    else
                    {
                        LocalizeDictionary.Instance.Culture = new CultureInfo(_settings.Culture);
                    }
                    break;

            }
        }

        private async Task OpenWith(object sender, string? arg)
        {
            await ServiceLocator.ExternalApplicationsService.OpenWith(sender, int.Parse(arg));
        }

    }

}

