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
    private int _totalProgress;
    private int _currentProgress;
    private string _status;
    private bool _isBusy;
    private ICommand _cancelCommand;
    private ICommand _aboutCommand;
    private ICommand _helpCommand;
    private MessagePopupModel _messagePopupModel;
    private ICommand _toggleInfoCommand;
    private ICommand _toggleNsfwBlurCommand;
    private ICommand _toggleHideNsfw;
    private bool _hideNsfwCommand;
    private bool _nsfwBlurCommand;
    private ICommand _showPromptsCommand;
    private bool _fitToPreview;
    private ICommand _toggleFitToPreview;
    private ICommand _setThumbnailSize;
    private ICommand _poputPreview;
    private ICommand _togglePreview;
    private bool _isPreviewVisible;
    private ICommand _addAllToAlbum;
    private ICommand _markAllForDeletion;
    private ICommand _unmarkAllForDeletion;
    private ICommand _removeMatching;
    private ICommand _autoTagNsfw;
    private ICommand _reloadHashes;
    private ICommand _showFolders;
    private ICommand _showAlbums;
    private ICommand _addMatchingToAlbum;
    private bool _showAlbumPanel;
    private ICommand _toggleAlbum;
    private ICommand _refresh;
    private ICommand _quickCopy;
    private int _thumbnailSize;
    private ICommand _escape;
    private ICommand _downloadCivitai;

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

    public ICommand ShowFolders
    {
        get => _showFolders;
        set => SetField(ref _showFolders, value);
    }


    public ICommand ShowAlbums
    {
        get => _showAlbums;
        set => SetField(ref _showAlbums, value);
    }

    public string Status
    {
        get => _status;
        set => SetField(ref _status, value);
    }
    public bool IsBusy
    {
        get => _isBusy;
        set => SetField(ref _isBusy, value);
    }

    public int CurrentProgress
    {
        get => _currentProgress;
        set => SetField(ref _currentProgress, value);
    }

    public int TotalProgress
    {
        get => _totalProgress;
        set => SetField(ref _totalProgress, value);
    }
    
    public ICommand CancelCommand
    {
        get => _cancelCommand;
        set => SetField(ref _cancelCommand, value);
    }

    public ICommand AboutCommand
    {
        get => _aboutCommand;
        set => SetField(ref _aboutCommand, value);
    }

    public ICommand HelpCommand
    {
        get => _helpCommand;
        set => SetField(ref _helpCommand, value);
    }

    public ICommand ToggleInfoCommand
    {
        get => _toggleInfoCommand;
        set => SetField(ref _toggleInfoCommand, value);
    }

    public ICommand ToggleNSFWBlurCommand
    {
        get => _toggleNsfwBlurCommand;
        set => SetField(ref _toggleNsfwBlurCommand, value);
    }

    public ICommand ToggleHideNSFW
    {
        get => _toggleHideNsfw;
        set => SetField(ref _toggleHideNsfw, value);
    }

    public bool NSFWBlurCommand
    {
        get => _nsfwBlurCommand;
        set => SetField(ref _nsfwBlurCommand, value);
    }

    public bool HideNSFWCommand
    {
        get => _hideNsfwCommand;
        set => SetField(ref _hideNsfwCommand, value);
    }

    public ICommand ShowPromptsCommand
    {
        get => _showPromptsCommand;
        set => SetField(ref _showPromptsCommand, value);
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

    public ICommand AddAllToAlbum
    {
        get => _addAllToAlbum;
        set => SetField(ref _addAllToAlbum, value);
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

    public ICommand AddMatchingToAlbum
    {
        get => _addMatchingToAlbum;
        set => SetField(ref _addMatchingToAlbum, value);
    }

    public bool ShowAlbumPanel
    {
        get => _showAlbumPanel;
        set => SetField(ref _showAlbumPanel, value);
    }

    public ICommand ToggleAlbum
    {
        get => _toggleAlbum;
        set => SetField(ref _toggleAlbum, value);
    }

    public ICommand Refresh
    {
        get => _refresh;
        set => SetField(ref _refresh, value);
    }

    public ICommand QuickCopy
    {
        get => _quickCopy;
        set => SetField(ref _quickCopy, value);
    }

    public int ThumbnailSize
    {
        get => _thumbnailSize;
        set => SetField(ref _thumbnailSize, value);
    }

    public ICommand Escape
    {
        get => _escape;
        set => SetField(ref _escape, value);
    }

    public ICommand DownloadCivitai
    {
        get => _downloadCivitai;
        set => SetField(ref _downloadCivitai, value);
    }
}