﻿using Diffusion.IO;
using Diffusion.Toolkit.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Diffusion.Database;

namespace Diffusion.Toolkit
{
    public partial class MainWindow
    {
        private Task ScanUnavailable(UnavailableFilesModel options)
        {
            _progressCancellationTokenSource = new CancellationTokenSource();

            var token = _progressCancellationTokenSource.Token;

            return Task.Run(() =>
            {
                var candidateImages = new List<int>();

                Dispatcher.Invoke(() =>
                {
                    _model.IsBusy = true;
                });

                if (options.UseRootFolders)
                {
                    var rootFolders = options.ImagePaths.Where(f => f.IsSelected);

                    var total = 0;

                    foreach (var folder in rootFolders)
                    {
                        total += _dataStore.CountAllPathImages(folder.Path);
                    }

                    Dispatcher.Invoke(() =>
                    {
                        _model.CurrentProgress = 0;
                        _model.TotalProgress = total;
                    });

                    var current = 0;

                    var scanning = GetLocalizedText("Actions.Scanning.Status");

                    foreach (var folder in rootFolders)
                    {
                        if (token.IsCancellationRequested)
                        {
                            break;
                        }

                        HashSet<string> ignoreFiles = new HashSet<string>();

                        var folderImages = _dataStore.GetAllPathImages(folder.Path).ToDictionary(f => f.Path);

                        if (Directory.Exists(folder.Path))
                        {
                            //var filesOnDisk = MetadataScanner.GetFiles(folder.Path, _settings.FileExtensions, null, _settings.RecurseFolders.GetValueOrDefault(true), null);
                            var filesOnDisk = MetadataScanner.GetFiles(folder.Path, _settings.FileExtensions, ignoreFiles, _settings.RecurseFolders.GetValueOrDefault(true), _settings.ExcludePaths);

                            foreach (var file in filesOnDisk)
                            {
                                if (token.IsCancellationRequested)
                                {
                                    break;
                                }

                                if (folderImages.ContainsKey(file))
                                {
                                    folderImages.Remove(file);
                                }

                                current++;

                                if (current % 113 == 0)
                                {

                                    Dispatcher.Invoke(() =>
                                    {
                                        _model.CurrentProgress = current;

                                        var status = scanning
                                            .Replace("{current}", $"{_model.CurrentProgress:#,###,##0}")
                                            .Replace("{total}", $"{_model.TotalProgress:#,###,##0}");

                                        _model.Status = status;
                                    });
                                }
                            }

                            foreach (var folderImage in folderImages)
                            {
                                candidateImages.Add(folderImage.Value.Id);
                            }
                        }
                        else
                        {
                            if (options.RemoveFromUnavailableRootFolders)
                            {
                                foreach (var folderImage in folderImages)
                                {
                                    candidateImages.Add(folderImage.Value.Id);

                                    current++;

                                    if (current % 113 == 0)
                                    {
                                        Dispatcher.Invoke(() =>
                                        {
                                            _model.CurrentProgress = current;

                                            var status = scanning
                                                .Replace("{current}", $"{_model.CurrentProgress:#,###,##0}")
                                                .Replace("{total}", $"{_model.TotalProgress:#,###,##0}");

                                            _model.Status = status;
                                        });
                                    }
                                }
                            }

                        }
                    }

                    Dispatcher.Invoke(() =>
                    {
                        _model.CurrentProgress = total;
                        _model.TotalProgress = total;

                        var unavailableFiles = GetLocalizedText("UnavailableFiles");

                        if (options.DeleteImmediately)
                        {
                            _dataStore.RemoveImages(candidateImages);

                            var removed = GetLocalizedText("UnavailableFiles.Results.Removed");

                            removed = removed.Replace("{count}", $"{candidateImages.Count:#,###,##0}");

                            _messagePopupManager.Show(removed, unavailableFiles, PopupButtons.OK);
                        }
                        else
                        {
                            _dataStore.SetDeleted(candidateImages, true);

                            var marked = GetLocalizedText("UnavailableFiles.Results.MarkedForDeletion");

                            marked = marked.Replace("{count}", $"{candidateImages.Count:#,###,##0}");

                            _messagePopupManager.Show(marked, unavailableFiles, PopupButtons.OK);
                        }

                        var completed = GetLocalizedText("Actions.Scanning.Completed");

                        _model.IsBusy = true;
                        _model.Status = completed;
                        _model.CurrentProgress = 0;
                        _model.TotalProgress = Int32.MaxValue;
                    });

                }


            });

        }


        private async Task RescanTask(object o)
        {
            if (_settings.ImagePaths.Any())
            {
                _ = Scan().ContinueWith((t) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        _search.ThumbnailListView.ReloadThumbnailsView(0);
                    });
                });
            }
            else
            {
                await _messagePopupManager.Show("No image paths configured!", "Rescan Folders");
                ShowSettings(null);
            }
        }


        private async Task RebuildTask(object o)
        {
            if (_settings.ImagePaths.Any())
            {
                var message = "This will update the metadata of ALL existing files in the database with current metadata in actual files.\r\n\r\n" +
                              "You only need to do this if you've updated the metadata in the files since they were added or if they contain metadata that an older version of this program didn't store.\r\n\r\n" +
                              "Are you sure you want to continue?";

                var result = await _messagePopupManager.ShowCustom(message, "Rebuild Metadata", PopupButtons.YesNo, 500, 400);
                if (result == PopupResult.Yes)
                {
                    await Rebuild();
                }
            }
            else
            {
                await _messagePopupManager.Show("No image paths configured!", "Rebuild Metadata");
                ShowSettings(null);
            }
        }

        private Task Scan()
        {
            _progressCancellationTokenSource = new CancellationTokenSource();

            return Task.Run(async () =>
            {
                var result = await ScanInternal(_settings!, false, true, _progressCancellationTokenSource.Token);
                if (result && _search != null)
                {
                    _search.SearchImages();
                }
            });
        }

        private async Task Rebuild()
        {
            _progressCancellationTokenSource = new CancellationTokenSource();

            await Task.Run(async () =>
            {
                var result = await ScanInternal(_settings!, true, true, _progressCancellationTokenSource.Token);
                if (result)
                {
                    _search.SearchImages();
                }
            });
        }

        private async void FixFolders()
        {
            var message = "This will fix scanned images missing in folders\r\n\r\n" +
                          "Are you sure you want to continue?";

            var result = await _messagePopupManager.ShowCustom(message, "Fix Folders", PopupButtons.YesNo, 500, 250);
            if (result == PopupResult.Yes)
            {
                var updated = await Task.Run(FixFoldersInternal);

                if (updated > 0)
                {
                    _search.SearchImages();
                }
            }
        }

        private int FixFoldersInternal()
        {
            int progress = 0;
            int updated = 0;

            var images = _dataStore.GetImagePaths().ToList();
            var folders = _dataStore.GetFolders();

            var folderIdCache = folders.ToDictionary(f => f.Path, f => f.Id);

            Dispatcher.Invoke(() =>
            {
                _model.Status = "Fixing Folders...s";
                _model.TotalProgress = images.Count;
                _model.CurrentProgress = 0;
            });

            foreach (var image in images)
            {
                var dirName = Path.GetDirectoryName(image.Path);

                var skip = false;

                if (folderIdCache.TryGetValue(dirName, out var folderId))
                {
                    if (image.FolderId == folderId)
                    {
                        skip = true;
                    }
                }

                if (!skip)
                {
                    _dataStore.UpdateImageFolderId(image.Id, image.Path, folderIdCache);
                    updated++;
                }

                var currentProgress = progress;
                if (currentProgress % 113 == 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        _model.CurrentProgress = currentProgress;
                        _model.Status = $"Checking {_model.CurrentProgress:#,###,###} of {_model.TotalProgress:#,###,###}...";
                    });
                }

                progress++;
            }


            Dispatcher.Invoke(() =>
            {
                _model.Status = "";
                _model.TotalProgress = Int32.MaxValue;
                _model.CurrentProgress = 0;
                Toast($"{updated} files were updated.", "Fix folders");
            });

            return updated;
        }

        private async Task MoveFiles(ICollection<ImageEntry> images, string targetPath, bool remove)
        {

            foreach (var watcher in _watchers)
            {
                watcher.EnableRaisingEvents = false;
            }

            Dispatcher.Invoke(() =>
            {
                _model.TotalProgress = images.Count;
                _model.CurrentProgress = 0;
            });

            var moved = 0;

            var folderIdCache = new Dictionary<string, int>();

            foreach (var image in images)
            {

                string newPath = "";
                string newTxtPath = "";
                string fileName = "";
                string fileNameOnly = "";
                string extension = "";
                int increment = 0;

                var directoryName = Path.GetDirectoryName(image.Path);


                string originalFileNameOnly = Path.GetFileNameWithoutExtension(image.Path);

                fileName = Path.GetFileName(image.Path);
                extension = Path.GetExtension(image.Path);

                var txtFileName = $"{originalFileNameOnly}.txt";
                var txtPath = Path.Join(directoryName, txtFileName);

                newPath = Path.Join(targetPath, fileName);
                newTxtPath = Path.Join(targetPath, txtFileName);

                // append number if file exists at target 
                while (File.Exists(newPath))
                {
                    increment++;
                    fileNameOnly = $"{originalFileNameOnly} ({increment})";
                    fileName = $"{fileNameOnly}{extension}";
                    txtFileName = $"{fileNameOnly}.txt";

                    newPath = Path.Join(targetPath, fileName);
                    newTxtPath = Path.Join(targetPath, txtFileName);
                };


                if (image.Path != newPath)
                {
                    File.Move(image.Path, newPath);

                    if (File.Exists(txtPath))
                    {
                        File.Move(txtPath, newTxtPath);
                    }

                    if (remove)
                    {
                        _dataStore.DeleteImage(image.Id);
                    }
                    else
                    {
                        _dataStore.MoveImage(image.Id, newPath, folderIdCache);
                    }

                    var moved1 = moved;
                    if (moved % 113 == 0)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            image.Path = newPath;
                            _model.CurrentProgress = moved1;
                            _model.Status = $"Moving {_model.CurrentProgress:#,###,###} of {_model.TotalProgress:#,###,###}...";
                        });
                    }

                    moved++;
                }
                else
                {
                    _model.TotalProgress--;
                }
            }


            await Dispatcher.Invoke(async () =>
            {
                _model.Status = $"Moving {_model.TotalProgress:#,###,###} of {_model.TotalProgress:#,###,###}...";
                _model.TotalProgress = Int32.MaxValue;
                _model.CurrentProgress = 0;
                Toast($"{moved} files were moved.", "Move images");
            });

            //await _search.ReloadMatches();

            foreach (var watcher in _watchers)
            {
                watcher.EnableRaisingEvents = true;
            }


        }


        private (int, float) ScanFiles(IList<string> filesToScan, bool updateImages, CancellationToken cancellationToken)
        {
            var added = 0;
            var scanned = 0;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var max = filesToScan.Count;

            Dispatcher.Invoke(() =>
            {
                _model.TotalProgress = max;
                _model.CurrentProgress = 0;
            });

            var folderIdCache = new Dictionary<string, int>();

            var newImages = new List<Image>();

            var includeProperties = new List<string>();

            if (_settings.AutoTagNSFW)
            {
                includeProperties.Add(nameof(Image.NSFW));
            }

            var scanning = GetLocalizedText("Actions.Scanning.Status");

            foreach (var file in MetadataScanner.Scan(filesToScan))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                scanned++;

                if (file != null)
                {
                    var fileInfo = new FileInfo(file.Path);

                    var image = new Image()
                    {
                        Prompt = file.Prompt,
                        NegativePrompt = file.NegativePrompt,
                        Path = file.Path,
                        FileName = fileInfo.Name,
                        Width = file.Width,
                        Height = file.Height,
                        ModelHash = file.ModelHash,
                        Model = file.Model,
                        Steps = file.Steps,
                        Sampler = file.Sampler,
                        CFGScale = file.CFGScale,
                        Seed = file.Seed,
                        BatchPos = file.BatchPos,
                        BatchSize = file.BatchSize,
                        CreatedDate = fileInfo.CreationTime,
                        ModifiedDate = fileInfo.LastWriteTime,
                        AestheticScore = file.AestheticScore,
                        HyperNetwork = file.HyperNetwork,
                        HyperNetworkStrength = file.HyperNetworkStrength,
                        ClipSkip = file.ClipSkip,
                        FileSize = file.FileSize,
                        NoMetadata = file.NoMetadata
                    };

                    if (!string.IsNullOrEmpty(file.HyperNetwork) && !file.HyperNetworkStrength.HasValue)
                    {
                        file.HyperNetworkStrength = 1;
                    }

                    if (_settings.AutoTagNSFW)
                    {
                        if (_settings.NSFWTags.Any(t => image.Prompt != null && image.Prompt.ToLower().Contains(t.Trim().ToLower())))
                        {
                            image.NSFW = true;
                        }
                    }

                    newImages.Add(image);
                }

                if (newImages.Count == 100)
                {
                    if (updateImages)
                    {

                        added += _dataStore.UpdateImagesByPath(newImages, includeProperties, folderIdCache, cancellationToken);
                    }
                    else
                    {
                        _dataStore.AddImages(newImages, includeProperties, folderIdCache, cancellationToken);
                        added += newImages.Count;
                    }

                    newImages.Clear();
                }

                if (scanned % 33 == 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        _model.CurrentProgress = scanned;

                        var text = scanning
                            .Replace("{current}", $"{_model.CurrentProgress:#,###,##0}")
                            .Replace("{total}", $"{_model.TotalProgress:#,###,##0}");

                        _model.Status = text;
                    });
                }
            }

            if (newImages.Count > 0)
            {
                if (updateImages)
                {
                    added += _dataStore.UpdateImagesByPath(newImages, includeProperties, folderIdCache, cancellationToken);
                }
                else
                {
                    _dataStore.AddImages(newImages, includeProperties, folderIdCache, cancellationToken);
                    added += newImages.Count;
                }
            }

            Dispatcher.Invoke(() =>
            {
                if (_model.TotalProgress > 0)
                {
                    var text = scanning
                        .Replace("{current}", $"{_model.TotalProgress:#,###,##0}")
                        .Replace("{total}", $"{_model.TotalProgress:#,###,##0}");

                    _model.Status = text;
                }
                _model.TotalProgress = Int32.MaxValue;
                _model.CurrentProgress = 0;
            });

            stopwatch.Stop();

            var elapsedTime = stopwatch.ElapsedMilliseconds / 1000f;


            return (added, elapsedTime);
        }


        private async Task<bool> ScanInternal(IScanOptions settings, bool updateImages, bool reportIfNone, CancellationToken cancellationToken)
        {
            if (_model.IsBusy) return false;

            _model.IsBusy = true;

            var removed = 0;
            var added = 0;

            Dispatcher.Invoke(() =>
            {
                _model.Status = GetLocalizedText("Actions.Scanning.BeginScanning");
            });

            try
            {
                var existingImages = _dataStore.GetImagePaths().ToList();

                Dispatcher.Invoke(() =>
                {
                    _model.Status = GetLocalizedText("Actions.Scanning.CheckRemoved");
                });

                var filesToScan = new List<string>();

                var gatheringFilesMessage = GetLocalizedText("Actions.Scanning.GatheringFiles");

                foreach (var path in settings.ImagePaths)
                {
                    if (_progressCancellationTokenSource.IsCancellationRequested)
                    {
                        break;
                    }

                    if (Directory.Exists(path))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            _model.Status = gatheringFilesMessage.Replace("{path}", path);
                        });

                        var ignoreFiles = updateImages ? null : existingImages.Where(p => p.Path.StartsWith(path)).Select(p => p.Path).ToHashSet();

                        filesToScan.AddRange(MetadataScanner.GetFiles(path, settings.FileExtensions, ignoreFiles, settings.RecurseFolders.GetValueOrDefault(true), settings.ExcludePaths).ToList());
                    }
                }

                var (_added, elapsedTime) = ScanFiles(filesToScan, updateImages, cancellationToken);

                added = _added;

                if ((added + removed == 0 && reportIfNone) || added + removed > 0)
                {
                    Report(added, removed, elapsedTime, updateImages);
                }
            }
            catch (Exception ex)
            {
                await _messagePopupManager.ShowMedium(
                    ex.Message,
                    "Scan Error", PopupButtons.OK);
            }
            finally
            {
                _model.IsBusy = false;

                Dispatcher.Invoke(() =>
                {
                    _model.Status = GetLocalizedText("Actions.Scanning.Completed");
                });
            }


            return added + removed > 0;
        }

        private void Report(int added, int removed, float elapsedTime, bool updateImages)
        {
            Dispatcher.Invoke(() =>
            {
                var scanComplete = GetLocalizedText("Actions.Scanning.ScanComplete.Caption");

                if (added == 0 && removed == 0)
                {
                    var message = GetLocalizedText("Actions.Scanning.NoNewImages.Toast");
                    Toast(message, scanComplete);
                }
                else
                {
                    var updatedMessage = GetLocalizedText("Actions.Scanning.ImagesUpdated.Toast");
                    var addedMessage = GetLocalizedText("Actions.Scanning.ImagesAdded.Toast");
                    var removedMessage = GetLocalizedText("Actions.Scanning.MissingImagesRemoved.Toast");

                    updatedMessage = updatedMessage.Replace("{count}", $"{added:#,###,##0}");
                    addedMessage = addedMessage.Replace("{count}", $"{added:#,###,##0}");
                    removedMessage = removedMessage.Replace("{count}", $"{removed:#,###,##0}");

                    var newOrOpdated = updateImages ? updatedMessage : addedMessage;

                    var missing = removed > 0 ? removedMessage : string.Empty;

                    var messages = new[] { newOrOpdated, missing };

                    var message = string.Join("\n", messages.Where(m => !string.IsNullOrEmpty(m)));

                    message = $"{message}";

                    if (updateImages)
                    {
                        var rebuildComplete = GetLocalizedText("Actions.Scanning.RebuildComplete.Caption");

                        Toast(message, rebuildComplete);
                    }
                    else
                    {
                        Toast(message, scanComplete, 10);
                    }
                }

                SetTotalFilesStatus();
            });

        }

        private async void CleanExcludedPaths()
        {
            var message = "This will remove any remaining images in excluded folders from the database. The images on disk will not be deleted.\r\n\r\n" +
                          "Are you sure you want to continue?";

            var result = await _messagePopupManager.ShowCustom(message, "Remove Excluded Images", PopupButtons.YesNo, 500, 250);
            if (result == PopupResult.Yes)
            {
                var total = CleanExcludedPaths(_settings.ExcludePaths);

                Toast($"{total} images removed from database", "");
            }
        }

        private int CleanExcludedPaths(IEnumerable<string> excludedPaths)
        {
            int total = 0;

            foreach (var excludedPath in excludedPaths)
            {
                var folder = _dataStore.GetFolder(excludedPath);
                if (folder != null)
                {
                    var images = _dataStore.GetFolderImages(folder.Id).ToList();
                    _dataStore.RemoveImages(images.Select(i => i.Id));
                    total += images.Count();
                }
            }

            return total;
        }

        private async Task CleanRemovedFolders(object o)
        {
            var message = "This will remove any remaining images in removed folders from the database. The images on disk will not be deleted.\r\n\r\n" +
                          "Are you sure you want to continue?";

            var result = await _messagePopupManager.ShowCustom(message, "Clean Removed Folders", PopupButtons.YesNo, 500, 250);
            if (result == PopupResult.Yes)
            {
                CleanRemovedFoldersInternal();
            }
        }

        private void CleanRemovedFoldersInternal()
        {
            var total = _dataStore.CleanRemovedFolders(_settings.ImagePaths);

            if (total > 0)
            {
                _search.ReloadMatches(null);

                Toast($"{total} images removed from database", "");
            }
        }

        private void SetTotalFilesStatus()
        {
            var total = _dataStore.GetTotal();
            _model.Status = $"{total:###,###,##0} images in database";
        }

        private void SortAlbums_OnClick(object sender, RoutedEventArgs e)
        {
            SortAlbums();
        }

        private void SortAlbums()
        {
            var window = new AlbumSortWindow(_dataStore, _settings);
            window.Owner = this;
            window.ShowDialog();
            LoadAlbums();
        }
    }
}