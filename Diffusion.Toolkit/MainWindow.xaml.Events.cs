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
using Diffusion.Toolkit.Classes;
using Diffusion.Toolkit.Controls;

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

        private void InitEvents()
        {
            _model.ReleaseNotesCommand = new RelayCommand<object>((o) => ShowReleaseNotes());
            _model.HelpCommand = new RelayCommand<object>((o) => ShowTips());
            _model.ToggleInfoCommand = new RelayCommand<object>((o) => ToggleInfo());

            _model.ToggleNSFWBlurCommand = new RelayCommand<object>((o) => ToggleNSFWBlur());

            _model.ToggleHideNSFW = new RelayCommand<object>((o) => ToggleHideNSFW());
            _model.ToggleHideDeleted = new RelayCommand<object>((o) => ToggleHideDeleted());
            _model.ToggleHideUnavailable = new RelayCommand<object>((o) => ToggleHideUnavailable());

            _model.ToggleFitToPreview = new RelayCommand<object>((o) => ToggleFitToPreview());
            _model.ToggleActualSize = new RelayCommand<object>((o) => ToggleActualSize());

            _model.ToggleAutoAdvance = new RelayCommand<object>((o) => ToggleAutoAdvance());
            _model.ToggleTagsCommand = new RelayCommand<object>((o) => ToggleTags());
            _model.ToggleNotificationsCommand = new RelayCommand<object>((o) => ToggleNotifications());

            _model.NavigateToParentFolderCommand = new RelayCommand<object>((o) => NavigateToParentFolder());

            _model.SetThumbnailSize = new RelayCommand<object>((o) => SetThumbnailSize(int.Parse((string)o)));
            _model.TogglePreview = new RelayCommand<object>((o) => TogglePreview());
            _model.PoputPreview = new RelayCommand<object>((o) => PopoutPreview(true, true, false));

            _model.ToggleFilenamesCommand = new RelayCommand<object>((o) => ToggleFilenames());

            _model.ResetLayout = new RelayCommand<object>((o) => ResetLayout());

            _model.RescanResults = new RelayCommand<object>((o) => RescanResults());
            _model.AddAllToAlbum = new RelayCommand<object>((o) => AddAllToAlbum());
            _model.MarkAllForDeletion = new RelayCommand<object>((o) => MarkAllForDeletion());
            _model.UnmarkAllForDeletion = new RelayCommand<object>((o) => UnmarkAllForDeletion());
            _model.RemoveMatching = new RelayCommand<object>((o) => RemoveFromDatabase());
            _model.AutoTagNSFW = new RelayCommand<object>((o) => AutoTagNSFW());
            _model.DownloadCivitai = new RelayCommand<object>((o) => DownloadCivitaiModels());

            _model.FixFoldersCommand = new RelayCommand<object>((o) => FixFolders());
            _model.RemoveExcludedImagesCommand = new RelayCommand<object>((o) => CleanExcludedPaths());
            _model.CleanRemovedFoldersCommand = new AsyncCommand<object>(CleanRemovedFolders);

            _model.UnavailableFilesCommand = new AsyncCommand<object>(UnavailableFiles);

            _model.ShowFilterCommand = new RelayCommand<object>((o) => _search?.ShowFilter());
            _model.ToggleAutoRefresh = new RelayCommand<object>((o) => ToggleAutoRefresh());

            _model.SortAlbumCommand = new RelayCommand<object>((o) => SortAlbums());
            _model.ClearAlbumsCommand = new RelayCommand<object>((o) => ClearAlbums());
            _model.ClearModelsCommand = new RelayCommand<object>((o) => ClearModels());

            _model.ToggleNavigationPane = new RelayCommand<object>((o) => ToggleNavigationPane());
            _model.ToggleVisibilityCommand = new RelayCommand<string>((p) => ToggleVisibility(p));
            _model.ToggleThumbnailViewModeCommand = new RelayCommand<ThumbnailViewMode>((p) => ToggleThumbnailViewMode(p));
            _model.FocusSearch = new RelayCommand<object>((p) => OpenQueryBar());
            _model.ShowInExplorerCommand = new RelayCommand<FolderViewModel>((p) => ShowInExplorer(p));
        }

        private void ToggleThumbnailViewMode(ThumbnailViewMode thumbnailViewMode)
        {
            _settings.ThumbnailViewMode = thumbnailViewMode;
        }

        // TODO: Move these to the property changed handlers and trigger on setting change

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
            _settings.NSFWBlur = !_settings.NSFWBlur;
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

        private void ToggleAutoAdvance()
        {
            _settings.AutoAdvance = !_settings.AutoAdvance;
        }

        private void ToggleFilenames()
        {
            _settings.ShowFilenames = !_settings.ShowFilenames;
        }

        private void ToggleTags()
        {
            _model.ShowTags = !_model.ShowTags;
            _model.Settings.ShowTags = _model.ShowTags;
        }

        private void ToggleInfo()
        {
            _search.ToggleInfo();
        }

        private void ShowAbout()
        {
            //var welcome = new WelcomeWindow(_settings!);
            //welcome.Owner = this;
            //welcome.ShowDialog();
        }

        private void ShowReleaseNotes()
        {
            var window = new ReleaseNotesWindow();
            window.Owner = this;
            window.ShowDialog();
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
                case nameof(Settings.ThumbnailViewMode):
                    _model.ThumbnailViewMode = _settings.ThumbnailViewMode;
                    break;

                case nameof(Settings.NSFWBlur):
                    _model.NSFWBlur = _settings.NSFWBlur;
                    break;

                case nameof(Settings.AutoAdvance):
                    _model.AutoAdvance = _settings.AutoAdvance;
                    break;

                case nameof(Settings.AutoRefresh):
                    _model.AutoRefresh = _settings.AutoRefresh;
                    break;

                case nameof(Settings.ShowFilenames):
                    _model.ShowFilenames = _settings.ShowFilenames;
                    break;

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

        private async Task RenameImageEntry()
        {
            var selectedImageEntry = ServiceLocator.MainModel.SelectedImageEntry;

            if (selectedImageEntry == null) return;

            var id = selectedImageEntry.Id;
            var oldPath = selectedImageEntry.Path;
            var entryType = selectedImageEntry.EntryType;

            if (entryType == EntryType.File)
            {
                ServiceLocator.FileService.RenameFile(id, oldPath);
            }
            else if (entryType == EntryType.Folder)
            {
                var name = selectedImageEntry.FileName;

                var folder = new FolderViewModel()
                {
                    Id = id,
                    Name = name, 
                    Path = oldPath,
                };

                await ServiceLocator.FolderService.ShowRenameFolderDialog(folder);
            }
            
            //_search.ThumbnailListView.Focus();
            _search.ThumbnailListView.FocusCurrentItem();

        }

        public void NavigateToParentFolder()
        {
            ServiceLocator.FolderService.NavigateToParentFolder();
        }
    }

}

