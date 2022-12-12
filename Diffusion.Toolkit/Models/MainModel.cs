using System.Windows.Controls;
using System.Windows.Input;

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
    private ICommand _showModels;
    private ICommand _showSearch;
    private int _totalFilesScan;
    private int _currentPositionScan;
    private string _status;
    private bool _isScanning;
    private ICommand _cancelScan;
    private ICommand _about;
    private ICommand _help;
    private MessagePopupModel _messagePopupModel;
    private ICommand _showInfo;

    public MainModel()
    {
        _status = "Ready";
        _messagePopupModel = new MessagePopupModel();
    }

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

    public ICommand ShowModels
    {
        get => _showModels;
        set => SetField(ref _showModels, value);
    }

    public ICommand ShowSearch
    {
        get => _showSearch;
        set => SetField(ref _showSearch, value);
    }

    public string Status
    {
        get => _status;
        set => SetField(ref _status, value);
    }
    public bool IsScanning
    {
        get => _isScanning;
        set => SetField(ref _isScanning, value);
    }

    public int CurrentPositionScan
    {
        get => _currentPositionScan;
        set => SetField(ref _currentPositionScan, value);
    }

    public int TotalFilesScan
    {
        get => _totalFilesScan;
        set => SetField(ref _totalFilesScan, value);
    }
    
    public ICommand CancelScan
    {
        get => _cancelScan;
        set => SetField(ref _cancelScan, value);
    }

    public ICommand About
    {
        get => _about;
        set => SetField(ref _about, value);
    }

    public ICommand Help
    {
        get => _help;
        set => SetField(ref _help, value);
    }

    public ICommand ShowInfo
    {
        get => _showInfo;
        set => SetField(ref _showInfo, value);
    }
}