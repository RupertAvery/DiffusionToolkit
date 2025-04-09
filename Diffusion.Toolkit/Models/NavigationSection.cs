using Diffusion.Toolkit.Configuration;

namespace Diffusion.Toolkit.Models;

public class NavigationSection : BaseNotify
{
    private AccordionState _folderState;
    private AccordionState _modelState;
    private AccordionState _albumState;
    private AccordionState _queryState;
    private bool _showFolders;
    private bool _showModels;
    private bool _showAlbums;
    private bool _showQueries;
    private FoldersSection? _foldersSection;
    private double _folderHeight;
    private double _albumHeight;
    private double _modelHeight;
    private double _queryHeight;

    public NavigationSection()
    {
        FoldersSection = new FoldersSection();
    }

    public bool ShowFolders
    {
        get => _showFolders;
        set => SetField(ref _showFolders, value);
    }

    public bool ShowModels
    {
        get => _showModels;
        set => SetField(ref _showModels, value);
    }

    public bool ShowAlbums
    {
        get => _showAlbums;
        set => SetField(ref _showAlbums, value);
    }

    public bool ShowQueries
    {
        get => _showQueries;
        set => SetField(ref _showQueries, value);
    }

    public AccordionState FolderState
    {
        get => _folderState;
        set => SetField(ref _folderState, value);
    }

    public double FolderHeight
    {
        get => _folderHeight;
        set => SetField(ref _folderHeight, value);
    }


    public AccordionState ModelState
    {
        get => _modelState;
        set => SetField(ref _modelState, value);
    }

    public double ModelHeight
    {
        get => _modelHeight;
        set => SetField(ref _modelHeight, value);
    }

    public AccordionState AlbumState
    {
        get => _albumState;
        set => SetField(ref _albumState, value);
    }

    public double AlbumHeight
    {
        get => _albumHeight;
        set => SetField(ref _albumHeight, value);
    }

    public AccordionState QueryState
    {
        get => _queryState;
        set => SetField(ref _queryState, value);
    }

    public double QueryHeight
    {
        get => _queryHeight;
        set => SetField(ref _queryHeight, value);
    }

    public FoldersSection FoldersSection
    {
        get => _foldersSection;
        set => SetField(ref _foldersSection, value);
    }
}