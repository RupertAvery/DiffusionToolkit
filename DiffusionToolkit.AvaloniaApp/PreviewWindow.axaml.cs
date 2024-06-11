using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Diffusion.Database;
using Diffusion.IO;
using DiffusionToolkit.AvaloniaApp.Common;
using DiffusionToolkit.AvaloniaApp.Controls.Metadata;
using DiffusionToolkit.AvaloniaApp.Controls.Thumbnail;
using DiffusionToolkit.AvaloniaApp.Services;
using ReactiveUI;
using SkiaSharp;

namespace DiffusionToolkit.AvaloniaApp;

public partial class PreviewWindow : Window
{
    private readonly PreviewWindowViewModel _viewModel;

    public PreviewWindow()
    {
        _viewModel = new PreviewWindowViewModel();
        InitializeComponent();
        DataContext = _viewModel;

        _viewModel.CloseCommand = ReactiveCommand.Create(() =>
        {
            Close();
        });

        PropertyChanged += OnPropertyChanged;
        PositionChanged += OnPositionChanged;

        AddHandler(InputElement.KeyDownEvent, Handler);

        Loaded += PreviewWindow_Loaded;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Grid grid = MainGrid;
            grid.RowDefinitions[0].Height = GridLength.Parse("0");
        }

        if (ServiceLocator.Settings.Preview != null)
        {
            //switch (ServiceLocator.Settings.Preview.WindowState)
            //{
            //    case WindowState.Normal:
            //        Position = ServiceLocator.Settings.Preview.Position;
            //        //ClientSize = ServiceLocator.Settings.Preview.ClientSize;
            //        //WindowState = ServiceLocator.Settings.Preview.WindowState;
            //        break;

            //    case WindowState.Maximized:
            //    case WindowState.FullScreen:
            //        //Position = ServiceLocator.Settings.Preview.MaxPosition;
            //        //ClientSize = ServiceLocator.Settings.Preview.MaxClientSize;
            //        WindowState = ServiceLocator.Settings.Preview.WindowState;
            //        break;
            //}



        }
    }

    public void FullScreen()
    {
        WindowState = WindowState.FullScreen;
        UpdateTitleBar();
    }

    private PixelPoint _lastPosition;

    private void OnPositionChanged(object? sender, PixelPointEventArgs e)
    {
        if (ServiceLocator.Settings.Preview != null)
        {
            switch (WindowState)
            {
                case WindowState.Normal:
                    ServiceLocator.Settings.Preview.Position = Position;
                    break;

                case WindowState.Maximized:
                case WindowState.FullScreen:
                    //ServiceLocator.Settings.Preview.MaxPosition = Position;
                    break;
            }

            Debug.WriteLine($"{Position.X},{Position.Y} {ClientSize.Width},{ClientSize.Height} {WindowState}");

            //ServiceLocator.Settings.Preview.Position = e.Point;
        }
    }

    private void PreviewWindow_Loaded(object? sender, RoutedEventArgs e)
    {
        if (ServiceLocator.Settings!.Preview == null)
        {
            ServiceLocator.Settings.Preview = new WindowPosition()
            {
                WindowState = WindowState,
                Position = Position,
                ClientSize = ClientSize,
            };
        }

        ImageGrid.Focus();
    }

    private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        switch (e.Property.Name)
        {
            case nameof(WindowState):
                //ServiceLocator.Settings.Preview.WindowState = WindowState;
                break;
            //case nameof(Position):
            //    ServiceLocator.Settings.Preview.Position = Position;
            //    break;
            case nameof(ClientSize):
                switch (WindowState)
                {
                    case WindowState.Normal:
                        //ServiceLocator.Settings.Preview.ClientSize = ClientSize;
                        break;

                    case WindowState.Maximized:
                    case WindowState.FullScreen:
                        //ServiceLocator.Settings.Preview.MaxClientSize = ClientSize;
                        break;
                }

                break;
        }
    }

    private WindowState _lastState;

    private void Handler(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Escape:
                Close();
                e.Handled = true;
                break;
            case Key.I:
                _viewModel.ToggleMetadata();
                break;
            //case Key.F11 when (e.KeyModifiers & KeyModifiers.Shift) != 0:
            //    WindowState = WindowState switch
            //    {
            //        WindowState.Normal => WindowState.FullScreen,
            //        WindowState.Maximized => WindowState.FullScreen,
            //        WindowState.FullScreen => WindowState.Normal,
            //        _ => WindowState
            //    };
            //    break;
            case Key.Enter when (e.KeyModifiers & KeyModifiers.Control) != 0:
            case Key.F11:
                if (WindowState != WindowState.FullScreen)
                {
                    _lastState = WindowState;
                }

                WindowState = WindowState switch
                {
                    WindowState.Normal => WindowState.FullScreen,
                    WindowState.Maximized => WindowState.FullScreen,
                    WindowState.FullScreen => _lastState,
                    _ => WindowState
                };

                UpdateTitleBar();

                //Debug.WriteLine($"{Position.X},{Position.Y} {ClientSize.Width},{ClientSize.Height} {WindowState}");
                //Position = ServiceLocator.Settings.Preview.Position;
                //ClientSize = ServiceLocator.Settings.Preview.ClientSize;

                break;
            case Key.Left:
                ServiceLocator.ThumbnailNavigationService.MovePrevious();
                break;
            case Key.Right:
                ServiceLocator.ThumbnailNavigationService.MoveNext();
                break;
        }
    }

    private void UpdateTitleBar()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Grid grid = MainGrid;
            if (WindowState == WindowState.FullScreen)
            {
                grid.RowDefinitions[0].Height = GridLength.Parse("0");
            }
            else
            {
                grid.RowDefinitions[0].Height = GridLength.Parse("30");
            }
        }
    }

    public void LoadImage(ThumbnailViewModel thumbnail)
    {
        _viewModel.SelectedEntry = thumbnail;

        var path = _viewModel.SelectedEntry.Path;

        _viewModel.PreviewImage?.Dispose();

        _viewModel.Metadata = null;

        if (File.Exists(path))
        {
            _viewModel.PreviewImage = new Bitmap(path);
            _viewModel.Metadata = MetadataViewModel.FromFileParameters(Metadata.ReadFromFile(path));
        }
    }

    private void InputElement_OnKeyDown(object? sender, KeyEventArgs e)
    {
        var isShiftPressed = (e.KeyModifiers & KeyModifiers.Shift) != 0;
        var isCtrlPressed = (e.KeyModifiers & KeyModifiers.Shift) != 0;


        if (e.Key == Key.I)
        {
            _viewModel.IsMetadataVisible = !_viewModel.IsMetadataVisible;
            e.Handled = true;
        }

        else if (e.Key == Key.Delete)
        {
            ServiceLocator.TaggingService.SetForDeletion(!_viewModel.SelectedEntry.ForDeletion);

            e.Handled = true;
        }

        else if (e.Key == Key.N)
        {
            ServiceLocator.TaggingService.SetNSFW(!_viewModel.SelectedEntry.ForDeletion);

            e.Handled = true;
        }

        else if (e.Key == Key.F)
        {
            ServiceLocator.TaggingService.SetFavorite(!_viewModel.SelectedEntry.Favorite);

            e.Handled = true;
        }

        else if (e.Key is >= Key.D0 and <= Key.D9)
        {
            int rating = e.Key - Key.D0;

            if (rating == 0) rating = 10;

            ServiceLocator.TaggingService.SetRating(rating);

            e.Handled = true;
        }
    }
}