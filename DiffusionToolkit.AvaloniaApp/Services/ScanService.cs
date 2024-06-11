using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Diffusion.Database;
using Diffusion.IO;

namespace DiffusionToolkit.AvaloniaApp.Services;

public class ScanService
{
    private DataStore _dataStore;

    public event EventHandler<ScanProgressEventArgs> ScanProgress;
    public event EventHandler<string> ScanStatus;
    public event EventHandler<EventArgs> ScanStart;
    public event EventHandler<ScanCompleteEventArgs> ScanComplete;

    public ScanService()
    {
        _dataStore = ServiceLocator.DataStore;
    }

    private string GetLocalizedText(string key)
    {
        return key;
    }

    public CancellationTokenSource? CancellationTokenSource { get; private set; }


    public void ScanFolders()
    {
        var settings = ServiceLocator.Settings;
        ScanFolders(settings.IncludedFolders, settings.ExcludedFolders, settings.RecurseFolders);
    }

    public void RebuildMetadata()
    {
        var settings = ServiceLocator.Settings;
        ScanFolders(settings.IncludedFolders, settings.ExcludedFolders, settings.RecurseFolders, true);
    }

    public void ScanFolders(IEnumerable<string> includeFolders, IEnumerable<string> excludeFolders, bool recurse, bool rebuildMetadata = false)
    {
        int progress = 0;
        int total = 0;
        var fileExtensions = ".png, .jpg, .jpeg, .webp";

        CancellationTokenSource?.Cancel();
        CancellationTokenSource = new CancellationTokenSource();

        ScanStart?.Invoke(this, EventArgs.Empty);

        var removed = 0;
        var added = 0;

        ScanStatus?.Invoke(this, GetLocalizedText("Actions.Scanning.BeginScanning"));

        try
        {
            var existingImages = _dataStore.GetImagePaths().ToList();

            ScanStatus?.Invoke(this, GetLocalizedText("Actions.Scanning.CheckRemoved"));

            var filesToScan = new List<string>();

            var gatheringFilesMessage = GetLocalizedText("Actions.Scanning.GatheringFiles");

            foreach (var path in includeFolders)
            {
                if (CancellationTokenSource.IsCancellationRequested)
                {
                    break;
                }

                if (Directory.Exists(path))
                {
                    ScanStatus?.Invoke(this, gatheringFilesMessage.Replace("{path}", path));

                    var ignoreFiles = rebuildMetadata ? null : existingImages.Where(p => p.Path.StartsWith(path)).Select(p => p.Path).ToHashSet();

                    filesToScan.AddRange(MetadataScanner.GetFiles(path, fileExtensions, ignoreFiles, recurse, excludeFolders).ToList());
                }
            }

            var results = ScanFiles(filesToScan, rebuildMetadata, CancellationTokenSource.Token);


            ScanComplete?.Invoke(this, new ScanCompleteEventArgs()
            {
                Added = results.Added,
                Updated = results.Updated,
                Scanned = results.Scanned,
                Message = "Scan Completed in {time}. {count} images {action}"
                    .Replace("{count}", rebuildMetadata ? $"{results.Updated:N0}" : $"{results.Added:N0}")
                    .Replace("{action}", rebuildMetadata ? "updated" : "added")
                    .Replace("{time}", $"{results.ElapsedTime:N0}s"),
                Cancelled = CancellationTokenSource.IsCancellationRequested,
                Removed = results.Removed,
                ElapsedTime = results.ElapsedTime,
            });

            //added = _added;

            //if ((added + removed == 0 && reportIfNone) || added + removed > 0)
            //{
            //    Report(added, removed, elapsedTime, updateImages);
            //}
        }
        //catch (Exception ex)
        //{
        //    //await _messagePopupManager.ShowMedium(
        //    //    ex.Message,
        //    //    "Scan Error", PopupButtons.OK);
        //}
        finally
        {
            //_model.IsBusy = false;

            //ScanStatus?.Invoke(this, GetLocalizedText("Actions.Scanning.Completed"));
        }

        //return added + removed > 0;

    }

    private struct ScanResults
    {
        public int Added { get; set; }
        public int Scanned { get; set; }
        public float ElapsedTime { get; set; }
        public int Removed { get; set; }
        public int Updated { get; set; }
    }

    private ScanResults ScanFiles(IList<string> filesToScan, bool rebuildMetadata, CancellationToken cancellationToken)
    {
        var scanResults = new ScanResults();

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var total = filesToScan.Count;

        ScanStart?.Invoke(this, EventArgs.Empty);

        var folderIdCache = new Dictionary<string, int>();

        var newImages = new List<Image>();

        var includeProperties = new List<string>();

        //if (_settings.AutoTagNSFW)
        //{
        //    includeProperties.Add(nameof(Image.NSFW));
        //}

        var scanning = GetLocalizedText("Actions.Scanning.Status");

        foreach (var file in MetadataScanner.Scan(filesToScan))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            scanResults.Scanned++;

            if (file != null)
            {
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
                    NoMetadata = file.NoMetadata
                };

                if (!string.IsNullOrEmpty(file.HyperNetwork) && !file.HyperNetworkStrength.HasValue)
                {
                    file.HyperNetworkStrength = 1;
                }

                //if (_settings.AutoTagNSFW)
                //{
                //    if (_settings.NSFWTags.Any(t => image.Prompt != null && image.Prompt.ToLower().Contains(t.Trim().ToLower())))
                //    {
                //        image.NSFW = true;
                //    }
                //}

                newImages.Add(image);
            }

            if (newImages.Count == 100)
            {
                if (rebuildMetadata)
                {

                    scanResults.Updated += _dataStore.UpdateImagesByPath(newImages, includeProperties, folderIdCache, cancellationToken);
                }
                else
                {
                    _dataStore.AddImages(newImages, includeProperties, folderIdCache, cancellationToken);
                    scanResults.Added += newImages.Count;
                }

                newImages.Clear();
            }

            if (scanResults.Scanned % 33 == 0)
            {
                ScanProgress?.Invoke(this, new ScanProgressEventArgs()
                {
                    Message = "Scanning {progress} of {total}",
                    Progress = scanResults.Scanned,
                    Total = total
                });
            }
        }

        if (newImages.Count > 0)
        {
            if (rebuildMetadata)
            {
                scanResults.Added += _dataStore.UpdateImagesByPath(newImages, includeProperties, folderIdCache, cancellationToken);
            }
            else
            {
                _dataStore.AddImages(newImages, includeProperties, folderIdCache, cancellationToken);
                scanResults.Added += newImages.Count;
            }
        }

        //Dispatcher.Invoke(() =>
        //{
        //    if (_model.TotalProgress > 0)
        //    {
        //        var text = scanning
        //            .Replace("{current}", $"{_model.TotalProgress:#,###,##0}")
        //            .Replace("{total}", $"{_model.TotalProgress:#,###,##0}");

        //        _model.Status = text;
        //    }
        //    _model.TotalProgress = Int32.MaxValue;
        //    _model.CurrentProgress = 0;
        //});

        stopwatch.Stop();

        scanResults.ElapsedTime = stopwatch.ElapsedMilliseconds / 1000f;

        return scanResults;
    }

    public void Cancel()
    {
        CancellationTokenSource?.Cancel();
        CancellationTokenSource = new CancellationTokenSource();
    }

}