using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Diffusion.Database;
using DiffusionToolkit.AvaloniaApp.Common;

namespace DiffusionToolkit.AvaloniaApp.Controls.Thumbnail;

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
            (o, v) => o.CurrentItem = v,
            null,
            BindingMode.TwoWay);

    public static readonly RoutedEvent<RoutedEventArgs> CurrentItemChangedEvent =
        RoutedEvent.Register<ThumbnailControl, RoutedEventArgs>(nameof(CurrentItemChanged), RoutingStrategies.Direct);

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

    protected virtual void OnCurrentItemChanged()
    {
        RoutedEventArgs args = new RoutedEventArgs(CurrentItemChangedEvent);
        RaiseEvent(args);
    }

    public ObservableCollection<ThumbnailViewModel>? SelectedItems
    {
        get => _selectedItems;
        set => _selectedItems = value;
    }

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



    public ThumbnailControl()
    {
        InitializeComponent();
        PropertyChanged += OnPropertyChanged;
        SizeChanged += ThumbnailControl_SizeChanged;
        _dataStore = ServiceLocator.DataStore;
    }


    private void ThumbnailControl_SizeChanged(object? sender, SizeChangedEventArgs e)
    {
        _columns = (int)(e.NewSize.Width / ThumbnailSize);

    }

    private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name == nameof(Thumbnails))
        {
            if (Thumbnails != null)
            {
                LoadThumbnails();
                Dispatcher.UIThread.Post(() =>
                {
                    SetCurrent(Thumbnails[0], true);
                });
            }
        }
        if (e.Property.Name == nameof(ThumbnailSize))
        {
            if (Thumbnails != null)
            {
                UnloadThumbnails();
                LoadThumbnails();
            }
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

    public void LoadThumbnails()
    {
        Task.Run(() =>
        {
            if (Thumbnails != null)
            {
                foreach (var thumbnail in Thumbnails)
                {
                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        break;
                    }
                    LoadThumbnail(thumbnail);
                }
            }
        }, _cancellationTokenSource.Token);
    }


    public void LoadThumbnail(ThumbnailViewModel thumbnail)
    {
        using var stream = File.Open(thumbnail.Path, FileMode.Open, FileAccess.Read);
        thumbnail.ThumbnailImage = Bitmap.DecodeToWidth(stream, ThumbnailSize);
    }

    private void Deselect()
    {
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
        }
    }

    private void InputElement_OnKeyDown(object? sender, KeyEventArgs e)
    {
        var validKeys = new Key[] { Key.Up, Key.Down, Key.Left, Key.Right };

        if ((e.KeyModifiers & KeyModifiers.Shift) == 0 && (e.KeyModifiers & KeyModifiers.Control) == 0 && validKeys.Contains(e.Key))
        {
            _anchorItem = null;
            Deselect();
        }

        switch (e.Key)
        {
            case Key.LeftCtrl:
            case Key.RightCtrl:
                CurrentItem.IsSelected = true;
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
                CurrentItem.IsSelected = !CurrentItem.IsSelected;
                e.Handled = true;
                break;

            case Key.Up:
                if (Thumbnails != null && CurrentItem != null)
                {
                    var index = Thumbnails.IndexOf(CurrentItem);
                    if (index - _columns < 0)
                    {
                        index = 0;
                    }
                    else
                    {
                        index -= _columns;
                    }

                    if ((e.KeyModifiers & KeyModifiers.Shift) != 0)
                    {
                        SelectRange(Thumbnails[index]);
                    }

                    SetCurrent(Thumbnails[index], true);

                }
                e.Handled = true;

                break;
            case Key.Down:
                if (Thumbnails != null && CurrentItem != null)
                {
                    var index = Thumbnails.IndexOf(CurrentItem);
                    if (index + _columns >= Thumbnails.Count)
                    {
                        index = Thumbnails.Count - 1;
                    }
                    else
                    {
                        index += _columns;
                    }

                    if ((e.KeyModifiers & KeyModifiers.Shift) != 0)
                    {
                        SelectRange(Thumbnails[index]);
                    }

                    SetCurrent(Thumbnails[index], true);
                }
                e.Handled = true;

                break;
            case Key.Left:
                if (Thumbnails != null && CurrentItem != null)
                {
                    var index = Thumbnails.IndexOf(CurrentItem);
                    if (index - 1 < 0)
                    {
                        index = 0;
                    }
                    else
                    {
                        index--;
                    }

                    if ((e.KeyModifiers & KeyModifiers.Shift) != 0)
                    {
                        SelectRange(Thumbnails[index]);
                    }

                    SetCurrent(Thumbnails[index], true);
                }
                e.Handled = true;

                break;
            case Key.Right:
                if (Thumbnails != null && CurrentItem != null)
                {
                    var index = Thumbnails.IndexOf(CurrentItem);
                    if (index + 1 >= Thumbnails.Count)
                    {
                        index = Thumbnails.Count - 1;
                    }
                    else
                    {
                        index++;
                    }

                    if ((e.KeyModifiers & KeyModifiers.Shift) != 0)
                    {
                        SelectRange(Thumbnails[index]);
                    }

                    SetCurrent(Thumbnails[index], true);
                }
                e.Handled = true;

                break;
        }
    }


    private void SelectRange(ThumbnailViewModel thumbnail)
    {
        Deselect();

        if (_anchorItem != null)
        {
            var start = Thumbnails.IndexOf(_anchorItem);
            var end = Thumbnails.IndexOf(thumbnail);

            for (var i = start; i != end; i += Math.Sign(end - start))
            {
                Thumbnails[i].IsSelected = true;
            }
        }
    }

    private void ItemsControl_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {

    }

    private void Thumbnail_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Panel { DataContext: ThumbnailViewModel thumbnail })
        {
            if ((e.KeyModifiers & KeyModifiers.Control) != 0)
            {
                if (_anchorItem == null)
                {
                    _anchorItem = thumbnail;
                }

                thumbnail.IsSelected = !thumbnail.IsSelected;
            }
            else if ((e.KeyModifiers & KeyModifiers.Shift) != 0)
            {
                SelectRange(thumbnail);
            }
            else
            {
                foreach (var item in Thumbnails)
                {
                    item.IsSelected = false;
                }

                _anchorItem = thumbnail;
            }

            SetCurrent(thumbnail, true);

        }
    }

    private void AddToSelection(ThumbnailViewModel thumbnail)
    {
        thumbnail.IsSelected = true;
    }

    private void SetCurrent(ThumbnailViewModel thumbnail, bool scrollIntoView = false)
    {

        if (CurrentItem != null) CurrentItem.IsCurrent = false;
        CurrentItem = thumbnail;
        CurrentItem.IsCurrent = true;

        if (scrollIntoView)
        {
            ThumbnailItemsControl.ScrollIntoView(thumbnail);
        }
    }
}