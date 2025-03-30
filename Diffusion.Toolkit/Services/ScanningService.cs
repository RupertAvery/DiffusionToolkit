using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Diffusion.Database;
using Diffusion.IO;
using Diffusion.Toolkit.Localization;
using Diffusion.Toolkit.Models;

namespace Diffusion.Toolkit.Services;

public class ScanningService
{
    private string GetLocalizedText(string key)
    {
        return (string)JsonLocalizationProvider.Instance.GetLocalizedObject(key, null, CultureInfo.InvariantCulture);
    }

    private Settings _settings => ServiceLocator.Settings!;

    private DataStore _dataStore => ServiceLocator.DataStore!;



    public async Task CheckUnavailableFolders()
    {
        await Task.Run(() =>
        {
            foreach (var path in _settings.ImagePaths)
            {
                // Check if we can access the path
                if (Directory.Exists(path))
                {
                    // See if this was previously tagged as unavailable
                    var folder = _dataStore.GetFolder(path);

                    if (folder is { Unavailable: true })
                    {
                        // Restore it
                        _dataStore.SetFolderUnavailable(path, false);

                        var childImages = _dataStore.GetAllPathImages(path);

                        var ids = childImages.Select(c => c.Id).ToList();

                        if (ids.Any())
                        {
                            _dataStore.SetUnavailable(ids, false);
                        }
                    }
                }
                else
                {
                    // Tag it and it's children as unavailable
                    _dataStore.SetFolderUnavailable(path, true);

                    var childImages = _dataStore.GetAllPathImages(path);

                    var ids = childImages.Select(c => c.Id).ToList();

                    if (ids.Any())
                    {
                        _dataStore.SetUnavailable(ids, true);
                    }
                }
            }
        });
       
    }

    private int CheckFilesUnavailable(List<ImagePath> folderImages, HashSet<string> folderImagesHashSet, CancellationToken cancellationToken)
    {
        int unavailable = 0;

        //folderImagesHashSet
        var unavailableFiles = new HashSet<string>();

        ServiceLocator.ProgressService.InitializeProgress(folderImagesHashSet.Count);

        var i = 0;

        foreach (var folderImage in folderImagesHashSet)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            if (!File.Exists(folderImage))
            {
                unavailableFiles.Add(folderImage);
            }

            if (i % 33 == 0)
            {
                ServiceLocator.ProgressService.SetProgress(i, "Scanning {current} of {total}");
            }

            i++;
        }


        if (!cancellationToken.IsCancellationRequested)
        {
            //var allDirectoryFiles = MetadataScanner.GetFiles(path, settings.FileExtensions, true, cancellationToken).ToHashSet();

            //var unavailableFiles = folderImagesHashSet.Except(allDirectoryFiles).ToHashSet();


            var unavailableIds = new List<int>();

            foreach (var image in folderImages.Where(f => unavailableFiles.Contains(f.Path)))
            {
                unavailableIds.Add(image.Id);
            }

            var currentUnavailable = _dataStore.GetUnavailable(true);

            var newlyUnavailable = unavailableIds.Except(currentUnavailable.Select(i => i.Id)).ToList();

            foreach (var chunk in newlyUnavailable.Chunk(100))
            {
                _dataStore.SetUnavailable(chunk, true);
            }

            unavailable += newlyUnavailable.Count;
        }

        return unavailable;

    }

    public async Task<IEnumerable<string>> GetFilesToScan(string path, HashSet<string> ignoreFiles, CancellationToken cancellationToken)
    {
        return await Task.Run(() => MetadataScanner.GetFiles(path, _settings.FileExtensions, ignoreFiles, _settings.RecurseFolders.GetValueOrDefault(true), _settings.ExcludePaths, cancellationToken).ToList());
    }

    public async Task<bool> ScanWatchedFolders(bool updateImages, bool reportIfNone, CancellationToken cancellationToken)
    {

        bool foldersUnavailable = false;
        bool foldersRestored = false;

        var unavailable = 0;
        var added = 0;

        ServiceLocator.ProgressService.SetStatus(GetLocalizedText("Actions.Scanning.BeginScanning"));

        try
        {
            //var existingImages = _dataStore.GetImagePaths().ToList();

            var filesToScan = new List<string>();

            var gatheringFilesMessage = GetLocalizedText("Actions.Scanning.GatheringFiles");

            foreach (var path in _settings.ImagePaths)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                if (Directory.Exists(path))
                {
                    //ServiceLocator.ProgressService.SetStatus(GetLocalizedText("Actions.Scanning.CheckUnavailable"));

                    //foldersUnavailable |= CheckFolderUnavailable(path);

                    var folderImages = _dataStore.GetAllPathImages(path).ToList();

                    var folderImagesHashSet = folderImages.Select(p => p.Path).ToHashSet();

                    //if (_settings.ScanUnavailable)
                    //{
                    //    unavailable += CheckFilesUnavailable(folderImages, folderImagesHashSet, cancellationToken);
                    //}

                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    ServiceLocator.ProgressService.SetStatus(gatheringFilesMessage.Replace("{path}", path));

                    var ignoreFiles = updateImages ? null : folderImagesHashSet;

                    filesToScan.AddRange(MetadataScanner.GetFiles(path, _settings.FileExtensions, ignoreFiles, _settings.RecurseFolders.GetValueOrDefault(true), _settings.ExcludePaths, cancellationToken).ToList());
                }
                else
                {
                    foldersUnavailable = true;

                    _dataStore.SetFolderUnavailable(path, true);

                    var childImages = _dataStore.GetAllPathImages(path);

                    foreach (var childImageChunk in childImages.Chunk(100))
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        _dataStore.SetUnavailable(childImageChunk.Select(c => c.Id), true);
                    }

                }
            }

            long elapsedTime = 0;

            if (!cancellationToken.IsCancellationRequested)
            {
                (added, elapsedTime) = ScanFiles(filesToScan, updateImages, _settings.StoreMetadata, _settings.StoreWorkflow, cancellationToken);
            }

            if ((added + unavailable == 0 && reportIfNone) || added + unavailable > 0)
            {
                Report(added, unavailable, elapsedTime, updateImages, foldersUnavailable, foldersRestored);
            }
        }
        catch (Exception ex)
        {
            await ServiceLocator.MessageService.ShowMedium(ex.Message,
                "Scan Error", PopupButtons.OK);
        }
        finally
        {
            ServiceLocator.ProgressService.ClearProgress();
        }


        return added + unavailable > 0;
    }


    public (int, long) ScanFiles(IList<string> filesToScan, bool updateImages, bool storeMetadata, bool storeWorkflow, CancellationToken cancellationToken)
    {

        try
        {
            var added = 0;
            var scanned = 0;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var max = filesToScan.Count;

            ServiceLocator.ProgressService.InitializeProgress(max);

            var folderIdCache = new Dictionary<string, int>();

            var newImages = new List<Image>();
            var newNodes = new List<IO.Node>();

            var includeProperties = new List<string>();

            if (_settings.AutoTagNSFW)
            {
                includeProperties.Add(nameof(Image.NSFW));
            }

            if (storeMetadata)
            {
                includeProperties.Add(nameof(Image.Workflow));
            }

            var scanning = GetLocalizedText("Actions.Scanning.Status");

            foreach (var file in MetadataScanner.Scan(filesToScan))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                scanned++;

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
                    NoMetadata = file.NoMetadata,
                    WorkflowId = file.WorkflowId,
                    HasError = file.HasError
                };

                if (storeMetadata)
                {
                    image.Workflow = file.Workflow;
                }

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

                if (storeWorkflow && file.Nodes is { Count: > 0 })
                {
                    foreach (var fileNode in file.Nodes)
                    {
                        fileNode.ImageRef = image;
                    }

                    newNodes.AddRange(file.Nodes);
                }

                if (newImages.Count == 100)
                {
                    if (updateImages)
                    {
                        added += _dataStore.UpdateImagesByPath(newImages, includeProperties, folderIdCache, cancellationToken);
                        if (storeWorkflow && newNodes.Any())
                        {
                            _dataStore.UpdateNodes(newNodes, cancellationToken);
                        }
                    }
                    else
                    {
                        _dataStore.AddImages(newImages, includeProperties, folderIdCache, cancellationToken);
                        if (storeWorkflow && newNodes.Any())
                        {
                            _dataStore.AddNodes(newNodes, cancellationToken);
                        }

                        added += newImages.Count;
                    }

                    newNodes.Clear();
                    newImages.Clear();
                }

                if (scanned % 33 == 0)
                {
                    ServiceLocator.ProgressService.SetProgress(scanned, scanning);
                }
            }

            if (newImages.Count > 0)
            {
                if (updateImages)
                {
                    added += _dataStore.UpdateImagesByPath(newImages, includeProperties, folderIdCache, cancellationToken);
                    if (storeWorkflow && newNodes.Any())
                    {
                        _dataStore.UpdateNodes(newNodes, cancellationToken);
                    }
                }
                else
                {
                    _dataStore.AddImages(newImages, includeProperties, folderIdCache, cancellationToken);
                    if (storeWorkflow && newNodes.Any())
                    {
                        _dataStore.AddNodes(newNodes, cancellationToken);
                    }

                    added += newImages.Count;
                }
            }


            stopwatch.Stop();
            var elapsedTime = stopwatch.ElapsedMilliseconds / 1000;
            return (added, elapsedTime);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            ServiceLocator.ProgressService.ClearProgress();
        }

    }

    public async Task ScanUnavailable(UnavailableFilesModel options, CancellationToken token)
    {
        var candidateImages = new List<int>();
        var restoredImages = new List<int>();

        try
        {
            if (options.UseRootFolders)
            {
                var rootFolders = options.ImagePaths.Where(f => f.IsSelected);

                var total = 0;

                foreach (var folder in rootFolders)
                {
                    total += _dataStore.CountAllPathImages(folder.Path);
                }

                ServiceLocator.ProgressService.InitializeProgress(total);

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
                        var filesOnDisk = MetadataScanner.GetFiles(folder.Path, _settings.FileExtensions, ignoreFiles, _settings.RecurseFolders.GetValueOrDefault(true), _settings.ExcludePaths, token);

                        foreach (var file in filesOnDisk)
                        {
                            if (token.IsCancellationRequested)
                            {
                                break;
                            }

                            if (folderImages.TryGetValue(file, out var imagePath))
                            {
                                if (imagePath.Unavailable)
                                {
                                    restoredImages.Add(imagePath.Id);
                                }

                                folderImages.Remove(file);
                            }

                            current++;

                            if (current % 113 == 0)
                            {
                                ServiceLocator.ProgressService.SetProgress(current, scanning);
                            }
                        }

                        foreach (var folderImage in folderImages)
                        {
                            candidateImages.Add(folderImage.Value.Id);
                        }
                    }
                    else
                    {
                        if (options.ShowUnavailableRootFolders)
                        {
                            foreach (var folderImage in folderImages)
                            {
                                candidateImages.Add(folderImage.Value.Id);

                                current++;

                                if (current % 113 == 0)
                                {
                                    ServiceLocator.ProgressService.SetProgress(current, scanning);
                                }
                            }
                        }

                    }
                }

                ServiceLocator.ProgressService.SetProgress(total);
            }


            if (restoredImages.Any())
            {
                _dataStore.SetUnavailable(restoredImages, false);
            }

            var unavailableFiles = GetLocalizedText("UnavailableFiles");

            if (candidateImages.Any())
            {
                if (options.JustUpdate)
                {
                    //var currentUnavailableImages = _dataStore.GetUnavailable(true);
                    //candidateImages = candidateImages.Except(currentUnavailableImages.Select(i => i.Id)).ToList();
                    _dataStore.SetUnavailable(candidateImages, true);

                    var updated = GetLocalizedText("UnavailableFiles.Results.Updated");
                    updated = updated.Replace("{count}", $"{candidateImages.Count:#,###,##0}");

                    if (restoredImages.Any())
                    {
                        var restored = GetLocalizedText("UnavailableFiles.Results.Restored");
                        updated += " " + restored.Replace("{count}", $"{candidateImages.Count:#,###,##0}");
                    }

                    await ServiceLocator.MessageService.Show(updated, unavailableFiles, PopupButtons.OK);
                }
                else if (options.MarkForDeletion)
                {
                    //var currentUnavailableImages = _dataStore.GetUnavailable(true);
                    //candidateImages = candidateImages.Except(currentUnavailableImages.Select(i => i.Id)).ToList();

                    _dataStore.SetUnavailable(candidateImages, true);
                    _dataStore.SetDeleted(candidateImages, true);

                    var marked = GetLocalizedText("UnavailableFiles.Results.MarkedForDeletion");
                    marked = marked.Replace("{count}", $"{candidateImages.Count:#,###,##0}");

                    if (restoredImages.Any())
                    {
                        var restored = GetLocalizedText("UnavailableFiles.Results.Restored");
                        marked += " " + restored.Replace("{count}", $"{candidateImages.Count:#,###,##0}");
                    }

                    await ServiceLocator.MessageService.Show(marked, unavailableFiles, PopupButtons.OK);
                }
                else if (options.RemoveImmediately)
                {
                    _dataStore.RemoveImages(candidateImages);

                    var removed = GetLocalizedText("UnavailableFiles.Results.Removed");
                    removed = removed.Replace("{count}", $"{candidateImages.Count:#,###,##0}");

                    if (restoredImages.Any())
                    {
                        var restored = GetLocalizedText("UnavailableFiles.Results.Restored");
                        removed += " " + restored.Replace("{count}", $"{candidateImages.Count:#,###,##0}");
                    }


                    await ServiceLocator.MessageService.Show(removed, unavailableFiles, PopupButtons.OK);
                }
            }
        }
        finally
        {
            ServiceLocator.ProgressService.ClearProgress();
        }
    }


    public void Report(int added, int unavailable, long elapsedTime, bool updateImages, bool foldersUnavailable, bool foldersRestored)
    {
        var scanComplete = updateImages
            ? GetLocalizedText("Actions.Scanning.RebuildComplete.Caption")
            : GetLocalizedText("Actions.Scanning.ScanComplete.Caption");

        if (added == 0 && unavailable == 0)
        {
            var message = GetLocalizedText("Actions.Scanning.NoNewImages.Toast");
            ServiceLocator.ToastService.Toast(message, scanComplete);
        }
        else
        {
            var updatedMessage = GetLocalizedText("Actions.Scanning.ImagesUpdated.Toast");
            var addedMessage = GetLocalizedText("Actions.Scanning.ImagesAdded.Toast");
            var unavailableMessage = GetLocalizedText("Actions.Scanning.FilesUnavailable.Toast");
            var foldersUnavailableMessage = GetLocalizedText("Actions.Scanning.FoldersUnavailable.Toast");
            var foldersRestoredMessage = GetLocalizedText("Actions.Scanning.FoldersRestored.Toast");

            updatedMessage = updatedMessage.Replace("{count}", $"{added:#,###,##0}");
            addedMessage = addedMessage.Replace("{count}", $"{added:#,###,##0}");
            unavailableMessage = unavailableMessage.Replace("{count}", $"{unavailable:#,###,##0}");

            var newOrOpdated = updateImages ? updatedMessage : addedMessage;

            var messages = new[]
             {
                        added > 0 ? newOrOpdated : string.Empty,
                        unavailable > 0 ? unavailableMessage : string.Empty,
                        foldersUnavailable ? foldersUnavailableMessage : string.Empty,
                        foldersRestored ? foldersRestoredMessage : string.Empty,
                    };

            foreach (var message in messages.Where(m => !string.IsNullOrEmpty(m)))
            {
                ServiceLocator.ToastService.Toast(message, scanComplete, 5);
            }

        }

        SetTotalFilesStatus();
    }

    public void SetTotalFilesStatus()
    {
        var total = _dataStore.GetTotal();

        ServiceLocator.ProgressService.SetStatus($"{total:###,###,##0} images in database");
    }

}