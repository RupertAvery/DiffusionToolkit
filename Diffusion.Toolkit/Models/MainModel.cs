﻿using System;
using System.Collections.Generic;
using Diffusion.Database;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;
using Diffusion.Common;
using WPFLocalizeExtension.Providers;

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
    private ICommand _sortAlbum;
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
    private IEnumerable<ModelViewModel> _imageModels;
    private IEnumerable<string> _imageModelNames;
    private ObservableCollection<FolderViewModel> _folders;
    public MainModel()
    {
        _status = "Ready";
        _isPreviewVisible = true;
        _messagePopupModel = new MessagePopupModel();
        _albums = new ObservableCollection<AlbumModel>()
        {
            new AlbumModel()
            {
                Name = "Album #12345",
                ImageCount = 12345
            }
        };
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

    public ICommand ToggleVisibilityCommand
    {
        get;
        set;
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
    public IEnumerable<ModelViewModel> ImageModels
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

    private FolderViewModel? _currentFolder;
    private ICommand _resetLayout;
    private ICommand _unavailableFilesCommand;

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

    public ICommand CreateFolderCommand { get; set; }
    public ICommand RenameFolderCommand { get; set; }
    public ICommand DeleteFolderCommand { get; set; }
    public ICommand ToggleNavigationPane { get; set; }
}