using Diffusion.Database;
using Diffusion.Toolkit.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Diffusion.Toolkit.Models;

namespace Diffusion.Toolkit.Controls
{
    /// <summary>
    /// Interaction logic for ThumbnailView.xaml
    /// </summary>
    public partial class ThumbnailView : UserControl
    {
     
        public ThumbnailViewModel Model { get; set; }

        public DataStore DataStore { get; set; }

        public MessagePopupManager MessagePopupManager { get; set; }

        public ThumbnailView()
        {

            InitializeComponent();

            Model = new ThumbnailViewModel();

            Model.Page = 0;
            Model.Pages = 0;
            Model.TotalFiles = 100;

            Model.PropertyChanged += ModelOnPropertyChanged;


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

            if (e.Key == Key.Enter)
            {
                OpenSelected();
            }
            else if (e.Key == Key.Delete || e.Key == Key.X)
            {
                if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                {
                    RemoveEntry();
                }
                else
                {
                    DeleteSelected();
                }
            }
            else if (e.Key == Key.F)
            {
                FavoriteSelected();
            }
            else if (e.Key == Key.N)
            {
                NSFWSelected();
            }
            else if (ratings.Contains(e.Key))
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
            using Process fileopener = new Process();

            if (Model.SelectedImageEntry != null)
            {
                fileopener.StartInfo.FileName = "explorer";
                fileopener.StartInfo.Arguments = "\"" + Model.SelectedImageEntry.Path + "\"";
                fileopener.Start();
            }
        }

        private List<ImageEntry> _selItems = new List<ImageEntry>();
        private Point _start;
        private bool _restoreSelection;
        private void ThumbnailListView_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //if (ThumbnailListView.SelectedItems.Count == 0)
            //    return;
            ImageEntry? currentShape = ThumbnailListView.SelectedItem as ImageEntry;

            Point pt = e.GetPosition(ThumbnailListView);
            var item = VisualTreeHelper.HitTest(ThumbnailListView, pt);

            var thumbnail = item.VisualHit as Thumbnail;

            this._start = e.GetPosition(null);
            _selItems.Clear();
            _selItems.AddRange(ThumbnailListView.SelectedItems.Cast<ImageEntry>());

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

            if (e.LeftButton == MouseButtonState.Pressed && (e.OriginalSource is Thumbnail) &&
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
                DragDrop.DoDragDrop(source, dataObject, DragDropEffects.Copy);

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

        public void ResetView(bool focus)
        {
            if (Model.Images is { Count: > 0 })
            {
                ThumbnailListView.ScrollIntoView(Model.Images[0]);
                ThumbnailListView.SelectedItem = Model.Images[0];

                if (focus)
                {
                    if (ThumbnailListView.ItemContainerGenerator.ContainerFromIndex(0) is ListViewItem item)
                    {
                        item.Focus();
                    }

                }
            }
        }

        private void ShowInExplorer(object obj)
        {
            if (Model.CurrentImage == null) return;
            var p = Model.CurrentImage.Path;
            Process.Start("explorer.exe", $"/select,\"{p}\"");
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
                PageChangedEvent?.Invoke(this, Model.Page);
                e.Handled = true;
            }
        }

        public Action<IList<ImageEntry>> MoveFiles;
    }
}
