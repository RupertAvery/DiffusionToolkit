using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Diffusion.Database;
using DiffusionToolkit.AvaloniaApp.Common;

namespace DiffusionToolkit.AvaloniaApp.Controls.Thumbnail;

public enum NavigationState
{
    StartOfPage,
    EndOfPage,
}

public class NavigationEventArgs : RoutedEventArgs
{
    public NavigationEventArgs()
    {
        RoutedEvent = ThumbnailControl.NavigationChangedEvent;
    }
    public NavigationState NavigationState { get; set; }
}

public partial class ThumbnailControl : UserControl
{
    private int _thumbnailSize = 256;
    private int _columns = 0;
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private DataStore _dataStore;

    private IList<ThumbnailViewModel>? _thumbnails;
    private ObservableCollection<ThumbnailViewModel>? _selectedItems;

    public static readonly DirectProperty<ThumbnailControl, IList<ThumbnailViewModel>?> ThumbnailsProperty =
        AvaloniaProperty.RegisterDirect<ThumbnailControl, IList<ThumbnailViewModel>?>(
            nameof(Thumbnails),
            (o) => o.Thumbnails,
            (o, v) =>
            {
                o.UnloadThumbnails();
                o.Thumbnails = v;
            });

    public static readonly DirectProperty<ThumbnailControl, int> ThumbnailSizeProperty =
        AvaloniaProperty.RegisterDirect<ThumbnailControl, int>(
            nameof(ThumbnailSize),
            (o) => o.ThumbnailSize,
            (o, v) => o.ThumbnailSize = v);

    public static readonly DirectProperty<ThumbnailControl, ThumbnailViewModel?> CurrentItemProperty =
        AvaloniaProperty.RegisterDirect<ThumbnailControl, ThumbnailViewModel?>(nameof(CurrentItem),
            o => o.CurrentItem,
            (o, v) =>
            {
                if (o.CurrentItem != null)
                {
                    o.CurrentItem.IsCurrent = false;
                }
                o.CurrentItem = v;
                if (o.CurrentItem != null)
                {
                    o.CurrentItem.IsCurrent = true;
                }
            },
            null,
            BindingMode.TwoWay);

    public static readonly DirectProperty<ThumbnailControl, ObservableCollection<ThumbnailViewModel>?> SelectedItemsProperty =
        AvaloniaProperty.RegisterDirect<ThumbnailControl, ObservableCollection<ThumbnailViewModel>?>(nameof(CurrentItem),
            o => o.SelectedItems,
            (o, v) =>
            {
                o.SelectedItems = v;
            },
            null,
            BindingMode.TwoWay);


    public static readonly RoutedEvent<RoutedEventArgs> CurrentItemChangedEvent =
        RoutedEvent.Register<ThumbnailControl, RoutedEventArgs>(nameof(CurrentItemChanged), RoutingStrategies.Direct);

    public static readonly RoutedEvent<NavigationEventArgs> NavigationChangedEvent =
        RoutedEvent.Register<ThumbnailControl, NavigationEventArgs>(nameof(NavigationChanged), RoutingStrategies.Direct);


    public int ThumbnailSize
    {
        get => _thumbnailSize;
        set => this.SetAndRaise(ThumbnailSizeProperty, ref _thumbnailSize, value);
    }

    public IList<ThumbnailViewModel>? Thumbnails
    {
        get => _thumbnails;
        set => this.SetAndRaise(ThumbnailsProperty, ref _thumbnails, value);
    }

    public event EventHandler<RoutedEventArgs> CurrentItemChanged
    {
        add => AddHandler(CurrentItemChangedEvent, value);
        remove => RemoveHandler(CurrentItemChangedEvent, value);
    }

    public event EventHandler<NavigationEventArgs> NavigationChanged
    {
        add => AddHandler(NavigationChangedEvent, value);
        remove => RemoveHandler(NavigationChangedEvent, value);
    }

    protected virtual void OnCurrentItemChanged()
    {
        RoutedEventArgs args = new RoutedEventArgs(CurrentItemChangedEvent);
        RaiseEvent(args);
    }

    public ObservableCollection<ThumbnailViewModel>? SelectedItems
    {
        get => _selectedItems;
        set
        {
            SetAndRaise(SelectedItemsProperty, ref _selectedItems, value);
            //if (value != null)
            //{
            //    value.CollectionChanged += ValueOnCollectionChanged;
            //}
        }
    }

    //private void ValueOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    //{
    //    Debug.WriteLine(e.Action);
    //    Debug.WriteLine(e.OldItems);
    //    Debug.WriteLine(e.NewItems);
    //}

    public ThumbnailViewModel? CurrentItem
    {
        get => _currentItem;
        set => SetAndRaise(CurrentItemProperty, ref _currentItem, value);
    }

    //public CancellationToken CancellationToken
    //{
    //    get => _cancellationToken;
    //    set => SetAndRaise(CancellationTokenProperty, ref _cancellationToken, value);
    //}

    private ThumbnailViewModel? _anchorItem;
    private ThumbnailViewModel? _currentItem;
    private ThumbnailNavigationManager _thumbnailNavigationManager;
    public ThumbnailControl()
    {
        InitializeComponent();

        _thumbnailNavigationManager = ServiceLocator.ThumbnailNavigationManager;
        _thumbnailNavigationManager.Next += OnNext;
        _thumbnailNavigationManager.Previous += OnPrevious;

        Loaded += OnLoaded;

        SelectedItems = new ObservableCollection<ThumbnailViewModel>();
        PropertyChanged += OnPropertyChanged;
        SizeChanged += ThumbnailControl_SizeChanged;
        _dataStore = ServiceLocator.DataStore;
    }

    private void OnPrevious(object? sender, EventArgs e)
    {
        InputElement_OnKeyDown(this, new KeyEventArgs() { Key = Key.Left });
    }

    private void OnNext(object? sender, EventArgs e)
    {
        InputElement_OnKeyDown(this, new KeyEventArgs() { Key = Key.Right });
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        var scrollViewer = ItemsScrollViewer;
        scrollViewer.ScrollChanged += ScrollViewerOnScrollChanged;
    }

    private void ScrollViewerOnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        RedrawThumbnails();
    }

    private void RedrawThumbnails()
    {
        var scrollViewer = ItemsScrollViewer;
        ItemsControl itemsControl = ThumbnailItemsControl;

        // Get the bounds of the ScrollViewer's viewport
        var viewportBounds = new Rect(scrollViewer.Offset.X, scrollViewer.Offset.Y, scrollViewer.Viewport.Width, scrollViewer.Viewport.Height);

        var wrapPanel = FindVisualChild<WrapPanel>(itemsControl);

        var items = wrapPanel.GetVisualChildren();
        foreach (var item in items)
        {
            var intersects = viewportBounds.Intersects(item.Bounds);
            if (intersects)
            {
                if (item.DataContext is ThumbnailViewModel { IsLoaded: false } thumbnail)
                {
                    Task.Run(() => LoadThumbnail(thumbnail));
                }
            }
        }
    }

    private T? FindVisualChild<T>(Visual control)
    {
        if (control == null)
            return default(T);

        if (control is T visualChild)
            return visualChild;

        foreach (var child in control.GetVisualChildren())
        {
            var result = FindVisualChild<T>(child);
            if (result != null)
                return result;
        }

        return default(T);
    }


    private void ThumbnailControl_SizeChanged(object? sender, SizeChangedEventArgs e)
    {
        _columns = (int)(e.NewSize.Width / ThumbnailSize);

    }

    private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        switch (e.Property.Name)
        {
            case nameof(Thumbnails):
                {
                    if (Thumbnails != null)
                    {
                        Deselect();
                        Dispatcher.UIThread.Post(() =>
                        {
                            RedrawThumbnails();
                        });
                    }

                    break;
                }
            case nameof(ThumbnailSize):
                {
                    if (Thumbnails != null)
                    {
                        UnloadThumbnails();
                        RedrawThumbnails();
                    }

                    break;
                }
            case nameof(CurrentItem):
                if (CurrentItem != null)
                {
                    ThumbnailItemsControl.ScrollIntoView(CurrentItem);
                }
                break;
        }
    }

    public void UnloadThumbnails()
    {
        _cancellationTokenSource.Cancel();

        Task.Run(() =>
        {
            if (Thumbnails != null)
            {
                foreach (var thumbnail in Thumbnails.ToList())
                {
                    thumbnail.Dispose();
                }
            }
        });

        _cancellationTokenSource = new CancellationTokenSource();
    }

    //public void LoadThumbnails()
    //{
    //    Task.Run(() =>
    //    {
    //        if (Thumbnails != null)
    //        {
    //            foreach (var thumbnail in Thumbnails)
    //            {
    //                if (_cancellationTokenSource.Token.IsCancellationRequested)
    //                {
    //                    break;
    //                }
    //                LoadThumbnail(thumbnail);
    //            }
    //        }
    //    }, _cancellationTokenSource.Token);
    //}


    public void LoadThumbnail(ThumbnailViewModel thumbnail)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (File.Exists(thumbnail.Path))
            {
                using var stream = File.Open(thumbnail.Path, FileMode.Open, FileAccess.Read);
                thumbnail.ThumbnailImage = Bitmap.DecodeToWidth(stream, ThumbnailSize);
                thumbnail.IsLoaded = true;
            }
        });
    }

    private void Deselect()
    {
        SelectedItems?.Clear();
        foreach (var item in Thumbnails)
        {
            item.IsSelected = false;
        }
    }

    private void SelectAll()
    {
        foreach (var item in Thumbnails)
        {
            item.IsSelected = true;
            SelectedItems?.Add(item);
        }
    }

    private void InputElement_OnKeyDown(object? sender, KeyEventArgs e)
    {
        var validKeys = new Key[] { Key.Up, Key.Down, Key.Left, Key.Right, Key.PageUp, Key.PageDown };

        if ((e.KeyModifiers & KeyModifiers.Shift) == 0 && (e.KeyModifiers & KeyModifiers.Control) == 0 && validKeys.Contains(e.Key))
        {
            _anchorItem = null;
            Deselect();
        }

        switch (e.Key)
        {
            case Key.LeftCtrl:
            case Key.RightCtrl:
                if (CurrentItem is { })
                {
                    CurrentItem.IsSelected = true;
                    UpdateSelection(CurrentItem);
                }
                break;


            case Key.LeftShift:
            case Key.RightShift:
                if (_anchorItem == null)
                {
                    _anchorItem = CurrentItem;
                }

                break;

            case Key.A:
                if ((e.KeyModifiers & KeyModifiers.Control) != 0)
                {
                    SelectAll();
                }

                e.Handled = true;
                break;

            case Key.Space:
                if (CurrentItem is { })
                {
                    CurrentItem.IsSelected = !CurrentItem.IsSelected;
                    UpdateSelection(CurrentItem);
                }
                e.Handled = true;
                break;

            case Key.Left:
            case Key.Right:
            case Key.Up:
            case Key.Down:
            case Key.PageUp:
            case Key.PageDown:
            case Key.Home:
            case Key.End:
                if (Thumbnails != null && CurrentItem != null)
                {
                    var index = Thumbnails.IndexOf(CurrentItem);

                    switch (e.Key)
                    {
                        case Key.Left:
                            index--;

                            if (index == -1)
                            {
                                RaiseEvent(new NavigationEventArgs() { NavigationState = NavigationState.StartOfPage });
                            }

                            break;
                        case Key.Right:
                            index++;

                            if (index == Thumbnails.Count)
                            {
                                RaiseEvent(new NavigationEventArgs() { NavigationState = NavigationState.EndOfPage });
                            }

                            break;
                        case Key.Up:
                            index -= _columns;
                            break;
                        case Key.Down:
                            index += _columns;
                            break;
                        case Key.PageUp:
                            index -= _columns * 5;
                            break;
                        case Key.PageDown:
                            index += _columns * 5;
                            break;
                        case Key.Home:
                            index = 0;
                            break;
                        case Key.End:
                            index = Thumbnails.Count - 1;
                            break;

                    }


                    index = Math.Clamp(index, 0, Thumbnails.Count - 1);

                    if ((e.KeyModifiers & KeyModifiers.Shift) != 0)
                    {
                        SelectRange(Thumbnails[index]);
                    }
                    else
                    {
                        Thumbnails[index].IsSelected = true;
                        UpdateSelection(Thumbnails[index]);
                    }

                    SetCurrent(Thumbnails[index], true);

                    e.Handled = true;
                }

                break;
        }
    }

    private void UpdateSelection(ThumbnailViewModel item)
    {
        if (SelectedItems == null) return;

        if (item.IsSelected)
        {
            if (!SelectedItems.Contains(item))
            {
                SelectedItems.Add(item);
            }
        }
        else
        {
            SelectedItems.Remove(item);
        }
    }


    private void SelectRange(ThumbnailViewModel thumbnail)
    {
        Deselect();

        if (_anchorItem != null)
        {
            var start = Thumbnails.IndexOf(_anchorItem);
            var end = Thumbnails.IndexOf(thumbnail);

            if (start > end)
            {
                (start, end) = (end, start);
            }

            for (var i = start; i <= end; i++)
            {
                Thumbnails[i].IsSelected = true;
                SelectedItems?.Add(Thumbnails[i]);
            }
        }
    }

    private void ItemsControl_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {

    }

    private Point lastPoint;

    private void Thumbnail_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Panel { DataContext: ThumbnailViewModel thumbnail } panel)
        {
            if ((e.KeyModifiers & KeyModifiers.Control) != 0)
            {
                if (_anchorItem == null)
                {
                    _anchorItem = thumbnail;
                }

                thumbnail.IsSelected = !thumbnail.IsSelected;
                UpdateSelection(thumbnail);
            }
            else if ((e.KeyModifiers & KeyModifiers.Shift) != 0)
            {
                SelectRange(thumbnail);
            }
            else
            {
                if (!thumbnail.IsSelected)
                {
                    Deselect();
                }

                thumbnail.IsSelected = true;
                if (SelectedItems != null && !SelectedItems.Contains(thumbnail))
                {
                    SelectedItems.Add(thumbnail);
                }
            }

            SetCurrent(thumbnail, true);

            _isMouseDown = true;
            lastPoint = e.GetPosition(panel);
        }
    }

    private bool _isMouseDown = false;
    private bool _isDragging = false;

    private void SetCurrent(ThumbnailViewModel thumbnail, bool scrollIntoView = false)
    {
        if (CurrentItem != null) CurrentItem.IsCurrent = false;

        CurrentItem = thumbnail;
        CurrentItem.IsCurrent = true;


        //if (scrollIntoView)
        //{
        //    ThumbnailItemsControl.ScrollIntoView(thumbnail);
        //}
    }

    private async void InputElement_OnPointerMoved(object? sender, PointerEventArgs e)
    {

        if (_isMouseDown && !_isDragging)
        {

            if (sender is Panel panel)
            {
                var thisPoint = e.GetPosition(panel);

                var distance = thisPoint - lastPoint;

                if (Math.Abs(distance.X) > 5 || Math.Abs(distance.Y) > 5)
                {
                    _isDragging = true;

                    var selectedItems = Thumbnails.Where(t => t.IsSelected || t.IsCurrent).Select(t => t.Path.Replace("\\", "/")).ToArray();

                    DataObject dataObject = new DataObject();
                    //dataObject.Set("FileDrop", selectedItems);
                    dataObject.Set(DataFormats.Files, selectedItems);
                    dataObject.Set("DTCustomDragSource", true);

                    await DragDrop.DoDragDrop(e, dataObject, DragDropEffects.Move | DragDropEffects.Copy);
                }
            }


        }
    }

    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_isDragging && ((e.KeyModifiers & KeyModifiers.Control) == 0 && (e.KeyModifiers & KeyModifiers.Shift) == 0))
        {
            _anchorItem = CurrentItem;
        }

        _isMouseDown = false;
        _isDragging = false;
    }
}