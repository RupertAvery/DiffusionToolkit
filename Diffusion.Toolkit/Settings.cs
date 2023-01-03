using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Diffusion.Toolkit;

public class Settings
{
    private List<string> _imagePaths;
    private string _modelRootPath;
    private string _fileExtensions;
    private int _pageSize;

    public Settings()
    {
        DontShowWelcomeOnStartup = false;
        ImagePaths = new List<string>();
        FileExtensions = ".png, .jpg";
        Theme = "System";
        PageSize = 100;
    }

    private bool _isDirty;

    public bool IsDirty()
    {
        return _isDirty;
    }

    private readonly Dictionary<string, bool> _isPropertyDirty = new Dictionary<string, bool>();
    private string _mainGridWidth;
    private string _previewGridHeight;
    private string _mainGridWidth2;
    private string _previewGridHeight2;
    private WindowState? _windowState;
    private Size? _windowSize;
    private bool _dontShowWelcomeOnStartup;
    private string _theme;
    private bool _watchFolders;
    private bool _hideNsfw;
    private bool _nsfwBlur;
    private bool _scanForNewImagesOnStartup;
    private bool _checkForUpdatesOnStartup;

    public bool IsPropertyDirty(string name)
    {
        return _isPropertyDirty.TryGetValue(name, out bool val) && val;
    }

    public void SetPristine()
    {
        _isDirty = false;
        _isPropertyDirty.Clear();
    }

    private void UpdateValue<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        _isPropertyDirty[propertyName] = true;
        _isDirty = true;
    }

    private void UpdateList<T>(ref List<T>? field, List<T>? value, [CallerMemberName] string propertyName = "")
    {
        bool hasDiff = false;
        if (field != null && value != null)
        {
            var firstNotSecond = field.Except(value).ToList();
            var secondNotFirst = value.Except(field).ToList();

            hasDiff = firstNotSecond.Any() || secondNotFirst.Any();
        }
        else if (field == null && value != null)
        {
            hasDiff = true;
        }
        else
        {
            hasDiff = value != null || value!.Any();
        }

        if (!hasDiff) return;
        field = value;
        _isPropertyDirty[propertyName] = true;
        _isDirty = true;
    }

    public List<string> ImagePaths
    {
        get => _imagePaths;
        set => UpdateList(ref _imagePaths, value);
    }

    public string ModelRootPath
    {
        get => _modelRootPath;
        set => UpdateValue(ref _modelRootPath, value);
    }

    public string FileExtensions
    {
        get => _fileExtensions;
        set => UpdateValue(ref _fileExtensions, value);
    }

    public int PageSize
    {
        get => _pageSize;
        set => UpdateValue(ref _pageSize, value);
    }

    public string MainGridWidth
    {
        get => _mainGridWidth;
        set => UpdateValue(ref _mainGridWidth, value);
    }

    public string MainGridWidth2
    {
        get => _mainGridWidth2;
        set => UpdateValue(ref _mainGridWidth2, value);
    }

    public WindowState? WindowState
    {
        get => _windowState;
        set => UpdateValue(ref _windowState, value);
    }

    public Size? WindowSize
    {
        get => _windowSize;
        set => UpdateValue(ref _windowSize, value);
    }


    public bool DontShowWelcomeOnStartup
    {
        get => _dontShowWelcomeOnStartup;
        set => UpdateValue(ref _dontShowWelcomeOnStartup, value);
    }

    public string Theme
    {
        get => _theme;
        set => UpdateValue(ref _theme, value);
    }

    public bool WatchFolders
    {
        get => _watchFolders;
        set => UpdateValue(ref _watchFolders, value);
    }

    public bool HideNSFW
    {
        get => _hideNsfw;
        set => UpdateValue(ref _hideNsfw, value);
    }

    public bool NSFWBlur
    {
        get => _nsfwBlur;
        set => UpdateValue(ref _nsfwBlur, value);
    }

    public bool CheckForUpdatesOnStartup
    {
        get => _checkForUpdatesOnStartup;
        set => UpdateValue(ref _checkForUpdatesOnStartup, value);
    }

    public bool ScanForNewImagesOnStartup
    {
        get => _scanForNewImagesOnStartup;
        set => UpdateValue(ref _scanForNewImagesOnStartup, value);
    }
}
