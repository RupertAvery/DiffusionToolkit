using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Diffusion.Common;
using Diffusion.Database;
using Diffusion.Database.Models;
using Diffusion.IO;
using Diffusion.Toolkit.Configuration;
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
        if(ServiceLocator.MainModel.FoldersBusy) return;

        await Task.Run(() =>
        {
            ServiceLocator.MainModel.FoldersBusy = true;

            try
            {
                foreach (var folder in ServiceLocator.FolderService.RootFolders)
                {
                    // Check if we can access the path
                    if (Directory.Exists(folder.Path))
                    {
                        if (folder is { Unavailable: true })
                        {
                            _dataStore.SetFolderUnavailable(folder.Id, false, true);
                        }
                    }
                    else
                    {
                        _dataStore.SetFolderUnavailable(folder.Id, true, true);
                    }
                }
            }
            finally
            {
                ServiceLocator.MainModel.FoldersBusy = false;
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


    public async Task ScanFolder(FolderViewModel folder)
    {
        if (await ServiceLocator.ProgressService.TryStartTask())
        {
            var filesToScan = new List<string>();

            var cancellationToken = ServiceLocator.ProgressService.CancellationToken;

            foreach (var model in ServiceLocator.MainModel.Folders.Where(d => d.IsSelected))
            {
                filesToScan.AddRange(await ServiceLocator.ScanningService.GetFilesToScan(model.Path, new HashSet<string>(), cancellationToken));
            }

            await ServiceLocator.MetadataScannerService.QueueBatchAsync(filesToScan,
                new ScanCompletionEvent()
                {
                    OnDatabaseWriteCompleted = () =>
                    {
                        if (folder.Id == 0)
                        {
                            ServiceLocator.FolderService.SetFoldersDirty();
                            var dbEntity = ServiceLocator.DataStore.GetFolder(folder.Path);
                            if (dbEntity != null)
                            {
                                folder.IsScanned = true;
                                folder.Id = dbEntity.Id;
                            }
                        }
                        ServiceLocator.SearchService.RefreshResults();
                        ServiceLocator.FolderService.LoadFolders();
                    }
                },
                cancellationToken);
        }
    }

    public async Task<IEnumerable<string>> GetFilesToScan(string path, HashSet<string> ignoreFiles, CancellationToken cancellationToken)
    {
        var excludePaths = ServiceLocator.FolderService.ExcludedOrArchivedFolderPaths;

        return await Task.Run(() => MetadataScanner.GetFiles(path, _settings.FileExtensions, ignoreFiles, _settings.RecurseFolders.GetValueOrDefault(true), excludePaths, cancellationToken).ToList());
    }

    public async Task ScanWatchedFolders(bool updateImages, bool reportIfNone, CancellationToken cancellationToken)
    {
        bool foldersUnavailable = false;
        bool foldersRestored = false;

        var unavailable = 0;
        var added = 0;

        ServiceLocator.ProgressService.SetStatus(GetLocalizedText("Actions.Scanning.BeginScanning"));

        try
        {
            var filesToScan = new List<string>();

            var gatheringFilesMessage = GetLocalizedText("Actions.Scanning.GatheringFiles");

            var excludedPaths = ServiceLocator.FolderService.ExcludedOrArchivedFolderPaths.ToHashSet();

            foreach (var folder in ServiceLocator.FolderService.RootFolders)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                if (Directory.Exists(folder.Path))
                {
                    //ServiceLocator.ProgressService.SetStatus(GetLocalizedText("Actions.Scanning.CheckUnavailable"));

                    //foldersUnavailable |= CheckFolderUnavailable(path);

                    var folderImages = _dataStore.GetAllPathImages(folder.Path).ToList();

                    var folderImagesHashSet = folderImages.Select(p => p.Path).ToHashSet();

                    //if (_settings.ScanUnavailable)
                    //{
                    //    unavailable += CheckFilesUnavailable(folderImages, folderImagesHashSet, cancellationToken);
                    //}

                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    ServiceLocator.ProgressService.SetStatus(gatheringFilesMessage.Replace("{path}", folder.Path));

                    var ignoreFiles = updateImages ? null : folderImagesHashSet;

                    filesToScan.AddRange(MetadataScanner.GetFiles(folder.Path, _settings.FileExtensions, ignoreFiles, _settings.RecurseFolders.GetValueOrDefault(true), excludedPaths, cancellationToken).ToList());
                }
                else
                {
                    _dataStore.SetFolderUnavailable(folder.Id, true, true);

                    //var childImages = _dataStore.GetAllPathImages(folder.Path);

                    //foreach (var childImageChunk in childImages.Chunk(100))
                    //{
                    //    if (cancellationToken.IsCancellationRequested)
                    //    {
                    //        break;
                    //    }

                    //    _dataStore.SetUnavailable(childImageChunk.Select(c => c.Id), true);
                    //}

                }
            }

            await ServiceLocator.MetadataScannerService.QueueBatchAsync(filesToScan, null, cancellationToken);

        }
        catch (Exception ex)
        {
            await ServiceLocator.MessageService.ShowMedium(ex.Message,
                "Scan Error", PopupButtons.OK);
        }
    }


    public (Image, IReadOnlyCollection<IO.Node>) ProcessFile(FileParameters file, bool storeMetadata, bool storeWorkflow)
    {
        var newNodes = new List<IO.Node>();

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
            HasError = file.HasError,
            Hash = file.Hash
        };

        if (storeMetadata)
        {
            image.Workflow = file.Workflow;
        }

        if (!string.IsNullOrEmpty(file.HyperNetwork) && !file.HyperNetworkStrength.HasValue)
        {
            image.HyperNetworkStrength = 1;
        }

        if (_settings.AutoTagNSFW)
        {
            if (_settings.NSFWTags.Any(t => image.Prompt != null && image.Prompt.ToLower().Contains(t.Trim().ToLower())))
            {
                image.NSFW = true;
            }
        }

        if (storeWorkflow && file.Nodes is { Count: > 0 })
        {
            foreach (var fileNode in file.Nodes)
            {
                fileNode.ImageRef = image;
            }

            newNodes.AddRange(file.Nodes);
        }

        return (image, newNodes);
    }

    private readonly object _lock = new object();

    public int UpdateImages(IReadOnlyCollection<Image> images, IReadOnlyCollection<IO.Node> nodes, IReadOnlyCollection<string> includeProperties, Dictionary<string, Folder> folderCache, bool storeWorkflow, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            var db = _dataStore.OpenConnection();

            int updated = 0;

            try
            {
                db.BeginTransaction();

                updated =
                    _dataStore.UpdateImagesByPath(db, images, includeProperties, folderCache, cancellationToken);

                if (storeWorkflow && nodes.Any())
                {
                    _dataStore.UpdateNodes(db, nodes, cancellationToken);
                }

                db.Commit();

            }
            catch (Exception ex)
            {
                db.Rollback();
                Logger.Log("UpdateImages: " + ex.Message + " " + ex.StackTrace);
            }

            return updated;
        }
    }

    public int AddImages(IReadOnlyCollection<Image> images, IReadOnlyCollection<IO.Node> nodes, IReadOnlyCollection<string> includeProperties, Dictionary<string, Folder> folderCache, bool storeWorkflow, CancellationToken cancellationToken)
    {
        lock (_lock)
        {

            var db = _dataStore.OpenConnection();

            int added = 0;

            try
            {
                db.BeginTransaction();

                added = _dataStore.AddImages(db, images, includeProperties, folderCache, cancellationToken);

                if (storeWorkflow && nodes.Any())
                {
                    _dataStore.AddNodes(db, nodes, cancellationToken);
                }

                db.Commit();

            }
            catch (Exception ex)
            {
                db.Rollback();
                Logger.Log("AddImages: " + ex.Message + " " + ex.StackTrace);
            }

            return added;
        }
    }


    public (int, long) ScanFiles(IList<string> filesToScan, bool updateImages, bool storeMetadata, bool storeWorkflow, CancellationToken cancellationToken)
    {
        //foreach (var file in filesToScan)
        //{
        //    _ = ServiceLocator.MetadataScannerService.QueueAsync(file);
        //}

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



                // var (image, nodes) = ProcessFile(file, storeMetadata, storeWorkflow);

                //var fileInfo = new FileInfo(file.Path);

                //var image = new Image()
                //{
                //    Prompt = file.Prompt,
                //    NegativePrompt = file.NegativePrompt,
                //    Path = file.Path,
                //    FileName = fileInfo.Name,
                //    Width = file.Width,
                //    Height = file.Height,
                //    ModelHash = file.ModelHash,
                //    Model = file.Model,
                //    Steps = file.Steps,
                //    Sampler = file.Sampler,
                //    CFGScale = file.CFGScale,
                //    Seed = file.Seed,
                //    BatchPos = file.BatchPos,
                //    BatchSize = file.BatchSize,
                //    CreatedDate = fileInfo.CreationTime,
                //    ModifiedDate = fileInfo.LastWriteTime,
                //    AestheticScore = file.AestheticScore,
                //    HyperNetwork = file.HyperNetwork,
                //    HyperNetworkStrength = file.HyperNetworkStrength,
                //    ClipSkip = file.ClipSkip,
                //    FileSize = file.FileSize,
                //    NoMetadata = file.NoMetadata,
                //    WorkflowId = file.WorkflowId,
                //    HasError = file.HasError
                //};

                //if (storeMetadata)
                //{
                //    image.Workflow = file.Workflow;
                //}

                //if (!string.IsNullOrEmpty(file.HyperNetwork) && !file.HyperNetworkStrength.HasValue)
                //{
                //    image.HyperNetworkStrength = 1;
                //}

                //if (_settings.AutoTagNSFW)
                //{
                //    if (_settings.NSFWTags.Any(t => image.Prompt != null && image.Prompt.ToLower().Contains(t.Trim().ToLower())))
                //    {
                //        image.NSFW = true;
                //    }
                //}

                //if (storeWorkflow && file.Nodes is { Count: > 0 })
                //{
                //    foreach (var fileNode in file.Nodes)
                //    {
                //        fileNode.ImageRef = image;
                //    }

                //    newNodes.AddRange(file.Nodes);
                //}

                //newImages.Add(image);
                //newNodes.AddRange(nodes);

                //if (newImages.Count == 100)
                //{
                //    if (updateImages)
                //    {
                //        added += UpdateImages(newImages, newNodes, includeProperties, folderIdCache, storeWorkflow, cancellationToken);
                //    }
                //    else
                //    {
                //        added += AddImages(newImages, newNodes, includeProperties, folderIdCache, storeWorkflow, cancellationToken);
                //    }

                //    newNodes.Clear();
                //    newImages.Clear();
                //}

                //if (scanned % 33 == 0)
                //{
                //    ServiceLocator.ProgressService.SetProgress(scanned, scanning);
                //}
            }

            //if (newImages.Count > 0)
            //{
            //    if (updateImages)
            //    {
            //        added += UpdateImages(newImages, newNodes, includeProperties, folderIdCache, storeWorkflow, cancellationToken);
            //    }
            //    else
            //    {
            //        added += AddImages(newImages, newNodes, includeProperties, folderIdCache, storeWorkflow, cancellationToken);
            //    }
            //}

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

                var excludePaths = _settings.ExcludePaths.Concat(ServiceLocator.DataStore.GetArchivedFolders().Select(d => d.Path)).ToHashSet();

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
                        var filesOnDisk = MetadataScanner.GetFiles(folder.Path, _settings.FileExtensions, ignoreFiles, _settings.RecurseFolders.GetValueOrDefault(true), excludePaths, token);

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