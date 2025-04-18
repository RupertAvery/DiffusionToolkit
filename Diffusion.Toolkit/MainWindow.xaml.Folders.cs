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

            _model.RefreshFolderCommand = new RelayCommand<FolderViewModel>((folder) =>
            {
                ServiceLocator.FolderService.RefreshFolder(folder);
            });

            _model.ScanFolderCommand = new RelayCommand<FolderViewModel>((folder) =>
            {
                ServiceLocator.ScanningService.ScanFolder(folder, false);
            });

            _model.CreateFolderCommand = new AsyncCommand<FolderViewModel>(async (o) =>
            {
                await ServiceLocator.FolderService.ShowCreateFolderDialog(o);
            });


            _model.RenameFolderCommand = new AsyncCommand<FolderViewModel>(async (o) =>
            {
                await ServiceLocator.FolderService.ShowRenameFolderDialog(o);
            });

            _model.RemoveFolderCommand = new AsyncCommand<FolderViewModel>(async (o) =>
            {
                await ServiceLocator.FolderService.ShowRemoveFolderDialog(o);
            });

            _model.DeleteFolderCommand = new AsyncCommand<FolderViewModel>(async (o) =>
            {
                var selectedFolders = ServiceLocator.FolderService.SelectedFolders.ToList();

                if (await ServiceLocator.FolderService.ShowDeleteFolderDialog(selectedFolders))
                {
                    _search.OpenFolder(selectedFolders[0].Parent);
                }

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


      
    }
}
