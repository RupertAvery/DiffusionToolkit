using System.Collections.ObjectModel;

namespace Diffusion.Toolkit;

public class SettingsModel : BaseNotify
{
    private string _modelRootPath;
    private ObservableCollection<string> _imagePaths;
    private int _selectedIndex;
    private string _fileExtensions;

    public SettingsModel()
    {
        _imagePaths = new ObservableCollection<string>();
    }

    public ObservableCollection<string> ImagePaths
    {
        get => _imagePaths;
        set => SetField(ref _imagePaths, value);
    }

    public int SelectedIndex
    {
        get => _selectedIndex;
        set => SetField(ref _selectedIndex, value);
    }

    public string FileExtensions
    {
        get => _fileExtensions;
        set => SetField(ref _fileExtensions, value);
    }

    public string ModelRootPath
    {
        get => _modelRootPath;
        set => SetField(ref _modelRootPath, value);
    }
}