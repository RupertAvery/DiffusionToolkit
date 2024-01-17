using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using Diffusion.Civitai;
using Diffusion.Civitai.Models;
using System.Diagnostics;
using System.Threading;
using Diffusion.Toolkit.Classes;
using Diffusion.Toolkit.Models;
using static System.Net.Mime.MediaTypeNames;

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

                Directory.Move(currentFolder.Path, newPath);

                Dispatcher.Invoke(() =>
                {
                    currentFolder.Path = newPath;
                    currentFolder.Name = text;
                });

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
                Path = path
            }));
        }
    }
}
