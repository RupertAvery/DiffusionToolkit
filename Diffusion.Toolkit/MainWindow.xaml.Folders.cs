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

            _model.RescanFolderCommand = new RelayCommand<FolderViewModel>((folder) =>
            {
                ServiceLocator.ScanningService.ScanFolder(folder, true);
            });

            _model.ScanFolderCommand = new RelayCommand<FolderViewModel>((folder) =>
            {
                ServiceLocator.ScanningService.ScanFolder(folder, false);
            });

            _model.CreateFolderCommand = new RelayCommand<FolderViewModel>((o) =>
            {
                ShowCreateFolderDialog(o);
            });

            _model.RenameFolderCommand = new AsyncCommand<FolderViewModel>(async (o) =>
            {
                var oldPath = o.Path;

                var (success, newName, newPath) = await ServiceLocator.FolderService.RenameFolder(o.Id, o.Name, o.Path);
                if (success)
                {
                    o.Name = newName;
                    o.Path = newPath;

                    if (o.HasChildren && o.Children != null)
                    {
                        foreach (var child in o.Children)
                        {
                            child.Path = Path.Combine(newPath, Path.GetFileName(child.Path));
                        }
                    }

                    if (_search.QueryOptions.Folder == oldPath)
                    {
                        _search.OpenFolder(o);
                    }

                    var existingEntry = _search.Images.FirstOrDefault(d => d.EntryType == EntryType.Folder && d.Path == oldPath);

                    if (existingEntry != null)
                    {
                        existingEntry.Name = newName;
                        existingEntry.Path = newPath;
                    }

                }

            });

            _model.DeleteFolderCommand = new RelayCommand<FolderViewModel>((o) =>
            {
                ShowDeleteFolderDialog(o);
            });

            _model.ArchiveFolderCommand = new RelayCommand<bool>((o) =>
            {
                Task.Run(async () =>
                {
                    var folders = ServiceLocator.FolderService.SelectedFolders;

                    foreach (var folder in folders)
                    {
                        ServiceLocator.DataStore.SetFolderArchived(folder.Id, o, false);
                    }

                    await ServiceLocator.FolderService.LoadFolders();
                });
            });

            _model.ArchiveFolderRecursiveCommand = new RelayCommand<bool>((o) =>
            {
                Task.Run(async () =>
                {
                    var folders = ServiceLocator.FolderService.SelectedFolders;

                    foreach (var folder in folders)
                    {
                        ServiceLocator.DataStore.SetFolderArchived(folder.Id, o, true);
                    }

                    await ServiceLocator.FolderService.LoadFolders();
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
            var selectedFolders = ServiceLocator.MainModel.Folders.Where(d => d.IsSelected).ToList();

            PopupResult result;

            var title = GetLocalizedText("Actions.Folders.Delete.Title");

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
                    if (Directory.Exists(model.Path))
                    {
                        ThumbnailCache.Instance.Unload(model.Path);
                        ServiceLocator.FolderService.Delete(model.Path);
                    }

                    ServiceLocator.DataStore.RemoveFolder(model.Path);

                    Dispatcher.Invoke(() =>
                    {
                        ServiceLocator.MainModel.Folders.Remove(model);
                        RemoveFolder(model);
                        model.IsSelected = false;
                        model.Parent!.Children!.Remove(model);
                    });

                    count++;
                }

                _search.OpenFolder(selectedFolders[0].Parent);
            }
        }


        private async void ShowCreateFolderDialog(FolderViewModel parentFolder)
        {
            var title = GetLocalizedText("Actions.Folders.Create.Title");

            var (result, name) = await ServiceLocator.MessageService.ShowInput(GetLocalizedText("Actions.Folders.Create.Message"), title);

            if (result == PopupResult.OK)
            {


                if (!FileUtility.IsValidFilename(name))
                {
                    await ServiceLocator.MessageService.Show(GetLocalizedText("Actions.Folders.Invalid.Message"), title);
                    return;
                }

                var newPath = Path.Combine(parentFolder.Path, name);

                if (Directory.Exists(newPath))
                {
                    await ServiceLocator.MessageService.Show(GetLocalizedText("Actions.Folders.Exists.Message").Replace("{folder}", name), title);
                    return;
                }

                var directory = new DirectoryInfo(parentFolder.Path);

                if (directory.Exists)
                {
                    directory.CreateSubdirectory(name);
                    
                    ServiceLocator.FolderService.AppendChild(parentFolder, new FolderViewModel()
                    {
                        Parent = parentFolder,
                        Name = name,
                        Depth = parentFolder.Depth + 1,
                        Path = newPath,
                        IsScanned = false
                    });


                    //ServiceLocator.FolderService.RefreshFolder(parentFolder);
                }

            }
        }
    }
}
