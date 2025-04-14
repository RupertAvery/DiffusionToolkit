using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Diffusion.Toolkit.Localization;

namespace Diffusion.Toolkit.Services;

public class AlbumService
{
    private string GetLocalizedText(string key)
    {
        return (string)JsonLocalizationProvider.Instance.GetLocalizedObject(key, null, CultureInfo.InvariantCulture);
    }

    public void UpdateSelectedImageAlbums()
    {
        var ids = ServiceLocator.MainModel.SelectedImages.Select(d => d.Id).ToList();

        ServiceLocator.MainModel.SelectionAlbumMenuItems = new ObservableCollection<Control>();

        if (ids.Any())
        {
            var albums = ServiceLocator.DataStore.GetImageAlbums(ids);

            foreach (var album in albums)
            {
                var menuItem = new MenuItem() { Header = album.Name, Tag = album };
                menuItem.Click += RemoveFromAlbum_OnClick;
                ServiceLocator.MainModel.SelectionAlbumMenuItems.Add(menuItem);
            }
        }
    }

    public void ReloadContextMenus()
    {
        var albumMenuItem = new MenuItem()
        {
            Header = GetLocalizedText("Thumbnail.ContextMenu.AddToAlbum.NewAlbum"),
        };

        albumMenuItem.Click += CreateAlbum_OnClick;

        //var refreshAlbumMenuItem = new MenuItem()
        //{
        //    Header = GetLocalizedText("Menu.View.Refresh"),
        //};
        //refreshAlbumMenuItem.Click += RefreshAlbum_OnClick;

        var menuItems = new List<Control>()
        {
            albumMenuItem,
            //refreshAlbumMenuItem,
            new Separator()
        };


        var albums = ServiceLocator.DataStore.GetAlbumsByName();

        foreach (var album in albums)
        {
            var menuItem = new MenuItem() { Header = album.Name, Tag = album };
            menuItem.Click += AddToAlbum_OnClick;
            menuItems.Add(menuItem);
        }

        ServiceLocator.MainModel.AlbumMenuItems = new ObservableCollection<Control>(menuItems);
    }


    private void RemoveFromAlbum_OnClick(object sender, RoutedEventArgs e)
    {
        ServiceLocator.MainModel.RemoveFromAlbumCommand?.Execute(sender);
    }

    private void CreateAlbum_OnClick(object sender, RoutedEventArgs e)
    {
        ServiceLocator.MainModel.AddAlbumCommand?.Execute(null);
    }

    private void RefreshAlbum_OnClick(object sender, RoutedEventArgs e)
    {
        ReloadContextMenus();
    }

    private void AddToAlbum_OnClick(object sender, RoutedEventArgs e)
    {
        ServiceLocator.MainModel.AddToAlbumCommand?.Execute(sender);
    }
}