using System;
using System.Collections.Generic;
using Diffusion.Database;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;
using Diffusion.Common;
using Diffusion.Toolkit.Classes;
using Diffusion.Toolkit.Configuration;

namespace Diffusion.Toolkit.Models;

public class MainModel : BaseNotify
{
    private Page _page;
    private ICommand _rescan;
    private ICommand _closeCommand;
    private ICommand _settingsCommand;
    private ICommand _rebuild;
    private bool _showIcons;
    private bool _hideIcons;
    private ICommand _removeMarked;
    private int _totalProgress;
    private int _currentProgress;
    private string _status;
    private bool _isBusy;
    private ICommand _cancelCommand;
    private ICommand _aboutCommand;
    private ICommand _helpCommand;
    private ICommand _toggleInfoCommand;
    private ICommand _toggleNsfwBlurCommand;
    private ICommand _toggleHideNsfw;
    private ICommand _toggleHideDeleted;
    private bool _hideNsfw;
    private bool _hideDeleted;
    private bool _nsfwBlur;
    private bool _fitToPreview;
    private bool _actualSize;
    private ICommand _toggleFitToPreview;
    private ICommand _toggleActualSize;
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
    private ICommand _sortAlbum;
    private ICommand _clearAlbums;
    private ICommand _refresh;
    private ICommand _quickCopy;
    private int _thumbnailSize;
    private ICommand _escape;
    private ICommand _downloadCivitai;
    private ICommand _addAlbumCommand;
    private ICommand _addToAlbumCommand;
    private ICommand _removeFromAlbumCommand;
    private ICommand _renameAlbumCommand;
    private ICommand _removeAlbumCommand;
    private ObservableCollection<AlbumModel> _albums;
    private ObservableCollection<ImageEntry>? _selectedImages;
    private AlbumListItem? _selectedAlbum;
    private AlbumModel? _currentAlbum;
    private ICommand _fixFoldersCommand;
    private ICommand _removeExcludedImagesCommand;
    private ICommand _cleanRemovedFoldersCommand;
    private ICommand _showFilterCommand;
    private ICommand _toggleAutoRefresh;
    private bool _autoRefresh;
    private IEnumerable<ModelViewModel>? _imageModels;
    private IEnumerable<string> _imageModelNames;
    private ObservableCollection<FolderViewModel> _folders;


    private FolderViewModel? _currentFolder;
    private ICommand _resetLayout;
    private ICommand _unavailableFilesCommand;
    private bool _hideUnavailable;
    private ICommand _toggleHideUnavailable;
    private bool _autoAdvance;
    private ICommand _toggleAutoAdvance;
    private bool _hasSelectedAlbums;
    private ICommand _clearModelsCommand;
    private bool _hasSelectedModels;
    private int _selectedAlbumsCount;
    private int _selectedModelsCount;
    private string _toastMessage;
    private ICommand _showSettingsCommand;
    private ObservableCollection<QueryModel> _queries;
    private QueryModel? _selectedQuery;
    private ICommand _renameQueryCommand;
    private ICommand _removeQueryCommand;

    public MainModel()
    {
        _status = "Ready";
        _isPreviewVisible = true;
        _selectedImages = new ObservableCollection<ImageEntry>();
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

    public ICommand OpenWithCommand
    {
        get => _openWithCommand;
        set => SetField(ref _openWithCommand, value);
    }

    public ICommand SettingsCommand
    {
        get => _settingsCommand;
        set => SetField(ref _settingsCommand, value);
    }

    public ICommand CloseCommand
    {
        get => _closeCommand;
        set => SetField(ref _closeCommand, value);
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

    public ICommand GotoUrl
    {
        get => _gotoUrl;
        set => SetField(ref _gotoUrl, value);
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

    public ICommand ToggleVisibilityCommand
    {
        get;
        set;
    }

    private ICommand _toggleTags;

    public ICommand ToggleTagsCommand
    {
        get => _toggleTags;
        set => SetField(ref _toggleTags, value);
    }

    private bool _showTags;

    public bool ShowTags
    {
        get => _showTags;
        set => SetField(ref _showTags, value);
    }

    public bool AutoAdvance
    {
        get => _autoAdvance;
        set => SetField(ref _autoAdvance, value);
    }

    public ICommand ToggleAutoAdvance
    {
        get => _toggleAutoAdvance;
        set => SetField(ref _toggleAutoAdvance, value);
    }

    public ICommand ToggleHideNSFW
    {
        get => _toggleHideNsfw;
        set => SetField(ref _toggleHideNsfw, value);
    }

    public ICommand ToggleHideDeleted
    {
        get => _toggleHideDeleted;
        set => SetField(ref _toggleHideDeleted, value);
    }

    public ICommand ToggleHideUnavailable
    {
        get => _toggleHideUnavailable;
        set => SetField(ref _toggleHideUnavailable, value);
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

    public bool HideDeleted
    {
        get => _hideDeleted;
        set => SetField(ref _hideDeleted, value);
    }

    public bool HideUnavailable
    {
        get => _hideUnavailable;
        set => SetField(ref _hideUnavailable, value);
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

    public bool ActualSize
    {
        get => _actualSize;
        set => SetField(ref _actualSize, value);
    }

    public ICommand ToggleActualSize
    {
        get => _toggleActualSize;
        set => SetField(ref _toggleActualSize, value);
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

    public ICommand ResetLayout
    {
        get => _resetLayout;
        set => SetField(ref _resetLayout, value);
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

    public ICommand SaveQuery
    {
        get;
        set;
    }

    public ICommand RescanResults
    {
        get;
        set;
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

    public ICommand ClearModelsCommand
    {
        get => _clearModelsCommand;
        set => SetField(ref _clearModelsCommand, value);
    }

    public bool HasSelectedModels
    {
        get => _hasSelectedModels;
        set => SetField(ref _hasSelectedModels, value);
    }

    public ICommand ClearAlbumsCommand
    {
        get => _clearAlbums;
        set => SetField(ref _clearAlbums, value);
    }

    public bool HasSelectedAlbums
    {
        get => _hasSelectedAlbums;
        set => SetField(ref _hasSelectedAlbums, value);
    }

    public ICommand SortAlbumCommand
    {
        get => _sortAlbum;
        set => SetField(ref _sortAlbum, value);
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
    
    public ObservableCollection<AlbumModel> Albums
    {
        get => _albums;
        set => SetField(ref _albums, value);
    }


    public ObservableCollection<QueryModel> Queries
    {
        get => _queries;
        set => SetField(ref _queries, value);
    }

    public QueryModel? SelectedQuery
    {
        get => _selectedQuery;
        set => SetField(ref _selectedQuery, value);
    }

    public int SelectedAlbumsCount
    {
        get => _selectedAlbumsCount;
        set => SetField(ref _selectedAlbumsCount, value);
    }



    public int SelectedModelsCount
    {
        get => _selectedModelsCount;
        set => SetField(ref _selectedModelsCount, value);
    }

    public ICommand AddAlbumCommand
    {
        get => _addAlbumCommand;
        set => SetField(ref _addAlbumCommand, value);
    }

    public ICommand AddToAlbumCommand
    {
        get => _addToAlbumCommand;
        set => SetField(ref _addToAlbumCommand, value);
    }


    public ICommand RemoveFromAlbumCommand
    {
        get => _removeFromAlbumCommand;
        set => SetField(ref _removeFromAlbumCommand, value);
    }

    public ICommand RenameAlbumCommand
    {
        get => _renameAlbumCommand;
        set => SetField(ref _renameAlbumCommand, value);
    }

    public ICommand RemoveAlbumCommand
    {
        get => _removeAlbumCommand;
        set => SetField(ref _removeAlbumCommand, value);
    }


    public ICommand RenameQueryCommand
    {
        get => _renameQueryCommand;
        set => SetField(ref _renameQueryCommand, value);
    }

    public ICommand RemoveQueryCommand
    {
        get => _removeQueryCommand;
        set => SetField(ref _removeQueryCommand, value);
    }


    public ObservableCollection<ImageEntry>? SelectedImages
    {
        get => _selectedImages;
        set => SetField(ref _selectedImages, value);
    }

    public AlbumListItem? SelectedAlbum
    {
        get => _selectedAlbum;
        set => SetField(ref _selectedAlbum, value);
    }

    public AlbumModel? CurrentAlbum
    {
        get => _currentAlbum;
        set => SetField(ref _currentAlbum, value);
    }

    private ModelViewModel? _currentModel;
    private string _activeView;
    private Settings _settings;
    private ICommand _reloadFoldersCommand;
    private int _progressTarget;
    private bool _isSettingsDirty;
    private ObservableCollection<Control> _openWithMenuItems;
    private ICommand _openWithCommand;
    private ICommand _gotoUrl;
    private bool _foldersBusy;
    private bool _showNotifications;
    private bool _permanentlyDelete;
    private ObservableCollection<Control> _albumMenuItems;
    private ObservableCollection<Control> _selectionAlbumMenuItems;

    public ModelViewModel? CurrentModel
    {
        get => _currentModel;
        set => SetField(ref _currentModel, value);
    }

    public ICommand CreateAlbumCommand { get; set; }
    public Action<IAlbumInfo> AddSelectedImagesToAlbum { get; set; }


    public ICommand FixFoldersCommand
    {
        get => _fixFoldersCommand;
        set => SetField(ref _fixFoldersCommand, value);
    }

    public ICommand RemoveExcludedImagesCommand
    {
        get => _removeExcludedImagesCommand;
        set => SetField(ref _removeExcludedImagesCommand, value);
    }

    public ICommand CleanRemovedFoldersCommand
    {
        get => _cleanRemovedFoldersCommand;
        set => SetField(ref _cleanRemovedFoldersCommand, value);
    }

    public ICommand UnavailableFilesCommand
    {
        get => _unavailableFilesCommand;
        set => SetField(ref _unavailableFilesCommand, value);
    }


    public ICommand ShowFilterCommand
    {
        get => _showFilterCommand;
        set => SetField(ref _showFilterCommand, value);
    }

    public ICommand ToggleAutoRefresh
    {
        get => _toggleAutoRefresh;
        set => SetField(ref _toggleAutoRefresh, value);
    }

    public bool AutoRefresh
    {
        get => _autoRefresh;
        set => SetField(ref _autoRefresh, value);
    }


    public ObservableCollection<FolderViewModel> Folders
    {
        get => _folders;
        set => SetField(ref _folders, value);
    }

    public IEnumerable<ModelViewModel>? ImageModels
    {
        get => _imageModels;
        set => SetField(ref _imageModels, value);
    }

    public IEnumerable<string> ImageModelNames
    {
        get => _imageModelNames;
        set => SetField(ref _imageModelNames, value);
    }

    public string ActiveView
    {
        get => _activeView;
        set => SetField(ref _activeView, value);
    }

    public Settings Settings
    {
        get => _settings;
        set => SetField(ref _settings, value);
    }

    public Action<FolderViewModel> MoveSelectedImagesToFolder { get; set; }


    public FolderViewModel? CurrentFolder
    {
        get => _currentFolder;
        set => SetField(ref _currentFolder, value);
    }

    public ICommand ReloadFoldersCommand
    {
        get => _reloadFoldersCommand;
        set => SetField(ref _reloadFoldersCommand, value);
    }

    public ICommand ScanFolderCommand { get; set; }
    public ICommand CreateFolderCommand { get; set; }
    public ICommand RenameFolderCommand { get; set; }
    public ICommand DeleteFolderCommand { get; set; }
    public ICommand ArchiveFolderCommand { get; set; }
    public ICommand ArchiveFolderRecursiveCommand { get; set; }
    public ICommand ExcludeFolderCommand { get; set; }

    

    public ICommand ToggleNavigationPane { get; set; }
    public ICommand ShowInExplorerCommand { get; set; }
    public string ToastMessage
    {
        get => _toastMessage;
        set => SetField(ref _toastMessage, value);
    }

    public QueryModel CurrentQuery { get; set; }

    public bool IsSettingsDirty
    {
        get => _isSettingsDirty;
        set => SetField(ref _isSettingsDirty, value);
    }

    public ObservableCollection<Control> OpenWithMenuItems
    {
        get => _openWithMenuItems;
        set => SetField(ref _openWithMenuItems, value);
    }

    public ImageViewModel CurrentImage { get; set; }

    public bool FoldersBusy
    {
        get => _foldersBusy;
        set => SetField(ref _foldersBusy, value);
    }

    public bool ShowNotifications
    {
        get => _showNotifications;
        set => SetField(ref _showNotifications, value);
    }

    public ICommand ToggleNotificationsCommand { get; set; }

    public bool PermanentlyDelete
    {
        get => _permanentlyDelete;
        set => SetField(ref _permanentlyDelete, value);
    }

    public ObservableCollection<Control> AlbumMenuItems
    {
        get => _albumMenuItems;
        set => SetField(ref _albumMenuItems, value);
    }

    public ObservableCollection<Control> SelectionAlbumMenuItems
    {
        get => _selectionAlbumMenuItems;
        set => SetField(ref _selectionAlbumMenuItems, value);
    }
}