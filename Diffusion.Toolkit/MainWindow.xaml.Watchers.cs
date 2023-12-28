using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Diffusion.Toolkit
{
    public partial class MainWindow
    {
        private readonly List<FileSystemWatcher> _watchers = new List<FileSystemWatcher>();
        private List<string>? _detectedFiles;
        private Timer? t = null;
        private object _lock = new object();

        private async void OnActivated(object? sender, EventArgs e)
        {
            await Task.Delay(500);

            if (addedTotal > 0)
            {
                Report(addedTotal, 0, 0, false);
                lock (_lock)
                {
                    addedTotal = 0;
                }
            }

        }

        private void WatcherOnCreated(object sender, FileSystemEventArgs e)
        {
            if (_settings.FileExtensions.IndexOf(Path.GetExtension(e.FullPath), StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                QueueFile(e.FullPath);
            }
        }

        private void WatcherOnRenamed(object sender, RenamedEventArgs e)
        {
            var wasTmp = Path.GetExtension(e.OldFullPath).ToLowerInvariant() == ".tmp";
            
            if (wasTmp && _settings.FileExtensions.IndexOf(Path.GetExtension(e.FullPath), StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                QueueFile(e.FullPath);
            }
        }

        private void CreateWatchers()
        {
            foreach (var path in _settings.ImagePaths)
            {
                if (Directory.Exists(path))
                {
                    var watcher = new FileSystemWatcher(path)
                    {
                        EnableRaisingEvents = true,
                        IncludeSubdirectories = _settings.RecurseFolders.GetValueOrDefault(true)
                    };
                    watcher.Created += WatcherOnCreated;
                    watcher.Renamed += WatcherOnRenamed;
                    _watchers.Add(watcher);
                }
            }
        }

        private void RemoveWatchers()
        {
            foreach (var watcher in _watchers)
            {
                watcher.Dispose();
            }
            _watchers.Clear();
        }
        
        private void QueueFile(string path)
        {
            lock (_lock)
            {
                if (t == null)
                {
                    _detectedFiles = new List<string>();
                    t = new Timer(ProcessQueueCallback, null, 2000, Timeout.Infinite);
                }
                else
                {
                    t.Change(2000, Timeout.Infinite);
                }
                _detectedFiles.Add(path);
            }
        }

        private int addedTotal = 0;

        private void ProcessQueueCallback(object? state)
        {
            int added;
            float elapsed;

            lock (_lock)
            {
                t?.Dispose();
                t = null;

                var filteredFiles = _detectedFiles.Where(f => !_settings.ExcludePaths.Any(p => f.StartsWith(p))).ToList();

                (added, elapsed) = ScanFiles(filteredFiles, false, CancellationToken.None);
            }

            if (added > 0)
            {
                Dispatcher.Invoke(() =>
                {
                    var currentWindow = Application.Current.Windows.OfType<Window>().First();
                    if (currentWindow.IsActive)
                    {
                        Report(added, 0, elapsed, false);
                    }
                    else
                    {
                        lock (_lock)
                        {
                            addedTotal += added;
                        }
                    }
                });

                if (_settings.AutoRefresh)
                {
                    _search.ReloadMatches(null);
                }
            }
        }

    }
}
