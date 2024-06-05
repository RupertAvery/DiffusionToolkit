using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using DiffusionToolkit.AvaloniaApp.Common;

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

        _viewModel.SelectFolder = SelectFolder;
    }

    private async Task<string> SelectFolder()
    {
        // Get top level from the current control. Alternatively, you can use Window reference instead.
        var topLevel = TopLevel.GetTopLevel(this);

        // Start async operation to open the dialog.
        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            Title = "Select Folder File",
            AllowMultiple = false
        });

        if (folders != null)
        {
            var folder = folders.First();
            return folder.Path.AbsolutePath;
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
            _ = _scanManager.ScanFolders(_viewModel.IncludedFolders, _viewModel.ExcludedFolders, _viewModel.RecurseFolders);
        }
    }
}