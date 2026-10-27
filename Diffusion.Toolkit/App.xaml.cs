using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Windows;

namespace Diffusion.Toolkit
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly object _logLock = new();
        private static StreamWriter? _log;

        [ThreadStatic] private static bool _inHandler;

        public App()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "firstchance.log");
            _log = new StreamWriter(path, append: false) { AutoFlush = true };
            _log.WriteLine($"=== Started {DateTime.Now:O} ===");

            AppDomain.CurrentDomain.FirstChanceException += OnFirstChance;
        }

        private static void OnFirstChance(object? sender, FirstChanceExceptionEventArgs e)
        {
            // Logging can itself throw; don't recurse.
            if (_inHandler) return;
            _inHandler = true;
            try
            {
                lock (_logLock)
                {
                    _log?.WriteLine($"--- {DateTime.Now:HH:mm:ss.fff} [thread {Environment.CurrentManagedThreadId}]");
                    _log?.WriteLine($"{e.Exception.GetType().FullName}: {e.Exception.Message}");
                    // At first-chance time, e.Exception.StackTrace only has the throwing frame,
                    // so capture the full current stack instead.
                    _log?.WriteLine(new StackTrace(1, true).ToString());
                }
            }
            catch { /* never let logging break the app */ }
            finally
            {
                _inHandler = false;
            }
        }
    }


}
