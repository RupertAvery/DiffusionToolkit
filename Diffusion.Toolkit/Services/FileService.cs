using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Diffusion.Common;
using Diffusion.Database;
using Diffusion.Toolkit.Localization;
using Diffusion.Toolkit.Models;
using Path = System.IO.Path;

namespace Diffusion.Toolkit.Services;

public class FileService
{
    //private BufferedChannel<UpdateViewed> _viewedChannel;

    private Action<int> _debounceUpdateViewed;

    public FileService()
    {
        //_viewedChannel = new BufferedChannel<UpdateViewed>(new BufferedChannelOptions()
        //{
        //    MaxLifetime = TimeSpan.FromSeconds(2),
        //    MaxSize = 30
        //}, (batch, token) =>
        //{
        //    ServiceLocator.DataStore.UpdateViewed(batch);
        //    return Task.CompletedTask;
        //});
        _debounceUpdateViewed = Utility.Debounce<int>((id) =>
        {
            ServiceLocator.DataStore.UpdateViewed(id);
        }, 2000);

        //Task.Run(() => _viewedChannel.ConsumeAsync());
    }

    private string GetLocalizedText(string key)
    {
        return (string)JsonLocalizationProvider.Instance.GetLocalizedObject(key, null, CultureInfo.InvariantCulture);
    }

    public void Delete(string path)
    {
        if (ServiceLocator.Settings.PermanentlyDelete)
        {
            File.Delete(path);
        }
        else
        {
            Win32FileAPI.Recycle(path);
        }
    }

    public async Task DeleteFiles(IReadOnlyCollection<ImagePath> files, CancellationToken token)
    {
        int count = 0;

        var cancelled = false;

        ServiceLocator.ProgressService.InitializeProgress(files.Count);

        foreach (var imagePath in files)
        {
            if (token.IsCancellationRequested)
            {
                break;
            }

            try
            {
                count++;

                var filename = Path.GetFileName(imagePath.Path);

                ServiceLocator.ProgressService.SetProgress(count, $"Deleting {filename}");

                ServiceLocator.DataStore.RemoveImage(imagePath.Id);

                ServiceLocator.FileService.Delete(imagePath.Path);

                var dir = Path.GetDirectoryName(imagePath.Path);
                var fileName = Path.GetFileNameWithoutExtension(imagePath.Path);
                var textFilePath = Path.Join(dir, $"{fileName}.txt");

                // TODO: Why delete twice?
                //ServiceLocator.FileService.Delete(imagePath.Path);

                if (File.Exists(textFilePath))
                {
                    ServiceLocator.FileService.Delete(textFilePath);
                }

            }
            catch (Exception e)
            {
                Logger.Log($"Failed to delete {imagePath.Path}. \n\n {e.Message}");

                var result = await ServiceLocator.MessageService.Show($"Failed to delete {imagePath.Path}. \n\n {e.Message}",
                    "Error", PopupButtons.OkCancel);

                if (result == PopupResult.Cancel)
                {
                    cancelled = true;
                    break;
                }

            }
        }

        ServiceLocator.ProgressService.ClearProgress();

        //await Dispatcher.Invoke(async () =>
        //{
        //    LoadAlbums();
        //});

        if (cancelled || token.IsCancellationRequested)
        {
            await ServiceLocator.MessageService.Show($"The operation was cancelled.", "Empty recycle bin", PopupButtons.OK);
        }

        ServiceLocator.ToastService.Toast($"{count} images were deleted", "Empty recycle bin");



    }

    public async Task RenameFile(int imageId, string path)
    {
        var title = GetLocalizedText("Actions.Files.Rename.Title");

        var oldPath = Path.GetDirectoryName(path);
        var oldFilename = Path.GetFileNameWithoutExtension(path);
        var extension = Path.GetExtension(path);

        var (result, newName) = await ServiceLocator.MessageService.ShowInput(GetLocalizedText("Actions.Files.Rename.Message"),
            title, oldFilename);

        var window = ServiceLocator.WindowService.CurrentWindow;

        if (result == PopupResult.OK)
        {
            newName = newName.Trim();

            if (FileUtility.IsValidFilename(newName))
            {
                var newFilename = $"{newName}{extension}";
                var newPath = Path.Combine(oldPath, newFilename);

                try
                {
                    ServiceLocator.FolderService.DisableWatchers();
                    File.Move(path, newPath);

                    ServiceLocator.DataStore.UpdateImageFilename(imageId, newPath, newFilename);

                    ServiceLocator.MainModel.CurrentImageEntry.Path = newPath;
                    ServiceLocator.MainModel.CurrentImageEntry.FileName = newFilename;
                    ServiceLocator.MainModel.CurrentImageEntry.Name = newFilename;
                    ServiceLocator.MainModel.CurrentImage.Path = newPath;

                }
                catch (Exception e)
                {
                    Logger.Log(e.Message + "\r\n" + e.StackTrace);
                    MessageBox.Show(window, e.Message, GetLocalizedText("Actions.Files.Rename.Error.Title"),
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    ServiceLocator.FolderService.EnableWatchers();
                }

            }
            else
            {
                await ServiceLocator.MessageService.Show(GetLocalizedText("Actions.Files.Rename.Invalid.Message"),
                    title, PopupButtons.OK);
            }

        }
    }

    public async Task RemoveImagesTaggedForDeletion()
    {
        var files = ServiceLocator.DataStore.GetImagesTaggedForDeletion().ToList();
        var count = 0;

        var title = GetLocalizedText("Actions.Delete.Caption");

        if (files.Count == 0)
        {
            var noFilesMessage = GetLocalizedText("Actions.Delete.NoFiles.Caption");
            await ServiceLocator.MessageService.Show(noFilesMessage, title);
            return;
        }

        var message = ServiceLocator.Settings.PermanentlyDelete
            ? GetLocalizedText("Actions.Delete.PermanentlyDelete.Message")
            : GetLocalizedText("Actions.Delete.Delete.Message");

        var result = await ServiceLocator.MessageService.Show(message, title, PopupButtons.YesNo);

        if (result == PopupResult.Yes)
        {
            await Task.Run(async () =>
            {
                if (await ServiceLocator.ProgressService.TryStartTask())
                {
                    try
                    {
                        await ServiceLocator.FileService.DeleteFiles(files,
                            ServiceLocator.ProgressService.CancellationToken);
                    }
                    finally
                    {
                        ServiceLocator.ProgressService.CompleteTask();

                        ServiceLocator.ScanningService.SetTotalFilesStatus();

                        ServiceLocator.SearchService.RefreshResults();
                    }
                }
            });
        }
        ;
    }

    public async Task<bool> MoveFiles(ICollection<ImagePath> images, string targetPath, bool remove)
    {
        if (await ServiceLocator.ProgressService.TryStartTask())
        {
            await Task.Run(() =>
            {
                ServiceLocator.FolderService.DisableWatchers();

                try
                {
                    ServiceLocator.ProgressService.InitializeProgress(images.Count);

                    var moved = 0;

                    var folderCache = ServiceLocator.DataStore.GetFolders().ToDictionary(d => d.Path);

                    var message = GetLocalizedText("Actions.Files.Move.Message");

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
                        }

                        if (image.Path != newPath)
                        {
                            File.Move(image.Path, newPath);

                            if (File.Exists(txtPath))
                            {
                                File.Move(txtPath, newTxtPath);
                            }

                            if (remove)
                            {
                                ServiceLocator.DataStore.RemoveImage(image.Id);
                            }
                            else
                            {
                                ServiceLocator.DataStore.MoveImage(image.Id, newPath, folderCache);
                            }

                            var moved1 = moved;
                            if (moved % 33 == 0)
                            {
                                ServiceLocator.ProgressService.SetProgress(moved1, message);
                            }

                            moved++;
                        }
                        else
                        {
                            ServiceLocator.MainModel.TotalProgress--;
                        }
                    }

                    ServiceLocator.ToastService.Toast(GetLocalizedText("Actions.Files.Move.Success").Replace("{count}", $"{moved}"), "");
                }
                catch (Exception e)
                {
                    Logger.Log(e);
                    MessageBox.Show(ServiceLocator.WindowService.CurrentWindow, $"An error occured while moving files: {e.Message}", "Moving Files", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    ServiceLocator.ProgressService.ClearProgress();
                    ServiceLocator.ProgressService.ClearStatus();
                    ServiceLocator.ProgressService.CompleteTask();
                    ServiceLocator.FolderService.EnableWatchers();
                }
            });

            return true;
        }

        return false;
    }

    public bool IsRegisteredExtension(string path)
    {
        return ServiceLocator.Settings.FileExtensions.IndexOf(Path.GetExtension(path),
            StringComparison.InvariantCultureIgnoreCase) > -1;
    }


    public void UpdateViewed(int imageId)
    {
        _debounceUpdateViewed(imageId);
        //_viewedChannel.Writer.WriteAsync(new UpdateViewed() { Id = imageId, Viewed = DateTime.Now });
    }
}
