using System.Collections.Generic;
using System.Windows.Input;
using Diffusion.Database.Models;
using Diffusion.Toolkit.Models;

namespace Diffusion.Toolkit;

public class AlbumListModel : BaseNotify
{
    private string _albumName;
    private bool _isNewAlbum;
    private bool _isExistingAlbum;
    private Album? _selectedAlbum;
    private IEnumerable<Album> _albums;
    public ICommand Escape { get; set; }

    public bool IsNewAlbum
    {
        get => _isNewAlbum;
        set => SetField(ref _isNewAlbum, value);
    }

    public bool IsExistingAlbum
    {
        get => _isExistingAlbum;
        set => SetField(ref _isExistingAlbum, value);
    }

    public Album? SelectedAlbum
    {
        get => _selectedAlbum;
        set => SetField(ref _selectedAlbum, value);
    }

    public string AlbumName
    {
        get => _albumName;
        set => SetField(ref _albumName, value);
    }

    public IEnumerable<Album> Albums
    {
        get => _albums;
        set => SetField(ref _albums, value);
    }
}