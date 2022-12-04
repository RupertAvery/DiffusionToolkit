using System.Windows.Controls;
using System.Windows.Input;
using Diffusion.Toolkit.Classes;

namespace Diffusion.Toolkit.Models;

public class MainModel : BaseNotify
{
    private Page _page;
    private ICommand _rescan;
    private ICommand _close;
    private ICommand _settings;
    private ICommand _rebuild;
    private bool _showIcons;
    private bool _hideIcons;
    private ICommand _removeMarked;
    private ICommand _showFavorite;
    private ICommand _showMarked;
    private ICommand _showLastQuery;

    public Page Page
    {
        get => _page;
        set => SetField(ref _page, value);
    }

    public ICommand Rescan
    {
        get => _rescan;
        set => SetField(ref _rescan, value);
    }

    public ICommand RemoveMarked
    {
        get => _removeMarked;
        set => SetField(ref _removeMarked, value);
    }


    public ICommand Settings
    {
        get => _settings;
        set => SetField(ref _settings, value);
    }

    public ICommand Close
    {
        get => _close;
        set => SetField(ref _close, value);
    }

    public ICommand Rebuild
    {
        get => _rebuild;
        set => SetField(ref _rebuild, value);
    }

    public bool ShowIcons
    {
        get => _showIcons;
        set => SetField(ref _showIcons, value);
    }

    public bool HideIcons
    {
        get => _hideIcons;
        set => SetField(ref _hideIcons, value);
    }

    public ICommand ShowFavorite
    {
        get => _showFavorite;
        set => SetField(ref _showFavorite, value);
    }

    public ICommand ShowMarked
    {
        get => _showMarked;
        set => SetField(ref _showMarked, value);
    }

    public ICommand ShowLastQuery
    {
        get => _showLastQuery;
        set => SetField(ref _showLastQuery, value);
    }
}