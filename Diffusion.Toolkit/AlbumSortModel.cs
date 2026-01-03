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
    public ICommand Escape { get; set; }

    public string SortAlbumsBy
    {
        get;
        set => SetField(ref field, value);
    }

    public Album? SelectedAlbum
    {
        get;
        set => SetField(ref field, value);
    }

    public ObservableCollection<Album> Albums
    {
        get;
        set => SetField(ref field, value);
    }

    public ObservableCollection<Album> SortedAlbums
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand MoveUpCommand { get; set; }
    public ICommand MoveDownCommand { get; set; }
}
