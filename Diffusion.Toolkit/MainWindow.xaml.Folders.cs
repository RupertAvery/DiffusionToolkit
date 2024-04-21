using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Diffusion.Toolkit.Classes;
using Diffusion.Toolkit.Models;
using SQLite;
using Path = System.IO.Path;

namespace Diffusion.Toolkit
{
    public partial class MainWindow
    {
        private void InitFolders()
        {
            _model.MoveSelectedImagesToFolder = MoveSelectedImagesToFolder;

            _model.CreateFolderCommand = new RelayCommand<object>((o) =>
            {
                ShowCreateFolderDialog();
            });

            _model.RenameFolderCommand = new RelayCommand<object>((o) =>
            {
                ShowRenameFolderDialog();
            });

            _model.DeleteFolderCommand = new RelayCommand<object>((o) =>
            {
                ShowDeleteFolderDialog();
            });

            _model.ReloadFoldersCommand = new RelayCommand<object>((o) =>
            {
                LoadFolders();
            });

            LoadFolders();
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

        private async void ShowDeleteFolderDialog()
        {
            var currentFolder = _model.CurrentFolder!;

            var result = await _messagePopupManager.Show($"Are you sure you want to delete the folder \"{currentFolder.Name}\"?", "Delete folder", PopupButtons.YesNo);

            if (result == PopupResult.Yes)
            {
                var directory = new DirectoryInfo(currentFolder.Path);

                if (directory.Exists)
                {
                    directory.Delete(true);

                    Dispatcher.Invoke(() =>
                    {
                        RemoveFolder(currentFolder);
                        currentFolder.Parent!.Children!.Remove(currentFolder);
                        currentFolder.Parent!.HasChildren = currentFolder.Parent!.Children.Any();
                    });

                    _search.OpenFolder(currentFolder.Parent);
                }

            }
        }

        private async void ShowCreateFolderDialog()
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

        private async void ShowRenameFolderDialog()
        {
            var currentFolder = _model.CurrentFolder!;

            var (result, text) = await _messagePopupManager.ShowInput("Enter a new name for the folder", "Rename folder", currentFolder.Name);

            if (result == PopupResult.OK)
            {
                if (!IsValidFolderName(text))
                {
                    await _messagePopupManager.Show("Invalid folder name", "Rename Folder");
                    return;
                }

                if (currentFolder.Name.Equals(text, StringComparison.InvariantCultureIgnoreCase))
                {
                    return;
                }

                var parentPath = Path.GetDirectoryName(currentFolder.Path);
                var newPath = Path.Combine(parentPath, text);

                using (var db = _dataStore.OpenConnection())
                {
                    db.BeginTransaction();

                    try
                    {
                        await MoveRenamePath(db, currentFolder.Path, newPath);

                        Directory.Move(currentFolder.Path, newPath);

                        RenameFolder(currentFolder, currentFolder.Path, newPath);

                        db.Commit();

                        _search.OpenFolder(currentFolder);
                    }
                    catch (Exception ex)
                    {
                        db.Rollback();
                        throw;
                    }
                }

                Dispatcher.Invoke(() =>
                {
                    currentFolder.Path = newPath;
                    currentFolder.Name = text;
                });

            }
        }
        
        private void RenameFolder(FolderViewModel folder, string source, string dest)
        {
            if (folder.Children != null)
            {
                foreach (var child in folder.Children)
                {
                    RenameFolder(child, source, dest);
                }
            }

            var subPath = folder.Path.Substring(source.Length);
            var newPath = Path.Join(dest, subPath);

            folder.Path = newPath;
        }


        private async Task MoveRenamePath(SQLiteConnection db, string source, string dest)
        {
            var images = _dataStore.GetAllPathImages(source).ToList();

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
                var subPath = image.Path.Substring(source.Length);
                var newPath = Path.Join(dest, subPath);

                if (image.Path != newPath)
                {
                    _dataStore.MoveImage(db, image.Id, newPath, folderIdCache);

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

        private void LoadFolders()
        {
            var folders = _settings.ImagePaths;

            _model.Folders = new ObservableCollection<FolderViewModel>(folders.Select(path => new FolderViewModel()
            {
                HasChildren = true,
                Visible = true,
                Depth = 0,
                Name = path,
                Path = path,
                IsUnavailable = !Directory.Exists(path)
            }));
        }
    }
}
