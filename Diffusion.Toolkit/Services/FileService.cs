using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Diffusion.Common;
using Diffusion.Database;
using Diffusion.Toolkit.Localization;
using Diffusion.Toolkit.Models;
using Diffusion.Toolkit.Services;

namespace Diffusion.Toolkit.Services;

public class FileService
{
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

                ServiceLocator.DataStore.DeleteImage(imagePath.Id);

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
}