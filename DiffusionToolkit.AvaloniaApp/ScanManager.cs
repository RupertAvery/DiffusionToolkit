using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using Diffusion.Database;
using Diffusion.IO;
using DiffusionToolkit.AvaloniaApp.Common;

namespace DiffusionToolkit.AvaloniaApp;

public class ScanManager
{
    private DataStore _dataStore;

    public event EventHandler<ScanProgressEventArgs> ScanProgress;
    public event EventHandler<string> ScanStatus;
    public event EventHandler<EventArgs> ScanStart;
    public event EventHandler<EventArgs> ScanEnd;

    public ScanManager()
    {
        _dataStore = ServiceLocator.DataStore;
    }

    private string GetLocalizedText(string key)
    {
        return key;
    }

    public CancellationTokenSource? CancellationTokenSource { get; private set; }

    public void ScanFolders(IEnumerable<string> includeFolders, IEnumerable<string> excludeFolders, bool recurse)
    {
        int progress = 0;
        int total = 0;
        var fileExtensions = ".png, .jpg, .jpeg, .webp";
        CancellationTokenSource?.Cancel();
        CancellationTokenSource = new CancellationTokenSource();

        ScanStart?.Invoke(this, EventArgs.Empty);

        var removed = 0;
        var added = 0;
        bool updateImages = false;

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

                    var ignoreFiles = updateImages ? null : existingImages.Where(p => p.Path.StartsWith(path)).Select(p => p.Path).ToHashSet();

                    filesToScan.AddRange(MetadataScanner.GetFiles(path, fileExtensions, ignoreFiles, recurse, excludeFolders).ToList());
                }
            }

            //var (_added, elapsedTime) = 
            ScanFiles(filesToScan, updateImages, CancellationTokenSource.Token);

            //added = _added;

            //if ((added + removed == 0 && reportIfNone) || added + removed > 0)
            //{
            //    Report(added, removed, elapsedTime, updateImages);
            //}
        }
        catch (Exception ex)
        {
            //await _messagePopupManager.ShowMedium(
            //    ex.Message,
            //    "Scan Error", PopupButtons.OK);
        }
        finally
        {
            //_model.IsBusy = false;

            ScanStatus?.Invoke(this, GetLocalizedText("Actions.Scanning.Completed"));
        }

        //return added + removed > 0;

        ScanEnd?.Invoke(this, EventArgs.Empty);
    }


    private void ScanFiles(IList<string> filesToScan, bool updateImages, CancellationToken cancellationToken)
    {
        var added = 0;
        var scanned = 0;
        //var stopwatch = new Stopwatch();
        //stopwatch.Start();

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

            scanned++;

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
                if (updateImages)
                {

                    added += _dataStore.UpdateImagesByPath(newImages, includeProperties, folderIdCache, cancellationToken);
                }
                else
                {
                    _dataStore.AddImages(newImages, includeProperties, folderIdCache, cancellationToken);
                    added += newImages.Count;
                }

                newImages.Clear();
            }

            if (scanned % 33 == 0)
            {
                ScanProgress?.Invoke(this, new ScanProgressEventArgs()
                {
                    Progress = scanned,
                    Total = total
                });
            }
        }

        if (newImages.Count > 0)
        {
            if (updateImages)
            {
                added += _dataStore.UpdateImagesByPath(newImages, includeProperties, folderIdCache, cancellationToken);
            }
            else
            {
                _dataStore.AddImages(newImages, includeProperties, folderIdCache, cancellationToken);
                added += newImages.Count;
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

        //stopwatch.Stop();

        //var elapsedTime = stopwatch.ElapsedMilliseconds / 1000f;
    }



}