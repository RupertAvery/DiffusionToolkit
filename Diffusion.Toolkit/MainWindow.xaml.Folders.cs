using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Diffusion.Common;
using Diffusion.Database;
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
        private async Task InitFolders()
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

            await ServiceLocator.FolderService.LoadFolders();


            ServiceLocator.DataStore.DataChanged += DataChanged;
        }

        private void DataChanged(object? sender, DataChangedEventArgs e)
        {
            if (e is { EntityType: EntityType.Folder, SourceType: SourceType.Collection })
            {
                _ = ServiceLocator.FolderService.LoadFolders();
            }
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

            var listItem = _model.Folders.FirstOrDefault(d => d.Path == folder.Path);

            if (listItem != null)
            {
                _model.Folders.Remove(listItem);
            }
        }

        private async void ShowDeleteFolderDialog(FolderViewModel folder)
        {
            var selectedFolders = ServiceLocator.MainModel.Folders.Where(d => d.IsSelected).ToList();

            PopupResult result;

            var title = GetLocalizedText("Actions.Folders.Delete.Message");

            if (selectedFolders.Count > 1)
            {
                result = await ServiceLocator.MessageService.Show(GetLocalizedText("Actions.Folders.DeleteSelection.Message"), title, PopupButtons.YesNo);
            }
            else
            {
                result = await ServiceLocator.MessageService.Show(GetLocalizedText("Actions.Folders.Delete.Message").Replace("{folder}", folder.Name), title, PopupButtons.YesNo);
            }

            if (result == PopupResult.Yes)
            {

                var count = 0;
                foreach (var model in selectedFolders)
                {
                    ServiceLocator.DataStore.RemoveFolder(model.Path, count == selectedFolders.Count - 1);

                    if (Directory.Exists(model.Path))
                    {
                        ThumbnailCache.Instance.Unload(model.Path);
                        ServiceLocator.FolderService.Delete(model.Path);
                    }

                    Dispatcher.Invoke(() =>
                    {
                        ServiceLocator.MainModel.Folders.Remove(model);
                        //RemoveFolder(model);
                        model.IsSelected = false;
                        folder.Parent!.Children!.Remove(model);
                        folder.Parent!.HasChildren = folder.Parent!.Children.Any();
                    });

                    count++;
                }

                

                _search.OpenFolder(selectedFolders[0]);
            }
        }


        private async void ShowCreateFolderDialog(FolderViewModel folder)
        {
            var title = GetLocalizedText("Actions.Folders.Create.Title");

            var (result, name) = await ServiceLocator.MessageService.ShowInput(GetLocalizedText("Actions.Folders.Create.Message"), title);

            if (result == PopupResult.OK)
            {
                var currentFolder = _model.CurrentFolder;

                if (!IsValidFolderName(name))
                {
                    await ServiceLocator.MessageService.Show(GetLocalizedText("Actions.Folders.Invalid.Message"), title);
                    return;
                }

                var newPath = Path.Combine(currentFolder.Path, name);

                if (Directory.Exists(newPath))
                {
                    await ServiceLocator.MessageService.Show(GetLocalizedText("Actions.Folders.Exists.Message").Replace("{folder}", name), title);
                    return;
                }


                var directory = new DirectoryInfo(currentFolder.Path);

                if (directory.Exists)
                {
                    directory.CreateSubdirectory(name);

                    Dispatcher.Invoke(() =>
                    {
                        _search.RefreshFolder(currentFolder);
                    });
                }
 
            }
        }

        private async void ShowRenameFolderDialog(FolderViewModel folder)
        {
            var title = GetLocalizedText("Actions.Folders.Rename.Title");

            var (result, name) = await ServiceLocator.MessageService.ShowInput(GetLocalizedText("Actions.Folders.Rename.Message"), title, folder.Name);

            if (result == PopupResult.OK)
            {
                var parentPath = Path.GetDirectoryName(folder.Path);
                var newPath = Path.Combine(parentPath, name);

                if (!IsValidFolderName(name))
                {
                    await ServiceLocator.MessageService.Show(GetLocalizedText("Actions.Folders.Invalid.Message"), title);
                    return;
                }

                if (Directory.Exists(newPath))
                {
                    await ServiceLocator.MessageService.Show(GetLocalizedText("Actions.Folders.Exists.Message").Replace("{folder}", name), title);
                    return;
                }

                if (folder.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    return;
                }

                await Task.Run(async () =>
                {
                    // TODO: Lock? Prevent other tasks from running?

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
                                    folder.Name = name;
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
