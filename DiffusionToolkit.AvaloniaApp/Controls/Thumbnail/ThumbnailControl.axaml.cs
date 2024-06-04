using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using DiffusionToolkit.AvaloniaApp.ViewModels;
using ReactiveUI;

namespace DiffusionToolkit.AvaloniaApp.Controls.Thumbnail;

public class ThumbnailViewModel : ViewModelBase
{
    public object Source { get; set; }
    public string Path { get; set; }

    private Bitmap _thumbnailImage;
    private bool _isCurrent;
    private bool _isSelected;

    public Bitmap ThumbnailImage
    {
        get => _thumbnailImage;
        set => this.RaiseAndSetIfChanged(ref _thumbnailImage, value);
    }

    public bool IsCurrent
    {
        get => _isCurrent;
        set => this.RaiseAndSetIfChanged(ref _isCurrent, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => this.RaiseAndSetIfChanged(ref _isSelected, value);
    }
}

public partial class ThumbnailControl : UserControl
{
    public static readonly DirectProperty<ThumbnailControl, IList<ThumbnailViewModel>?> ThumbnailsProperty =
        AvaloniaProperty.RegisterDirect<ThumbnailControl, IList<ThumbnailViewModel>?>(
            nameof(Thumbnails),
            (o) => o.Thumbnails,
            (o, v) => o.Thumbnails = v);

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


    private IList<ThumbnailViewModel>? _thumbnails;
    private ObservableCollection<ThumbnailViewModel>? _selectedItems;
    private int _thumbnailSize = 256;

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

    public static readonly RoutedEvent<RoutedEventArgs> CurrentItemChangedEvent =
        RoutedEvent.Register<ThumbnailControl, RoutedEventArgs>(nameof(CurrentItemChanged), RoutingStrategies.Direct);

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

    private ThumbnailViewModel _anchorItem;
    private ThumbnailViewModel? _currentItem;



    public ThumbnailControl()
    {
        InitializeComponent();
        PropertyChanged += OnPropertyChanged;
        SizeChanged += ThumbnailControl_SizeChanged;
    }

    private int _columns = 0;
    //private CancellationToken _cancellationToken;
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    //public static readonly DirectProperty<ThumbnailControl, CancellationToken> CancellationTokenProperty = AvaloniaProperty.RegisterDirect<ThumbnailControl, CancellationToken>("CancellationToken", o => o.CancellationToken, (o, v) => o.CancellationToken = v);

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
                SetCurrent(Thumbnails[0], true);
                LoadThumbnails();
            }
        }
        if (e.Property.Name == nameof(ThumbnailSize))
        {
            if (Thumbnails != null)
            {
                LoadThumbnails();
            }
        }

        //if (e.Property.Name == nameof(CancellationToken))
        //{
        //    CancellationToken.
        //}
    }

    public void LoadThumbnails()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();

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

    private void InputElement_OnKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
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
                    SetCurrent(Thumbnails[index], true);
                }

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
                    SetCurrent(Thumbnails[index], true);
                }

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
                    SetCurrent(Thumbnails[index], true);
                }

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
                    SetCurrent(Thumbnails[index], true);
                }

                break;
        }
    }

    private void ItemsControl_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {

    }

    private void Thumbnail_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Panel { DataContext: ThumbnailViewModel thumbnail })
        {
            SetCurrent(thumbnail);
        }
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