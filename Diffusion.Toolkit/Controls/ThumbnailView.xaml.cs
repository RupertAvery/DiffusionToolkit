using Diffusion.Database;
using Diffusion.Toolkit.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Diffusion.Toolkit.Models;
using Diffusion.Toolkit.Services;
using Diffusion.Toolkit.Pages;
using Diffusion.Common;
using Diffusion.Toolkit.Localization;
using Settings = Diffusion.Toolkit.Configuration.Settings;
using Diffusion.Database.Models;
using System.Reflection;
using FontAwesome.WPF;

namespace Diffusion.Toolkit.Controls
{
    public enum ThumbnailViewMode
    {
        Classic,
        Compact
    }

    /// <summary>
    /// Interaction logic for ThumbnailView.xaml
    /// </summary>
    public partial class ThumbnailView : UserControl
    {
        private IEnumerable<Album> _albums;

        public ThumbnailViewModel Model { get; set; }

        private DataStore _dataStore => ServiceLocator.DataStore;

        private string GetLocalizedText(string key)
        {
            return (string)JsonLocalizationProvider.Instance.GetLocalizedObject(key, null, CultureInfo.InvariantCulture);
        }

        public ThumbnailView()
        {
            InitializeComponent();

            Model = new ThumbnailViewModel();

            Model.Page = 0;
            Model.Pages = 0;
            Model.TotalFiles = 100;

            Model.PropertyChanged += ModelOnPropertyChanged;

            Model.CopyPathCommand = new RelayCommand<object>(ServiceLocator.ContextMenuService.CopyPath);
            Model.CopyPromptCommand = new RelayCommand<object>(ServiceLocator.ContextMenuService.CopyPrompt);
            Model.CopyNegativePromptCommand = new RelayCommand<object>(ServiceLocator.ContextMenuService.CopyNegative);
            Model.CopySeedCommand = new RelayCommand<object>(ServiceLocator.ContextMenuService.CopySeed);
            Model.CopyHashCommand = new RelayCommand<object>(ServiceLocator.ContextMenuService.CopyHash);
            Model.CopyParametersCommand = new RelayCommand<object>(ServiceLocator.ContextMenuService.CopyParameters);
            Model.ShowInExplorerCommand = new RelayCommand<object>(ShowInExplorer);
            Model.ExpandToFolderCommand = new RelayCommand<object>(ExpandToFolder);
            Model.DeleteCommand = new RelayCommand<object>(o => DeleteSelected());
            Model.PermanentlyDeleteCommand = new RelayCommand<object>(o => PermanentlyDeleteSelected());
            Model.FavoriteCommand = new RelayCommand<object>(o => FavoriteSelected());
            Model.NSFWCommand = new RelayCommand<object>(o => NSFWSelected());
            Model.RatingCommand = new RelayCommand<object>(o => RateSelected(int.Parse((string)o)));
            Model.RemoveEntryCommand = new RelayCommand<object>(o => RemoveEntry());
            Model.CopyCommand = new RelayCommand<object>(o => CopySelected());
            Model.MoveCommand = new RelayCommand<object>(o => MoveSelected());
            Model.RescanCommand = new AsyncCommand<object>(o => RescanSelected());

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

            _debounceRedrawThumbnails = Utility.Debounce(() => Dispatcher.Invoke(() => ReloadThumbnailsView()));

            Model.ThumbnailSpacing = ServiceLocator.Settings.ThumbnailSpacing;

            //GotFocus += ThumbnailView_GotFocus;
            Init();
        }

        //private void ThumbnailView_GotFocus(object sender, RoutedEventArgs e)
        //{
        //    FocusCurrentItem();
        //}

        private readonly Action _debounceRedrawThumbnails;

        private async Task RescanSelected()
        {

            if (await ServiceLocator.ProgressService.TryStartTask())
            {
                var imageEntries = ThumbnailListView.SelectedItems.Cast<ImageEntry>().ToList();

                if (imageEntries.Count > 50)
                {
                    await ServiceLocator.MetadataScannerService.QueueBatchAsync(
                        imageEntries.Select(s => s.Path),
                        null,
                        ServiceLocator.ProgressService.CancellationToken);
                }
                else
                {
                    await ServiceLocator.ProgressService.StartTask();

                    foreach (var imageEntry in imageEntries)
                    {
                        await ServiceLocator.MetadataScannerService.QueueAsync(imageEntry.Path, ServiceLocator.ProgressService.CancellationToken);
                    }

                }
            }

        }


        private void CopySelected()
        {
            var imageEntries = ThumbnailListView.SelectedItems.Cast<ImageEntry>().ToList();
            CopyFiles(imageEntries);
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
                case nameof(ThumbnailViewModel.ThumbnailSpacing):
                    ServiceLocator.Settings.ThumbnailSpacing = Model.ThumbnailSpacing;
                    break;
            }
        }

        //public IEnumerable<ImageEntry> SelectedImages => ThumbnailListView.SelectedItems.Cast<ImageEntry>().ToList();

        private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition(ThumbnailListView);
            var item = VisualTreeHelper.HitTest(ThumbnailListView, pt);
            if (item == null) return;
            if (item.VisualHit is FrameworkElement { DataContext: ImageEntry { IsEmpty: false } entry })
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
                        switch (e.KeyboardDevice.Modifiers)
                        {
                            case ModifierKeys.Control:
                                RemoveEntry();
                                break;
                            case ModifierKeys.None:
                                DeleteSelected();
                                break;
                            case ModifierKeys.Shift:
                                PermanentlyDeleteSelected();
                                break;
                        }

                        break;
                    }

                case Key.F when e.KeyboardDevice.Modifiers == ModifierKeys.None:
                    FavoriteSelected();
                    break;

                case Key.N when e.KeyboardDevice.Modifiers == ModifierKeys.None:
                    NSFWSelected();
                    break;

                case Key.OemTilde:
                    UnrateSelected();
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

        public void FocusItem(int index)
        {
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

            var visibleCount = 0;

            foreach (var item1 in ThumbnailListView.Items)
            {
                visibleCount += ((ImageEntry)item1).IsEmpty ? 0 : 1;
            }


            int delta = 0;
            var item = wrapPanel.Children[0] as ListViewItem;
            var columnWidth = (int)(wrapPanel.ActualWidth / item.ActualWidth);

            switch (e.Key)
            {
                case Key.End:
                    var index = visibleCount - 1;
                    SelectedImageEntry = (ImageEntry)ThumbnailListView.Items[index];
                    ThumbnailListView.SelectedItem = SelectedImageEntry;
                    wrapPanel.Children[index].Focus();
                    e.Handled = true;
                    break;

                //case Key.Left or Key.Right or Key.Up or Key.Down when ThumbnailListView.SelectedItems == null || ThumbnailListView.SelectedIndex == -1:
                //    return;
                case Key.A when (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl) && !Keyboard.IsKeyDown(Key.LeftAlt) && !Keyboard.IsKeyDown(Key.RightAlt) && !Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift)):
                    ThumbnailListView.SelectedItems.Clear();
                    foreach (var childItem in ThumbnailListView.Items.Cast<ImageEntry>().Where(d => !d.IsEmpty))
                    {
                        ThumbnailListView.SelectedItems.Add(childItem);
                    }
                    e.Handled = true;
                    break;

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
                                        var index = visibleCount - 1;
                                        SelectedImageEntry = (ImageEntry)ThumbnailListView.Items[^1];
                                        ThumbnailListView.SelectedItem = SelectedImageEntry;
                                        wrapPanel.Children[index].Focus();
                                    }, true);
                                    e.Handled = true;
                                }
                                return;
                            case 1 when currentItemIndex == visibleCount - 1 && !e.IsRepeat:
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


                        if (Model.MainModel.ThumbnailViewMode == ThumbnailViewMode.Compact)
                        {
                            //switch (delta)
                            //{
                            //    case < 0:
                            //        ServiceLocator.ThumbnailNavigationService.MovePrevious();
                            //        break;
                            //    case > 0:
                            //        ServiceLocator.ThumbnailNavigationService.MoveNext();
                            //        break;
                            //}
                            //FocusItem(ThumbnailListView.SelectedIndex);
                            //e.Handled = true;
                            //return;
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

                        if (Model.MainModel.ThumbnailViewMode == ThumbnailViewMode.Compact)
                        {
                            return;
                        }

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

                //int? effectiveRating = rating;

                //if (imageEntries.Count(i => i.Rating == rating) > imageEntries.Count / 2)
                //{
                //    effectiveRating = null;
                //}

                foreach (var entry in imageEntries)
                {
                    entry.Rating = rating;

                    if (Model.CurrentImage != null && Model.CurrentImage.Path == entry.Path)
                    {
                        Model.CurrentImage.Rating = entry.Rating;
                    }
                }

                var ids = imageEntries.Select(x => x.Id).ToList();

                _dataStore.SetRating(ids, rating);

                if (imageEntries.Count == 1)
                {
                    AdvanceOnTag();
                }
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

                _dataStore.SetRating(ids, null);

                if (imageEntries.Count == 1)
                {
                    AdvanceOnTag();
                }
            }
        }

        private void FavoriteSelected()
        {
            if (ThumbnailListView.SelectedItems != null)
            {
                var imageEntries = ThumbnailListView.SelectedItems.Cast<ImageEntry>().ToList();

                if (!imageEntries.Any()) return;

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
                _dataStore.SetFavorite(ids, favorite);

                if (imageEntries.Count == 1)
                {
                    AdvanceOnTag();
                }
            }
        }

        private void NSFWSelected()
        {
            if (ThumbnailListView.SelectedItems != null)
            {
                var imageEntries = ThumbnailListView.SelectedItems.Cast<ImageEntry>().ToList();

                if (!imageEntries.Any()) return;

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
                _dataStore.SetNSFW(ids, nsfw);

                if (imageEntries.Count == 1)
                {
                    AdvanceOnTag();
                }
            }
        }

        void AdvanceOnTag()
        {
            if (ServiceLocator.Settings.AutoAdvance)
            {
                ServiceLocator.ThumbnailNavigationService.MoveNext();
                FocusItem(ThumbnailListView.SelectedIndex);
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

                var result = await ServiceLocator.MessageService.ShowMedium(message, caption, PopupButtons.YesNo);

                if (result == PopupResult.Yes)
                {
                    var ids = imageEntries.Select(x => x.Id).ToList();

                    Model.SelectedImageEntry = null;
                    //ThumbnailListView.SelectedIndex = -1;
                    Task.Run(async () =>
                    {
                        if (await ServiceLocator.ProgressService.TryStartTask())
                        {
                            try
                            {
                                _dataStore.RemoveImages(ids);
                                ServiceLocator.SearchService.RefreshResults();
                            }
                            finally
                            {
                                ServiceLocator.ProgressService.CompleteTask();
                            }
                        }

                    });
                }
            }
        }

        private void PermanentlyDeleteSelected()
        {
            if (ThumbnailListView.SelectedItems is { Count: > 0 })
            {
                var imageEntries = ThumbnailListView.SelectedItems.Cast<ImageEntry>().ToList();

                _ = Task.Run(async () =>
                {
                    var result = PopupResult.Yes;

                    if (ServiceLocator.Settings.ConfirmDeletion)
                    {
                        var message = ServiceLocator.Settings.PermanentlyDelete
                            ? GetLocalizedText("Actions.Delete.PermanentlyDelete.Message")
                            : GetLocalizedText("Actions.Delete.Delete.Message");

                        var title = GetLocalizedText("Actions.Delete.Caption");

                        result = await ServiceLocator.MessageService.Show(message, title, PopupButtons.YesNo);
                    }

                    if (result == PopupResult.Yes)
                    {
                        if (await ServiceLocator.ProgressService.TryStartTask())
                        {
                            try
                            {
                                var files = imageEntries.Select(d => new ImagePath() { Id = d.Id, Path = d.Path }).ToList();

                                await ServiceLocator.FileService.DeleteFiles(files,
                                    ServiceLocator.ProgressService.CancellationToken);
                            }
                            finally
                            {
                                ServiceLocator.ProgressService.CompleteTask();

                                ServiceLocator.ScanningService.SetTotalFilesStatus();

                                ServiceLocator.SearchService.RefreshResults();
                            }
                        }
                    }
                });

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
                _dataStore.SetDeleted(ids, delete);

                if (imageEntries.Count == 1)
                {
                    AdvanceOnTag();
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

        private Point _start;
        private bool _dragStarted;

        private void ThumbnailListView_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition(ThumbnailListView);
            var item = VisualTreeHelper.HitTest(ThumbnailListView, pt);

            if (item != null)
            {
                var thumbnail = item.VisualHit as Thumbnail;

                if (item.VisualHit is FrameworkElement { DataContext: ImageEntry { IsEmpty: false } entry })
                {
                    //currentItemIndex = ThumbnailListView.Items.IndexOf(f.DataContext);
                    Model.MainModel.CurrentImageEntry = entry;
                    SelectedImageEntry = entry;

                    if (e.LeftButton == MouseButtonState.Pressed && (e.OriginalSource is Thumbnail or Border or Grid or ImageAwesome))
                    {
                        if (ThumbnailListView.SelectedItems.Contains(entry))
                        {
                            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0 || (Keyboard.Modifiers & ModifierKeys.Shift) != 0)
                            {

                            }
                            else
                            {
                                if (ThumbnailListView.SelectedItems.Count > 1)
                                {
                                    e.Handled = true;
                                }
                            }
                        }
                    }

                }

                this._start = e.GetPosition(null);

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

            Point pt = e.GetPosition(ThumbnailListView);
            var item = VisualTreeHelper.HitTest(ThumbnailListView, pt);

            if (e.LeftButton == MouseButtonState.Pressed && item != null && !_dragStarted &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                 Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                if (item.VisualHit is FrameworkElement { DataContext: ImageEntry { IsEmpty: false } entry })
                {
                    _dragStarted = true;

                    var source = (ListView)sender;

                    var items = ThumbnailListView.SelectedItems.Cast<ImageEntry>();

                    DataObject dataObject = new DataObject();
                    dataObject.SetData(DataFormats.FileDrop, items.Select(t => t.Path).ToArray());
                    dataObject.SetData(DragAndDrop.DragFiles, items.ToArray());

                    DragDrop.DoDragDrop(source, dataObject, DragDropEffects.Move | DragDropEffects.Copy);
                    _dragStarted = false;
                    e.Handled = true;
                }
            }
        }

        private void Unrate_OnClick(object sender, RoutedEventArgs e)
        {
            UnrateSelected();
        }

        public void ResetView(ReloadOptions? reloadOptions)
        {
            Dispatcher.Invoke(() =>
            {
                if (reloadOptions is not null && Model.Images is { Count: > 0 })
                {
                    switch (reloadOptions.CursorPosition)
                    {
                        case CursorPosition.Start:
                            ShowItem(0, reloadOptions.Focus);
                            break;
                        case CursorPosition.End:
                            ShowItem(Model.Images.Count - 1, reloadOptions.Focus);
                            break;
                        case CursorPosition.Unspecified:
                            ShowItem(0, reloadOptions.Focus);
                            break;
                    }
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
            if (Model.SelectedImageEntry == null) return;
            var p = Model.SelectedImageEntry.Path;
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

        public Action<IList<ImageEntry>> CopyFiles;

        public Action<IList<ImageEntry>> MoveFiles;


        public void SetThumbnailSize(int thumbnailSize)
        {
            Model.ThumbnailSize = thumbnailSize;
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
            Point pt = e.GetPosition(ThumbnailListView);
            var item = VisualTreeHelper.HitTest(ThumbnailListView, pt);


            if (item != null)
            {
                var thumbnail = item.VisualHit as Thumbnail;

                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    Debug.WriteLine(e.OriginalSource.GetType().Name);
                }

                if (item.VisualHit is FrameworkElement { DataContext: ImageEntry { IsEmpty: false } entry })
                {
                    if (e.LeftButton == MouseButtonState.Released && (e.OriginalSource is Thumbnail or Border or Grid or ImageAwesome))
                    {
                        if ((Keyboard.Modifiers & ModifierKeys.Control) != 0 || (Keyboard.Modifiers & ModifierKeys.Shift) != 0)
                        {

                        }
                        else
                        {
                            if (ThumbnailListView.SelectedItems.Count > 1)
                            {
                                ThumbnailListView.SelectedIndex = ThumbnailListView.Items.IndexOf(entry);
                                FocusCurrentItem();
                                ThumbnailListView.SelectedItems.Clear();
                            }
                        }
                    }
                }
            }

        }

        public void ScrollToBottom()
        {
            var scrollViewer = FindScrollViewer(ThumbnailListView);
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToBottom();
            }
        }

        public void ScrollToTop()
        {
            var scrollViewer = FindScrollViewer(ThumbnailListView);
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToTop();
            }
        }


        private ScrollViewer? FindScrollViewer(DependencyObject d)
        {
            if (d is ScrollViewer)
                return (ScrollViewer)d;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(d); i++)
            {
                var child = VisualTreeHelper.GetChild(d, i);
                var result = FindScrollViewer(child);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}
