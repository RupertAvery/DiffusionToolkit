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
using DiffusionToolkit.AvaloniaApp.Services;
using static System.Net.Mime.MediaTypeNames;

namespace DiffusionToolkit.AvaloniaApp.Pages.Settings;

public partial class SettingsPage : UserControl, INavigationTarget
{
    private SettingsPageViewModel _viewModel = new SettingsPageViewModel();
    private readonly ScanService _scanService;

    public SettingsPage()
    {
        _viewModel = new SettingsPageViewModel();
        _scanService = ServiceLocator.ScanService;
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

        if (_viewModel.IsRescanRequired)
        {
            Task.Run(() =>
            {
                _scanService.ScanFolders();
            });
        }
    }
}