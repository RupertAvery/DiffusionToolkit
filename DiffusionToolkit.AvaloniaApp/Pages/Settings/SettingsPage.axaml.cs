using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using DiffusionToolkit.AvaloniaApp.Common;
using static System.Net.Mime.MediaTypeNames;

namespace DiffusionToolkit.AvaloniaApp.Pages.Settings;

public partial class SettingsPage : UserControl, INavigationTarget
{
    private SettingsPageViewModel _viewModel = new SettingsPageViewModel();
    private readonly ScanManager _scanManager;

    public SettingsPage()
    {
        _viewModel = new SettingsPageViewModel();
        _scanManager = ServiceLocator.ScanManager;
        InitializeComponent();
        DataContext = _viewModel;

        Loaded += OnLoaded;

        _viewModel.SelectFolderDelegate = SelectFolder;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        _viewModel.LoadSettings(ServiceLocator.Settings);
    }

    private async Task<string> SelectFolder()
    {
        // Get top level from the current control. Alternatively, you can use Window reference instead.
        var topLevel = TopLevel.GetTopLevel(this);

        // Start async operation to open the dialog.
        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            Title = "Select Folder File",
            AllowMultiple = false,
        });

        if (folders != null)
        {
            var folder = folders.First();
            var path = folder.Path.LocalPath;

            return path;

            //if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            //{
            //    return path.Replace("/", "\\").Replace("%20", " ");
            //}
            //else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            //{
            //    return path.Replace("%20", " ");
            //}
            //else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            //{
            //    return path.Replace("%20", " ");
            //}
        }

        return null;
    }

    public void Activate()
    {

    }

    public void Deactivate()
    {
        ServiceLocator.SetSettings(new AvaloniaApp.Settings()
        {
            IncludedFolders = _viewModel.IncludedFolders,
            ExcludedFolders = _viewModel.ExcludedFolders,
            RecurseFolders = _viewModel.RecurseFolders
        });

        if (_viewModel.IsRescanRequired)
        {
            Task.Run(() =>
            {
                _scanManager.ScanFolders(_viewModel.IncludedFolders, _viewModel.ExcludedFolders, _viewModel.RecurseFolders);
            });
        }
    }
}