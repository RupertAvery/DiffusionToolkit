using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;
using Diffusion.Database.Models;
using Diffusion.Toolkit.Models;

namespace Diffusion.Toolkit;

public class AlbumSortModel : BaseNotify
{
    private ObservableCollection<Album> _albums;
    private ObservableCollection<Album> _sortedAlbums;
    private Album? _selectedAlbum;
    private string _sortAlbumsBy;
    public ICommand Escape { get; set; }

    public string SortAlbumsBy
    {
        get => _sortAlbumsBy;
        set => SetField(ref _sortAlbumsBy, value);
    }

    public Album? SelectedAlbum
    {
        get => _selectedAlbum;
        set => SetField(ref _selectedAlbum, value);
    }

    public ObservableCollection<Album> Albums
    {
        get => _albums;
        set => SetField(ref _albums, value);
    }

    public ObservableCollection<Album> SortedAlbums
    {
        get => _sortedAlbums;
        set => SetField(ref _sortedAlbums, value);
    }
    public ICommand MoveUpCommand { get; set; }
    public ICommand MoveDownCommand { get; set; }
}
