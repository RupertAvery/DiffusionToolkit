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
    private ICommand _toggleInfo;
    private ICommand _toggleNsfwBlur;
    private ICommand _toggleHideNsfw;
    private bool _hideNsfw;
    private bool _nsfwBlur;
    private ICommand _showPrompts;
    private bool _fitToPreview;
    private ICommand _toggleFitToPreview;
    private ICommand _setThumbnailSize;
    private ICommand _poputPreview;
    private ICommand _togglePreview;
    private bool _isPreviewVisible;
    private ICommand _markAllForDeletion;
    private ICommand _unmarkAllForDeletion;
    private ICommand _removeMatching;
    private ICommand _autoTagNsfw;
    private ICommand _reloadHashes;

    public MainModel()
    {
        _status = "Ready";
        _isPreviewVisible = true;
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

    public ICommand ReloadHashes
    {
        get => _reloadHashes;
        set => SetField(ref _reloadHashes, value);
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

    public ICommand ToggleInfo
    {
        get => _toggleInfo;
        set => SetField(ref _toggleInfo, value);
    }

    public ICommand ToggleNSFWBlur
    {
        get => _toggleNsfwBlur;
        set => SetField(ref _toggleNsfwBlur, value);
    }

    public ICommand ToggleHideNSFW
    {
        get => _toggleHideNsfw;
        set => SetField(ref _toggleHideNsfw, value);
    }

    public bool NSFWBlur
    {
        get => _nsfwBlur;
        set => SetField(ref _nsfwBlur, value);
    }

    public bool HideNSFW
    {
        get => _hideNsfw;
        set => SetField(ref _hideNsfw, value);
    }

    public ICommand ShowPrompts
    {
        get => _showPrompts;
        set => SetField(ref _showPrompts, value);
    }

    public bool FitToPreview
    {
        get => _fitToPreview;
        set => SetField(ref _fitToPreview, value);
    }


    public ICommand ToggleFitToPreview
    {
        get => _toggleFitToPreview;
        set => SetField(ref _toggleFitToPreview, value);
    }

    public ICommand SetThumbnailSize
    {
        get => _setThumbnailSize;
        set => SetField(ref _setThumbnailSize, value);
    }

    public ICommand PoputPreview
    {
        get => _poputPreview;
        set => SetField(ref _poputPreview, value);
    }

    public ICommand TogglePreview
    {
        get => _togglePreview;
        set => SetField(ref _togglePreview, value);
    }

    public bool IsPreviewVisible
    {
        get => _isPreviewVisible;
        set => SetField(ref _isPreviewVisible, value);
    }

    public ICommand MarkAllForDeletion
    {
        get => _markAllForDeletion;
        set => SetField(ref _markAllForDeletion, value);
    }

    public ICommand UnmarkAllForDeletion
    {
        get => _unmarkAllForDeletion;
        set => SetField(ref _unmarkAllForDeletion, value);
    }

    public ICommand RemoveMatching
    {
        get => _removeMatching;
        set => SetField(ref _removeMatching, value);
    }

    public ICommand AutoTagNSFW
    {
        get => _autoTagNsfw;
        set => SetField(ref _autoTagNsfw, value);
    }
}