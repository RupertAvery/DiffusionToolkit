using Diffusion.Database;
using Diffusion.Toolkit.Models;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Diffusion.Common;

namespace Diffusion.Toolkit
{
    public partial class MainWindow
    {
        private void ToggleAlbum()
        {
            _model.ShowAlbumPanel = !_model.ShowAlbumPanel;
            _search.SetShowAlbumPanel(_model.ShowAlbumPanel);
            _settings.ShowAlbumPanel = _model.ShowAlbumPanel;
        }

        private void ToggleHideNSFW()
        {
            _model.HideNSFW = !_model.HideNSFW;
            QueryBuilder.HideNFSW = _model.HideNSFW;
            _settings.HideNSFW = _model.HideNSFW;
            _search.SearchImages();
        }

        private void ToggleNSFWBlur()
        {
            _model.NSFWBlur = !_model.NSFWBlur;
            _settings.NSFWBlur = _model.NSFWBlur;
        }

        private void ToggleFitToPreview()
        {
            _model.FitToPreview = !_model.FitToPreview;
            _settings.FitToPreview = _model.FitToPreview;
        }

        private void ToggleInfo()
        {
            _search.ToggleInfo();
        }

        private void RemoveMarked(object obj)
        {
            if (_model.IsScanning)
            {
                return;
            }
            //var files = new List<ImagePath>();

            //for (var i = 1; i <= 100; i++)
            //{
            //    files.Add(new ImagePath() { Id = i, Path = $"File{i:000}.png" });
            //}

            var files = _dataStore.GetMarkedImagePaths().ToList();
            var count = 0;

            if (files.Count == 0)
            {
                _messagePopupManager.Show("There are no files to delete", "Empty recycle bin");
                return;
            }

            _scanCancellationTokenSource = new CancellationTokenSource();

            _messagePopupManager.Show("This will delete the files from your hard drive! Are you sure you want to continue?", "Empty recycle bin", PopupButtons.YesNo).ContinueWith(t =>
            {
                if (t.Result == PopupResult.Yes)
                {
                    Task.Run(async () =>
                    {
                        _model.IsScanning = true;

                        var cancelled = false;

                        try
                        {
                            Dispatcher.Invoke(() =>
                            {
                                _model.TotalFilesScan = files.Count;
                                _model.CurrentPositionScan = 0;
                            });

                            foreach (var imagePath in files)
                            {
                                if (_scanCancellationTokenSource.IsCancellationRequested)
                                {
                                    break;
                                }

                                try
                                {
                                    count++;

                                    var filename = Path.GetFileName(imagePath.Path);

                                    Dispatcher.Invoke(() =>
                                    {
                                        _model.Status = $"Deleting {filename}...";
                                        _model.CurrentPositionScan = count;
                                    });

                                    File.Delete(imagePath.Path);
                                    var dir = Path.GetDirectoryName(imagePath.Path);
                                    var fileName = Path.GetFileNameWithoutExtension(imagePath.Path);
                                    var textFilePath = Path.Join(dir, $"{fileName}.txt");

                                    File.Delete(imagePath.Path);
                                    if (File.Exists(textFilePath))
                                    {
                                        File.Delete(textFilePath);
                                    }

                                    _dataStore.DeleteImage(imagePath.Id);

                                }
                                catch (Exception e)
                                {
                                    Logger.Log($"Failed to delete {imagePath.Path}. \n\n {e.Message}");

                                    var result = await Dispatcher.Invoke(async () =>
                                    {
                                        return await _messagePopupManager.Show($"Failed to delete {imagePath.Path}. \n\n {e.Message}", "Error", PopupButtons.OkCancel);
                                    });

                                    if (result == PopupResult.Cancel)
                                    {
                                        cancelled = true;
                                        break;
                                    }

                                }
                            }


                            await Dispatcher.Invoke(async () =>
                            {
                                _model.TotalFilesScan = 100;
                                _model.CurrentPositionScan = 0;


                                if (cancelled || _scanCancellationTokenSource.IsCancellationRequested)
                                {
                                    await _messagePopupManager.Show($"The operation was cancelled.", "Empty recycle bin", PopupButtons.OK);
                                }

                                Toast($"{count} images were deleted", "Empty recycle bin");
                            });

                        }
                        finally
                        {
                            _model.IsScanning = false;

                            SetTotalFilesStatus();

                            _search.ReloadMatches();

                            //await _search.ReloadMatches();

                        }
                    });
                }
            });
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

        private async Task CancelScan()
        {
            var dialogResult = await _messagePopupManager.Show("Are you sure you want to cancel the operation?", "Cancel", PopupButtons.YesNo);

            if (dialogResult == PopupResult.Yes)
            {
                _scanCancellationTokenSource.Cancel();
            }
        }

        private void OnStateChanged(object? sender, EventArgs e)
        {
            _settings.WindowState = this.WindowState;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _settings.WindowSize = e.NewSize;
        }

        private void OnClosing(object? sender, CancelEventArgs e)
        {
            if (_settings.IsDirty())
            {
                _configuration.Save(_settings);
            }
        }


    }
}