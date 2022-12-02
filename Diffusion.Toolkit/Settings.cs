using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Diffusion.Toolkit;

public class Settings
{
    private List<string> _imagePaths;
    private string _modelRootPath;
    private string _fileExtensions;
    private int _pageSize;

    public Settings()
    {
        ImagePaths = new List<string>();
        FileExtensions = ".png, .jpg";
        PageSize = 100;
    }

    public bool IsDirty { get; private set; }
    private readonly Dictionary<string, bool> _isPropertyDirty = new Dictionary<string, bool>();

    public bool IsPropertyDirty(string name)
    {
        return _isPropertyDirty.TryGetValue(name, out bool val) && val;
    }

    public void SetPristine()
    {
        IsDirty = false;
        _isPropertyDirty.Clear();
    }

    private void UpdateValue<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        _isPropertyDirty[propertyName] = true;
        IsDirty = true;
    }

    public List<string> ImagePaths
    {
        get => _imagePaths;
        set => UpdateValue(ref _imagePaths, value);
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
}