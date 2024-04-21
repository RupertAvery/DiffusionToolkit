﻿using Diffusion.Database;
using Diffusion.Toolkit.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Diffusion.Toolkit.Localization;
using Diffusion.Toolkit.Models;
using Microsoft.Extensions.Options;
using Image = Diffusion.Database.Image;

namespace Diffusion.Toolkit.Controls
{
    /// <summary>
    /// Interaction logic for ThumbnailView.xaml
    /// </summary>
    public partial class ThumbnailView : UserControl
    {
        private IEnumerable<Album> _albums;
        private IOptions<DataStore> _dataStoreOptions;

        public ThumbnailViewModel Model { get; set; }


        public IOptions<DataStore> DataStoreOptions
        {
            get => _dataStoreOptions;
            set
            {
                _dataStoreOptions = value;
                ReloadAlbums();
            }
        }

        public DataStore DataStore => _dataStoreOptions.Value;

        public void ReloadAlbums()
        {
            var albumMenuItem = new MenuItem()
            {
                Header = GetLocalizedText("Thumbnail.ContextMenu.AddToAlbum.NewAlbum"),
            };
            albumMenuItem.Click += CreateAlbum_OnClick;

            var refreshAlbumMenuItem = new MenuItem()
            {
                Header = GetLocalizedText("Menu.View.Refresh"),
            };
            refreshAlbumMenuItem.Click += RefreshAlbum_OnClick;

            Model.AlbumMenuItems = new ObservableCollection<Control>(new List<Control>()
            {
                albumMenuItem,
                refreshAlbumMenuItem,
                new Separator()
            });


            var albums = DataStore.GetAlbumsByName();

            foreach (var album in albums)
            {
                var menuItem = new MenuItem() { Header = album.Name, Tag = album };
                menuItem.Click += AddToAlbum_OnClick;
                Model.AlbumMenuItems.Add(menuItem);
            }
        }

        private string GetLocalizedText(string key)
        {
            return (string)JsonLocalizationProvider.Instance.GetLocalizedObject(key, null, CultureInfo.InvariantCulture);
        }


        //private void AlbumMenuItem_Click(object sender, RoutedEventArgs e)
        //{
        //    var menuItem = (MenuItem)sender;
        //    AddToAlbumCommand?.Execute(null);
        //}

        public MessagePopupManager MessagePopupManager { get; set; }

        public ThumbnailView()
        {
            InitializeComponent();

            Model = new ThumbnailViewModel();

            Model.Page = 0;
            Model.Pages = 0;
            Model.TotalFiles = 100;

            var albumMenuItem = new MenuItem()
            {
                Header = "_New Album",
            };

            albumMenuItem.Click += CreateAlbum_OnClick;

            Model.PropertyChanged += ModelOnPropertyChanged;
            Model.AlbumMenuItems = new ObservableCollection<Control>(new List<Control>()
            {
                albumMenuItem
            });

            Model.CopyPathCommand = new RelayCommand<object>(CopyPath);
            Model.CopyPromptCommand = new RelayCommand<object>(CopyPrompt);
            Model.CopyNegativePromptCommand = new RelayCommand<object>(CopyNegative);
            Model.CopySeedCommand = new RelayCommand<object>(CopySeed);
            Model.CopyHashCommand = new RelayCommand<object>(CopyHash);
            Model.CopyParametersCommand = new RelayCommand<object>(CopyParameters);
            Model.ShowInExplorerCommand = new RelayCommand<object>(ShowInExplorer);
            Model.ExpandToFolderCommand = new RelayCommand<object>(ExpandToFolder);
            Model.DeleteCommand = new RelayCommand<object>(o => DeleteSelected());
            Model.FavoriteCommand = new RelayCommand<object>(o => FavoriteSelected());
            Model.NSFWCommand = new RelayCommand<object>(o => NSFWSelected());
            Model.RatingCommand = new RelayCommand<object>(o => RateSelected(int.Parse((string)o)));
            Model.RemoveEntryCommand = new RelayCommand<object>(o => RemoveEntry());
            Model.MoveCommand = new RelayCommand<object>(o => MoveSelected());

            Model.NextPage = new RelayCommand<object>((o) => GoNextPage(null));
            Model.PrevPage = new RelayCommand<object>((o) => GoPrevPage(null));
            Model.FirstPage = new RelayCommand<object>((o) => GoFirstPage(null));
            Model.LastPage = new RelayCommand<object>((o) => GoLastPage(null));
            //_model.Refresh = new RelayCommand<object>((o) => ReloadMatches());
            //_model.ToggleParameters = new RelayCommand<object>((o) => ToggleInfo());
            //_model.CopyFiles = new RelayCommand<object>((o) => CopyFiles());

            //_model.FocusSearch = new RelayCommand<object>((o) => SearchTermTextBox.Focus());
            //_model.ShowDropDown = new RelayCommand<object>((o) => SearchTermTextBox.IsDropDownOpen = true);
            //_model.HideDropDown = new RelayCommand<object>((o) => SearchTermTextBox.IsDropDownOpen = false);
        }

        private void MoveSelected()
        {
            var imageEntries = ThumbnailListView.SelectedItems.Cast<ImageEntry>().ToList();
            MoveFiles(imageEntries);
        }

        private void ModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ThumbnailViewModel.SelectedImageEntry):
                    SelectedImageEntry = Model.SelectedImageEntry;
                    break;
                case nameof(ThumbnailViewModel.Page):
                    Page = Model.Page;
                    break;
            }
        }

        //public IEnumerable<ImageEntry> SelectedImages => ThumbnailListView.SelectedItems.Cast<ImageEntry>().ToList();

        private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition(ThumbnailListView);
            var item = VisualTreeHelper.HitTest(ThumbnailListView, pt);
            if (item == null) return;
            if (item.VisualHit is FrameworkElement { DataContext: ImageEntry })
            {
                OpenSelected();
            }

        }

        private void ThumbnailListView_OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter when e.KeyboardDevice.Modifiers == ModifierKeys.None:
                    OpenSelected();
                    break;
                
                case Key.Delete:
                case Key.X:
                    {
                        if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                        {
                            RemoveEntry();
                        }
                        else if (e.KeyboardDevice.Modifiers == ModifierKeys.None)
                        {
                            DeleteSelected();
                        }

                        break;
                    }
                
                case Key.F when e.KeyboardDevice.Modifiers == ModifierKeys.None:
                    FavoriteSelected();
                    break;

                case Key.N when e.KeyboardDevice.Modifiers == ModifierKeys.None:
                    NSFWSelected();
                    break;

                case >= Key.D0 and <= Key.D9 when e.KeyboardDevice.Modifiers == ModifierKeys.None:
                    {
                        var rating = e.Key switch
                        {
                            Key.D1 => 1,
                            Key.D2 => 2,
                            Key.D3 => 3,
                            Key.D4 => 4,
                            Key.D5 => 5,
                            Key.D6 => 6,
                            Key.D7 => 7,
                            Key.D8 => 8,
                            Key.D9 => 9,
                            Key.D0 => 10,
                        };
                        RateSelected(rating);
                    }

                    break;
                case >= Key.NumPad0 and <= Key.NumPad9 when e.KeyboardDevice.Modifiers == ModifierKeys.None:
                    {
                        var rating = e.Key switch
                        {
                            Key.NumPad1 => 1,
                            Key.NumPad2 => 2,
                            Key.NumPad3 => 3,
                            Key.NumPad4 => 4,
                            Key.NumPad5 => 5,
                            Key.NumPad6 => 6,
                            Key.NumPad7 => 7,
                            Key.NumPad8 => 8,
                            Key.NumPad9 => 9,
                            Key.NumPad0 => 10,
                        };
                        RateSelected(rating);
                    }

                    break;
            }
        }

        private static T? GetChildOfType<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        public void ShowItem(int index, bool focus = false)
        {
            var wrapPanel = GetChildOfType<WrapPanel>(this)!;
            if (wrapPanel == null) return;

            ListViewItem? item = wrapPanel.Children[index] as ListViewItem;
            if (item == null) return;

            ThumbnailListView.ScrollIntoView(item);
            item.BringIntoView();
            if (focus)
                item.Focus();
        }


        public void FocusCurrentItem()
        {
            var index = ThumbnailListView.SelectedIndex;
            if (index >= 0)
            {
                var wrapPanel = GetChildOfType<WrapPanel>(this)!;
                var item = wrapPanel.Children[index] as ListViewItem;
                ThumbnailListView.ScrollIntoView(item);
                item.BringIntoView();
                item.Focus();
            }
        }

        /// <summary>
        /// Handle wrapping around if an arrow key is pressed at the edge of the <see cref="ListView"/>.
        /// </summary>
        private void ThumbnailListView_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var wrapPanel = GetChildOfType<WrapPanel>(this)!;
            if (wrapPanel.Children.Count == 0)
            {
                return;
            }

            var currentItemIndex = -1;

            // Get the current item index based on focus
            // This may be causing issues with page navigation from within the listview
            for (int i = 0; i < wrapPanel.Children.Count; i++)
            {
                var child = wrapPanel.Children[i];
                if (child.IsFocused)
                {
                    currentItemIndex = i;
                    break;
                }
            }


            int delta = 0;
            var item = wrapPanel.Children[0] as ListViewItem;
            var columnWidth = (int)(wrapPanel.ActualWidth / item.ActualWidth);

            switch (e.Key)
            {
                //case Key.Left or Key.Right or Key.Up or Key.Down when ThumbnailListView.SelectedItems == null || ThumbnailListView.SelectedIndex == -1:
                //    return;

                case Key.Left or Key.Right:
                    {
                        delta = e.Key switch
                        {
                            Key.Left => -1,
                            Key.Right => 1,
                            _ => 0
                        };

                        switch (delta)
                        {
                            case -1 when currentItemIndex == 0 && !e.IsRepeat:
                                if (ThumbnailListView.SelectedItems.Count == 1)
                                {
                                    GoPrevPage(() =>
                                    {
                                        var index = ThumbnailListView.Items.Count - 1;
                                        SelectedImageEntry = (ImageEntry)ThumbnailListView.Items[^1];
                                        ThumbnailListView.SelectedItem = SelectedImageEntry;
                                        wrapPanel.Children[index].Focus();
                                    }, true);
                                    e.Handled = true;
                                }
                                return;
                            case 1 when currentItemIndex == ThumbnailListView.Items.Count - 1 && !e.IsRepeat:
                                if (ThumbnailListView.SelectedItems.Count == 1)
                                {
                                    GoNextPage(() =>
                                    {
                                        var index = 0;
                                        SelectedImageEntry = (ImageEntry)ThumbnailListView.Items[0];
                                        ThumbnailListView.SelectedItem = SelectedImageEntry;
                                        wrapPanel.Children[index].Focus();
                                    });
                                    e.Handled = true;
                                }
                                return;
                        }


                        if (delta != 0)
                        {
                            if (currentItemIndex + delta < 0 || currentItemIndex + delta >= wrapPanel.Children.Count)
                            {
                                e.Handled = true;
                                return;
                            }


                            if ((currentItemIndex + delta) % columnWidth == (delta == 1 ? 0 : columnWidth - 1))
                            {
                                wrapPanel.Children[currentItemIndex + delta].Focus();
                                e.Handled = true;
                            }

                        }


                        break;
                    }
                case Key.Up or Key.Down:
                    {
                        delta = e.Key switch
                        {
                            Key.Up => -columnWidth,
                            Key.Down => columnWidth,
                            _ => 0
                        };

                        if (currentItemIndex + delta < 0 || currentItemIndex + delta >= wrapPanel.Children.Count)
                        {
                            e.Handled = true;
                            return;
                        }

                        break;
                    }
            }
        }

        private void RateSelected(int rating)
        {
            if (ThumbnailListView.SelectedItems != null)
            {
                var imageEntries = ThumbnailListView.SelectedItems.Cast<ImageEntry>().ToList();

                int? effectiveRating = rating;

                if (imageEntries.Count(i => i.Rating == rating) > imageEntries.Count / 2)
                {
                    effectiveRating = null;
                }

                foreach (var entry in imageEntries)
                {
                    entry.Rating = effectiveRating;

                    if (Model.CurrentImage != null && Model.CurrentImage.Path == entry.Path)
                    {
                        Model.CurrentImage.Rating = entry.Rating;
                    }
                }

                var ids = imageEntries.Select(x => x.Id).ToList();

                DataStore.SetRating(ids, effectiveRating);
            }
        }

        private void UnrateSelected()
        {
            if (ThumbnailListView.SelectedItems != null)
            {
                var imageEntries = ThumbnailListView.SelectedItems.Cast<ImageEntry>().ToList();

                foreach (var entry in imageEntries)
                {

                    entry.Rating = null;

                    if (Model.CurrentImage != null && Model.CurrentImage.Path == entry.Path)
                    {
                        Model.CurrentImage.Rating = entry.Rating;
                    }
                }

                var ids = imageEntries.Select(x => x.Id).ToList();
                DataStore.SetRating(ids, null);
            }
        }

        private void FavoriteSelected()
        {
            if (ThumbnailListView.SelectedItems != null)
            {
                var imageEntries = ThumbnailListView.SelectedItems.Cast<ImageEntry>().ToList();

                var favorite = !imageEntries.GroupBy(e => e.Favorite).OrderByDescending(g => g.Count()).First().Key;

                foreach (var entry in imageEntries)
                {
                    entry.Favorite = favorite;
                    if (Model.CurrentImage != null && Model.CurrentImage.Path == entry.Path)
                    {
                        Model.CurrentImage.Favorite = favorite;
                    }
                }

                var ids = imageEntries.Select(x => x.Id).ToList();
                DataStore.SetFavorite(ids, favorite);
            }
        }

        private void NSFWSelected()
        {
            if (ThumbnailListView.SelectedItems != null)
            {
                var imageEntries = ThumbnailListView.SelectedItems.Cast<ImageEntry>().ToList();

                var nsfw = !imageEntries.GroupBy(e => e.NSFW).OrderByDescending(g => g.Count()).First().Key;

                foreach (var entry in imageEntries)
                {
                    entry.NSFW = nsfw;
                    if (Model.CurrentImage != null && Model.CurrentImage.Path == entry.Path)
                    {
                        Model.CurrentImage.NSFW = nsfw;
                    }
                }

                var ids = imageEntries.Select(x => x.Id).ToList();
                DataStore.SetNSFW(ids, nsfw);
            }
        }


        public Action<IEnumerable<ImageEntry>> OnRemoveEntries { get; set; }

        private async void RemoveEntry()
        {
            if (ThumbnailListView.SelectedItems != null)
            {
                var imageEntries = ThumbnailListView.SelectedItems.Cast<ImageEntry>().ToList();

                var message = GetLocalizedText("Actions.RemoveEntry.Message");
                var caption = GetLocalizedText("Actions.RemoveEntry.Caption");

                var result = await MessagePopupManager.ShowMedium(message, caption, PopupButtons.YesNo);

                if (result == PopupResult.Yes)
                {
                    var ids = imageEntries.Select(x => x.Id).ToList();

                    Model.SelectedImageEntry = null;
                    //ThumbnailListView.SelectedIndex = -1;

                    foreach (var image in imageEntries)
                    {
                        Model.Images.Remove(image);
                    }

                    DataStore.RemoveImages(ids);
                }
            }
        }


        private void DeleteSelected()
        {
            if (ThumbnailListView.SelectedItems != null && ThumbnailListView.SelectedItems.Count > 0)
            {
                var imageEntries = ThumbnailListView.SelectedItems.Cast<ImageEntry>().ToList();

                var delete = !imageEntries.GroupBy(e => e.ForDeletion).OrderByDescending(g => g.Count()).First().Key;

                foreach (var entry in imageEntries)
                {
                    entry.ForDeletion = delete;
                    if (Model.CurrentImage != null && Model.CurrentImage.Path == entry.Path)
                    {
                        Model.CurrentImage.ForDeletion = delete;
                    }
                }

                var ids = imageEntries.Select(x => x.Id).ToList();
                DataStore.SetDeleted(ids, delete);
            }
        }

        private void OpenSelected()
        {
            if (Model.SelectedImageEntry != null)
            {
                OpenCommand?.Execute(Model.SelectedImageEntry);
            }
        }

        private List<ImageEntry> _selItems = new List<ImageEntry>();
        private Point _start;
        private bool _restoreSelection;
        private bool _dragStarted;

        private void ThumbnailListView_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //if (ThumbnailListView.SelectedItems.Count == 0)
            //    return;
            ImageEntry? currentShape = ThumbnailListView.SelectedItem as ImageEntry;

            Point pt = e.GetPosition(ThumbnailListView);
            var item = VisualTreeHelper.HitTest(ThumbnailListView, pt);


            if (item != null)
            {
                var thumbnail = item.VisualHit as Thumbnail;

                if (item.VisualHit is FrameworkElement { DataContext: ImageEntry } f)
                {
                    //currentItemIndex = ThumbnailListView.Items.IndexOf(f.DataContext);

                    SelectedImageEntry = f.DataContext as ImageEntry;
                }

                if (e.LeftButton == MouseButtonState.Pressed && (e.OriginalSource is Thumbnail or Border))
                {
                    _dragStarted = true;
                }

                this._start = e.GetPosition(null);
                _selItems.Clear();
                _selItems.AddRange(ThumbnailListView.SelectedItems.Cast<ImageEntry>());


            }

            //_restoreSelection = false;

            //if (thumbnail != null && ThumbnailListView.SelectedItems.Contains(thumbnail.DataContext))
            //{
            //    _restoreSelection = true;
            //}
        }

        private void ThumbnailListView_OnMouseMove(object sender, MouseEventArgs e)
        {
            Point mpos = e.GetPosition(null);
            Vector diff = this._start - mpos;

            if (_dragStarted && e.LeftButton == MouseButtonState.Pressed && (e.OriginalSource is Thumbnail or Border) &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                 Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                if (this.ThumbnailListView.SelectedItems.Count == 0)
                {
                    return;
                }

                if (_selItems.Contains(ThumbnailListView.SelectedItems[0]))
                {
                    foreach (object selItem in _selItems)
                    {
                        if (!ThumbnailListView.SelectedItems.Contains(selItem))
                            ThumbnailListView.SelectedItems.Add(selItem);
                    }
                }
                else
                {
                    _selItems.Clear();
                    _selItems.AddRange(ThumbnailListView.SelectedItems.Cast<ImageEntry>());
                }


                var source = (ListView)sender;
                //var path = ((ImageEntry)source.DataContext).Path;

                DataObject dataObject = new DataObject();
                dataObject.SetData(DataFormats.FileDrop, _selItems.Select(t => t.Path).ToArray());
                dataObject.SetData("DTCustomDragSource", true);

                DragDrop.DoDragDrop(source, dataObject, DragDropEffects.Move | DragDropEffects.Copy);
            }

        }

        private void ThumbnailListView_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            // This doesn't get called, probably handled by some object
            // Instead, see Preview*

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point pt = e.GetPosition(ThumbnailListView);
                var item = VisualTreeHelper.HitTest(ThumbnailListView, pt);
                if (item?.VisualHit is FrameworkElement { DataContext: ImageEntry } f)
                {
                    //currentItemIndex = ThumbnailListView.Items.IndexOf(f.DataContext);
                }
            }

        }

        private void Unrate_OnClick(object sender, RoutedEventArgs e)
        {
            UnrateSelected();
        }

        public void ResetView(bool focus, bool gotoEnd = false)
        {
            Dispatcher.Invoke(() =>
            {
                if (Model.Images is { Count: > 0 })
                {
                    var index = gotoEnd ? Model.Images.Count - 1 : 0;

                    ShowItem(index, focus);
                }
            });
        }

        public Action<ImageEntry> OnExpandToFolder { get; set; }

        private void ExpandToFolder(object obj)
        {
            if (ThumbnailListView.SelectedItems.Count == 0) return;

            var imageEntry = (ImageEntry)ThumbnailListView.SelectedItems[0];

            OnExpandToFolder?.Invoke(imageEntry);
        }

        private void ShowInExplorer(object obj)
        {
            if (Model.CurrentImage == null) return;
            var p = Model.CurrentImage.Path;
            var processInfo = new ProcessStartInfo()
            {
                FileName = "explorer.exe",
                Arguments = $"/select,\"{p}\"",
                UseShellExecute = true
            };

            Process.Start(processInfo);

            //Process.Start("explorer.exe", $"/select,\"{p}\"");
        }

        private void FirstPage_OnClick(object sender, RoutedEventArgs e)
        {
            GoFirstPage(null);
        }

        private void PrevPage_OnClick(object sender, RoutedEventArgs e)
        {
            GoPrevPage(null);
        }

        private void NextPage_OnClick(object sender, RoutedEventArgs e)
        {
            GoNextPage(null);
        }

        private void LastPage_OnClick(object sender, RoutedEventArgs e)
        {
            GoLastPage(null);
        }

        private void Page_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var args = new PageChangedEventArgs()
                {
                    Page = Model.Page
                };

                PageChangedCommand?.Execute(args);
                e.Handled = true;
            }
        }

        public Action<IList<ImageEntry>> MoveFiles;


        public void SetThumbnailSize(int thumbnailSize)
        {
            Model.ThumbnailSize = thumbnailSize;
        }

        private void CreateAlbum_OnClick(object sender, RoutedEventArgs e)
        {
            AddAlbumCommand?.Execute(null);
        }

        private void RefreshAlbum_OnClick(object sender, RoutedEventArgs e)
        {
            ReloadAlbums(); 
        }

        private void AddToAlbum_OnClick(object sender, RoutedEventArgs e)
        {
            AddToAlbumCommand?.Execute(sender);
        }

        private void RemoveFromAlbum_OnClick(object sender, RoutedEventArgs e)
        {
            RemoveFromAlbumCommand?.Execute(null);
        }

        private void RenameAlbum_OnClick(object sender, RoutedEventArgs e)
        {
            RenameAlbumCommand?.Execute(null);
        }

        private void RemoveAlbum_OnClick(object sender, RoutedEventArgs e)
        {
            RemoveAlbumCommand?.Execute(null);
        }

        private void ThumbnailListView_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            _dragStarted = false;
        }

        private void ThumbnailListView_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _dragStarted = false;
        }

    }
}
