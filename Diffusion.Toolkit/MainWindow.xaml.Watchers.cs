using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;

namespace Diffusion.Toolkit
{
    public partial class MainWindow
    {

        private void OnActivated(object? sender, EventArgs e)
        {

            if (addedTotal > 0)
            {
                Report(addedTotal, 0, 0, false);
                lock (_lock)
                {
                    addedTotal = 0;
                }
            }

        }

        private PreviewWindow? _previewWindow;

        private void WatcherOnCreated(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                if (_settings.FileExtensions.IndexOf(Path.GetExtension(e.FullPath)) > -1)
                {
                    AddFile(e.FullPath);
                }
            }
            else if (e.ChangeType == WatcherChangeTypes.Renamed)
            {
                if (_settings.FileExtensions.IndexOf(Path.GetExtension(e.FullPath)) > -1)
                {
                    AddFile(e.FullPath);
                }
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
                        IncludeSubdirectories = true,
                    };
                    watcher.Created += WatcherOnCreated;
                    _watchers.Add(watcher);
                }
            }
        }

        private void RemoveWatchers()
        {
            if (_watchers != null)
            {
                foreach (var watcher in _watchers)
                {
                    watcher.Dispose();
                }
                _watchers.Clear();
            }
        }


        private List<FileSystemWatcher> _watchers = new List<FileSystemWatcher>();

        private List<string> detectedFiles;

        private Timer? t = null;
        private object _lock = new object();

        private void AddFile(string path)
        {
            lock (_lock)
            {
                if (t == null)
                {
                    detectedFiles = new List<string>();
                    t = new Timer(Callback, null, 2000, Timeout.Infinite);
                }
                else
                {
                    t.Change(2000, Timeout.Infinite);
                }
                detectedFiles.Add(path);
            }
        }

        private int addedTotal = 0;

        private void Callback(object? state)
        {
            int added;
            float elapsed;

            lock (_lock)
            {
                t?.Dispose();
                t = null;
                (added, elapsed) = ScanFiles(detectedFiles.Where(f => !_settings.ExcludePaths.Any(p => f.StartsWith(p))).ToList(), false, CancellationToken.None);
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


            }

        }


    }
}
