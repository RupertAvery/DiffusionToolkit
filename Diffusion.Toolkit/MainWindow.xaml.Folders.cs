using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using Diffusion.Common;
using Diffusion.Toolkit.Classes;
using Diffusion.Toolkit.Models;
using Diffusion.Toolkit.Services;
using Diffusion.Toolkit.Thumbnails;
using SQLite;
using Path = System.IO.Path;

namespace Diffusion.Toolkit
{
    public partial class MainWindow
    {
        private void InitFolders()
        {
            _model.MoveSelectedImagesToFolder = MoveSelectedImagesToFolder;

            _model.ScanFolderCommand = new RelayCommand<FolderViewModel>((o) =>
            {
                ServiceLocator.ScanningService.ScanFolder(o);
            });

            _model.CreateFolderCommand = new RelayCommand<FolderViewModel>((o) =>
            {
                ShowCreateFolderDialog(o);
            });

            _model.RenameFolderCommand = new RelayCommand<FolderViewModel>((o) =>
            {
                ShowRenameFolderDialog(o);
            });

            _model.DeleteFolderCommand = new RelayCommand<FolderViewModel>((o) =>
            {
                ShowDeleteFolderDialog(o);
            });

            _model.ArchiveFolderCommand = new RelayCommand<bool>((o) =>
            {
                Task.Run(() =>
                {
                    var folders = ServiceLocator.MainModel.Folders.Where(d => d.IsSelected);
                    foreach (var folder in folders)
                    {
                        ServiceLocator.DataStore.SetFolderArchived(folder.Id, o, false);
                    }
                    ServiceLocator.FolderService.LoadFolders();
                });
            });

            _model.ArchiveFolderRecursiveCommand = new RelayCommand<bool>((o) =>
            {
                Task.Run(() =>
                {
                    var folders = ServiceLocator.MainModel.Folders.Where(d => d.IsSelected);
                    foreach (var folder in folders)
                    {
                        ServiceLocator.DataStore.SetFolderArchived(folder.Id, o, true);
                    }
                    ServiceLocator.FolderService.LoadFolders();
                });
            });

            _model.ExcludeFolderCommand = new RelayCommand<FolderViewModel>((o) =>
            {
                ServiceLocator.DataStore.SetFolderExcluded(o.Id, !o.IsExcluded, false);
            });

            _model.ReloadFoldersCommand = new RelayCommand<object>((o) =>
            {
                _ = ServiceLocator.FolderService.LoadFolders();
            });

            _ = ServiceLocator.FolderService.LoadFolders();

        }

        static bool IsValidFolderName(string folderName)
        {
            string[] reservedNames = { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" };

            if (Array.IndexOf(reservedNames, folderName.ToUpper()) != -1)
            {
                return false;
            }

            char[] invalidChars = Path.GetInvalidFileNameChars();

            if (folderName.IndexOfAny(invalidChars) != -1)
            {
                return false;
            }

            if (folderName.Trim() != folderName)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(folderName))
            {
                return false;
            }

            return true;
        }

        private void RemoveFolder(FolderViewModel folder)
        {
            if (folder.Children != null)
            {
                foreach (var child in folder.Children)
                {
                    RemoveFolder(child);
                }
            }

            _model.Folders.Remove(folder);
        }

        private async void ShowDeleteFolderDialog(FolderViewModel folder)
        {
            var result = await _messagePopupManager.Show($"Are you sure you want to delete the folder \"{folder.Name}\" and all the images and metadata under it? This cannot be undone!", "Delete folder", PopupButtons.YesNo);

            if (result == PopupResult.Yes)
            {
                using (var db = _dataStore.OpenConnection())
                {
                    try
                    {
                        ServiceLocator.DataStore.RemoveFolder(folder.Path);

                        ThumbnailCache.Instance.Unload(folder.Path);
                        ServiceLocator.FileService.Delete(folder.Path);

                        db.Commit();

                        Dispatcher.Invoke(() =>
                        {
                            RemoveFolder(folder);
                            folder.Parent!.Children!.Remove(folder);
                            folder.Parent!.HasChildren = folder.Parent!.Children.Any();
                        });

                        _search.OpenFolder(folder.Parent);
                    }
                    catch (Exception e)
                    {
                        db.Rollback();
                    }


                }

            }
        }


        private async void ShowCreateFolderDialog(FolderViewModel folder)
        {
            var (result, text) = await _messagePopupManager.ShowInput("Enter a name for the new folder", "New folder");

            if (result == PopupResult.OK)
            {
                var currentFolder = _model.CurrentFolder;

                if (!IsValidFolderName(text))
                {
                    await _messagePopupManager.Show("Invalid folder name", "New Folder");
                    return;
                }

                var directory = new DirectoryInfo(currentFolder.Path);

                if (directory.Exists)
                {
                    directory.CreateSubdirectory(text);

                    Dispatcher.Invoke(() =>
                    {
                        _search.RefreshFolder(currentFolder);
                    });
                }
            }
        }

        private async void ShowRenameFolderDialog(FolderViewModel folder)
        {

            var (result, text) = await ServiceLocator.MessageService.ShowInput("Enter a new name for the folder", "Rename folder", folder.Name);

            if (result == PopupResult.OK)
            {
                var parentPath = Path.GetDirectoryName(folder.Path);
                var newPath = Path.Combine(parentPath, text);

                if (!IsValidFolderName(text))
                {
                    await ServiceLocator.MessageService.Show("Invalid folder name", "Rename Folder");
                    return;
                }

                if (Directory.Exists(newPath))
                {
                    await ServiceLocator.MessageService.Show($"The folder \"{text}\" already exists.", "Rename Folder");
                    return;
                }

                if (folder.Name.Equals(text, StringComparison.InvariantCultureIgnoreCase))
                {
                    return;
                }

                await Task.Run(async () =>
                {
                    using (var db = _dataStore.OpenConnection())
                    {
                        db.BeginTransaction();

                        try
                        {
                            if (await MovePath(db, folder.Path, newPath))
                            {
                                Directory.Move(folder.Path, newPath);

                                RenameFolderViews(folder, folder.Path, newPath);

                                Dispatcher.Invoke(() =>
                                {
                                    folder.Path = newPath;
                                    folder.Name = text;
                                });

                                db.Commit();
                            }
                            else
                            {
                                db.Rollback();
                            }

                            _search.OpenFolder(folder);

                        }
                        catch (Exception ex)
                        {
                            db.Rollback();
                            throw;
                        }
                    }
                });


            }
        }

        private void RenameFolderViews(FolderViewModel folder, string source, string dest)
        {
            if (folder.Children != null)
            {
                foreach (var child in folder.Children)
                {
                    RenameFolderViews(child, source, dest);
                }
            }

            var subPath = folder.Path.Substring(source.Length);
            var newPath = Path.Join(dest, subPath);

            folder.Path = newPath;
        }


        private async Task<bool> MovePath(SQLiteConnection db, string sourcePath, string destinationPath)
        {
            //TODO: Check that destinationPath is under a watched folder
            var moved = 0;

            if (await ServiceLocator.ProgressService.TryStartTask())
            {
                try
                {
                    ThumbnailCache.Instance.Unload(sourcePath);

                    var images = _dataStore.GetAllPathImages(sourcePath).ToList();

                    ServiceLocator.FolderService.DisableWatchers();

                    Dispatcher.Invoke(() =>
                    {
                        _model.TotalProgress = images.Count;
                        _model.CurrentProgress = 0;
                    });


                    var folderCache = ServiceLocator.FolderService.AllFolders.ToDictionary(d => d.Path);

                    foreach (var image in images)
                    {
                        var subPath = image.Path.Substring(sourcePath.Length);
                        var newPath = Path.Join(destinationPath, subPath);

                        if (image.Path != newPath)
                        {
                            _dataStore.MoveImage(db, image.Id, newPath, folderCache);

                            var moved1 = moved;
                            if (moved % 33 == 0)
                            {
                                image.Path = newPath;

                                ServiceLocator.ProgressService.SetProgress(moved1);
                                ServiceLocator.ProgressService.SetStatus(
                                    $"Moving {_model.CurrentProgress:#,###,###} of {_model.TotalProgress:#,###,###}...");
                            }

                            moved++;
                        }
                        else
                        {
                            _model.TotalProgress--;
                        }
                    }

                    return true;
                }
                catch (Exception e)
                {
                    Logger.Log("MovePath: " + e.Message);
                    await ServiceLocator.MessageService.ShowMedium($"An error occured while moving a folder:\r\n{e.Message}", "Move images", PopupButtons.OK);
                    return false;
                }
                finally
                {
                    ServiceLocator.ToastService.Toast($"{moved} files were moved.", "Move images");

                    ServiceLocator.ProgressService.CompleteTask();
                    ServiceLocator.ProgressService.ClearProgress();
                    ServiceLocator.ProgressService.SetStatus("");
                    ServiceLocator.FolderService.EnableWatchers();
                }
            }

            return false;
        }


    }
}
