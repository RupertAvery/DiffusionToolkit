using Diffusion.Database;
using Diffusion.Toolkit.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Diffusion.Toolkit.Models;
using Microsoft.Extensions.Options;

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
                Header = "_New Album",
            };

            albumMenuItem.Click += CreateAlbum_OnClick;

            Model.AlbumMenuItems = new ObservableCollection<Control>(new List<Control>()
            {
                albumMenuItem,
                new Separator()
            });


            var albums = DataStore.GetAlbumsByLastUpdated(10);


            foreach (var album in albums)
            {
                var menuItem = new MenuItem() { Header = album.Name, Tag = album };
                menuItem.Click += AddToAlbum_OnClick;
                Model.AlbumMenuItems.Add(menuItem);
            }
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
            Model.DeleteCommand = new RelayCommand<object>(o => DeleteSelected());
            Model.FavoriteCommand = new RelayCommand<object>(o => FavoriteSelected());
            Model.NSFWCommand = new RelayCommand<object>(o => NSFWSelected());
            Model.RatingCommand = new RelayCommand<object>(o => RateSelected(int.Parse((string)o)));
            Model.RemoveEntryCommand = new RelayCommand<object>(o => RemoveEntry());
            Model.MoveCommand = new RelayCommand<object>(o => MoveSelected());

            Model.NextPage = new RelayCommand<object>((o) => GoNextPage());
            Model.PrevPage = new RelayCommand<object>((o) => GoPrevPage());
            Model.FirstPage = new RelayCommand<object>((o) => GoFirstPage());
            Model.LastPage = new RelayCommand<object>((o) => GoLastPage());
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

        public IEnumerable<ImageEntry> SelectedImages => ThumbnailListView.SelectedItems.Cast<ImageEntry>().ToList();

        private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenSelected();
        }

        private void ThumbnailListView_OnKeyDown(object sender, KeyEventArgs e)
        {
            var ratings = new[]
            {
                Key.D1,
                Key.D2,
                Key.D3,
                Key.D4,
                Key.D5,
                Key.D6,
                Key.D7,
                Key.D8,
                Key.D9,
                Key.D0,
            };

            if (e.Key == Key.Enter && e.KeyboardDevice.Modifiers == ModifierKeys.None)
            {
                OpenSelected();
            }
            else if (e.Key == Key.Delete || e.Key == Key.X)
            {
                if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                {
                    RemoveEntry();
                }
                else if (e.KeyboardDevice.Modifiers == ModifierKeys.None)
                {
                    DeleteSelected();
                }
            }
            else if (e.Key == Key.F && e.KeyboardDevice.Modifiers == ModifierKeys.None)
            {
                FavoriteSelected();
            }
            else if (e.Key == Key.N && e.KeyboardDevice.Modifiers == ModifierKeys.None)
            {
                NSFWSelected();
            }
            else if (ratings.Contains(e.Key) && e.KeyboardDevice.Modifiers == ModifierKeys.None)
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

        public void SelectItem(int index)
        {
            ThumbnailListView.SelectedIndex = index;
            var wrapPanel = GetChildOfType<WrapPanel>(this)!;
            var item = wrapPanel.Children[index] as ListViewItem;
            //var item = wrapPanel.Children[ThumbnailListView.SelectedIndex];
            ThumbnailListView.ScrollIntoView(item);
            item.BringIntoView();
        }



        /// <summary>
        /// Handle wrapping around if an arrow key is pressed at the edge of the <see cref="ListView"/>.
        /// </summary>
        private void ThumbnailListView_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Left or Key.Right)
            {

                if (ThumbnailListView.SelectedItems == null || ThumbnailListView.SelectedIndex == -1)
                {
                    return;
                }

                if (!_keyPressed)
                {
                    _startIndex = ThumbnailListView.SelectedIndex;
                    _keyPressed = true;
                }


                var delta = e.Key switch
                {
                    Key.Left => -1,
                    Key.Right => 1,
                    _ => 0
                };

                if (delta == -1 && _startIndex == 0)
                {
                    GoPrevPage(true);
                    e.Handled = true;
                    return;
                }


                if (delta == 1 && _startIndex == Model.Images.Count - 1)
                {
                    GoNextPage();
                    e.Handled = true;
                    return;
                }


                if (delta != 0)
                {
                    var wrapPanel = GetChildOfType<WrapPanel>(this)!;
                    var item = wrapPanel.Children[0] as ListViewItem;
                    var columns = (int)(wrapPanel.ActualWidth / item.ActualWidth);

                    if (ThumbnailListView.SelectedIndex + delta < 0 || ThumbnailListView.SelectedIndex + delta >= wrapPanel.Children.Count)
                    {
                        e.Handled = true;
                    }
                    else if ((ThumbnailListView.SelectedIndex + delta) % columns == (delta == 1 ? 0 : columns - 1))
                    {
                        ThumbnailListView.SelectedIndex += delta;
                        wrapPanel.Children[ThumbnailListView.SelectedIndex].Focus();
                        e.Handled = true;
                    }
                }

            }

        }

        private void RateSelected(int rating)
        {
            if (ThumbnailListView.SelectedItems != null)
            {
                var imageEntries = ThumbnailListView.SelectedItems.Cast<ImageEntry>().ToList();

                foreach (var entry in imageEntries)
                {

                    if (entry.Rating == rating)
                    {
                        entry.Rating = null;
                    }
                    else
                    {
                        entry.Rating = rating;
                    }
                    if (Model.CurrentImage != null && Model.CurrentImage.Path == entry.Path)
                    {
                        Model.CurrentImage.Rating = entry.Rating;
                    }
                }

                var ids = imageEntries.Select(x => x.Id).ToList();
                DataStore.SetRating(ids, rating);
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

                var message = "This will remove the entry from the database, but will not delete the image. You can use this to remove duplicates. \r\n\r\n You will lose any ratings or favorites set on the images! Are you sure you want to continue?";

                var result = await MessagePopupManager.ShowMedium(message, "Remove Entries", PopupButtons.YesNo);

                if (result == PopupResult.Yes)
                {
                    var ids = imageEntries.Select(x => x.Id).ToList();

                    Model.SelectedImageEntry = null;
                    ThumbnailListView.SelectedIndex = -1;

                    foreach (var image in imageEntries)
                    {
                        Model.Images.Remove(image);
                    }

                    DataStore.DeleteImages(ids);
                }
            }
        }


        private void DeleteSelected()
        {
            if (ThumbnailListView.SelectedItems != null)
            {
                if (ThumbnailListView.SelectedItems != null)
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
            //if (e.LeftButton == MouseButtonState.Pressed)
            //{
            //    if (this.ThumbnailListView.SelectedItems.Count == 0)
            //    {
            //        return;
            //    }

            //    foreach (object selItem in _selItems)
            //    {
            //        if (!ThumbnailListView.SelectedItems.Contains(selItem))
            //            ThumbnailListView.SelectedItems.Add(selItem);
            //    }

            //}

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

                    SelectItem(index);

                    //ThumbnailListView.SelectedIndex = index;
                    //var wrapPanel = GetChildOfType<WrapPanel>(this)!;
                    //var item = wrapPanel.Children[index] as ListViewItem;

                    //ThumbnailListView.ScrollIntoView(item);
                    ////ThumbnailListView.SelectedItem = item;

                    //if (focus)
                    //{
                    //    item.BringIntoView();

                    //    //if (ThumbnailListView.ItemContainerGenerator.ContainerFromIndex(index) is ListViewItem item)
                    //    //{
                    //    //    item.Focus();
                    //    //}
                    //}
                }
            });
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
            GoFirstPage();
        }

        private void PrevPage_OnClick(object sender, RoutedEventArgs e)
        {
            GoPrevPage();
        }

        private void NextPage_OnClick(object sender, RoutedEventArgs e)
        {
            GoNextPage();
        }

        private void LastPage_OnClick(object sender, RoutedEventArgs e)
        {
            GoLastPage();
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

        private int _startIndex;
        private bool _keyPressed;

        private void ThumbnailListView_OnKeyUp(object sender, KeyEventArgs e)
        {
            _keyPressed = false;
            _startIndex = -1;
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
