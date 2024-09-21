using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Diffusion.Database;
using Diffusion.IO;
using Diffusion.Toolkit.Localization;

namespace Diffusion.Toolkit.Services;

public class ScanningService
{
    private CancellationTokenSource _progressCancellationTokenSource;

    public event EventHandler<ScanningEventArgs> ScanProgress;

    private string GetLocalizedText(string key)
    {
        return (string)JsonLocalizationProvider.Instance.GetLocalizedObject(key, null, CultureInfo.InvariantCulture);
    }

    public void StartTask()
    {
        _progressCancellationTokenSource = new CancellationTokenSource();
    }

    public CancellationToken CancellationToken => _progressCancellationTokenSource.Token;

    public void EndTask()
    {
        _progressCancellationTokenSource.Cancel();
    }

    public void Scan(ICollection<string> filesToScan, bool updateImages)
    {
        StartTask();

        var _settings = ServiceLocator.Settings;
        var _dataStore = ServiceLocator.DataStore;
        var added = 0;
        var scanned = 0;

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var max = filesToScan.Count;

        ScanProgress?.Invoke(this, new ScanningEventArgs() { TotalCount = max });

        var folderIdCache = new Dictionary<string, int>();

        var newImages = new List<Image>();

        var includeProperties = new List<string>();

        if (_settings.AutoTagNSFW)
        {
            includeProperties.Add(nameof(Image.NSFW));
        }

        var scanning = GetLocalizedText("Actions.Scanning.Status");

        foreach (var file in MetadataScanner.Scan(filesToScan))
        {
            if (CancellationToken.IsCancellationRequested)
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
                    NoMetadata = file.NoMetadata,
                    Workflow = file.Workflow,
                    WorkflowId = file.WorkflowId,
                    HasError = file.HasError
                };

                if (!string.IsNullOrEmpty(file.HyperNetwork) && !file.HyperNetworkStrength.HasValue)
                {
                    file.HyperNetworkStrength = 1;
                }

                if (_settings.AutoTagNSFW)
                {
                    if (_settings.NSFWTags.Any(t => image.Prompt != null && image.Prompt.ToLower().Contains((string)t.Trim().ToLower())))
                    {
                        image.NSFW = true;
                    }
                }

                newImages.Add(image);
            }

            if (newImages.Count == 100)
            {
                if (updateImages)
                {

                    added += _dataStore.UpdateImagesByPath(newImages, includeProperties, folderIdCache, CancellationToken);
                }
                else
                {
                    _dataStore.AddImages(newImages, includeProperties, folderIdCache, CancellationToken);
                    added += newImages.Count;
                }

                newImages.Clear();
            }

            if (scanned % 33 == 0)
            {
                ScanProgress?.Invoke(this, new ScanningEventArgs()
                {
                    Type = ScanningEventType.Progress, 
                    TotalCount = max, 
                    ProgressCount = scanned
                });

                //Dispatcher.Invoke(() =>
                //{
                //    _model.CurrentProgress = scanned;

                //    var text = scanning
                //        .Replace("{current}", $"{_model.CurrentProgress:#,###,##0}")
                //        .Replace("{total}", $"{_model.TotalProgress:#,###,##0}");

                //    _model.Status = text;
                //});
            }
        }

        if (newImages.Count > 0)
        {
            if (updateImages)
            {
                added += _dataStore.UpdateImagesByPath(newImages, includeProperties, folderIdCache, CancellationToken);
            }
            else
            {
                _dataStore.AddImages(newImages, includeProperties, folderIdCache, CancellationToken);
                added += newImages.Count;
            }
        }

        ScanProgress?.Invoke(this, new ScanningEventArgs()
        {
            Type = ScanningEventType.Complete,
            TotalCount = max,
            ProgressCount = max
        });

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

        var elapsedTime = stopwatch.ElapsedMilliseconds / 1000f;


        //return (added, elapsedTime);
    }
}