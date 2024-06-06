using System;
using System.IO;
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

        if (ServiceLocator.Settings.Preview != null)
        {
            switch (ServiceLocator.Settings.Preview.WindowState)
            {
                case WindowState.Normal:
                    Position = ServiceLocator.Settings.Preview.Position;
                    ClientSize = ServiceLocator.Settings.Preview.ClientSize;
                    WindowState = ServiceLocator.Settings.Preview.WindowState;
                    break;

                case WindowState.Maximized:
                case WindowState.FullScreen:
                    Position = ServiceLocator.Settings.Preview.MaxPosition;
                    ClientSize = ServiceLocator.Settings.Preview.MaxClientSize;
                    WindowState = ServiceLocator.Settings.Preview.WindowState;
                    break;
            }

            

        }
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
                    ServiceLocator.Settings.Preview.MaxPosition = Position;
                    break;
            }
            //ServiceLocator.Settings.Preview.Position = e.Point;
        }
    }

    private void PreviewWindow_Loaded(object? sender, RoutedEventArgs e)
    {
        if (ServiceLocator.Settings.Preview == null)
        {
            ServiceLocator.Settings.Preview = new WindowPosition()
            {
                WindowState = WindowState,
                Position = Position,
                ClientSize = ClientSize,
            };
        }
    }

    private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        switch (e.Property.Name)
        {
            case nameof(WindowState):
                ServiceLocator.Settings.Preview.WindowState = WindowState;
                break;
            //case nameof(Position):
            //    ServiceLocator.Settings.Preview.Position = Position;
            //    break;
            case nameof(ClientSize):
                switch (WindowState)
                {
                    case WindowState.Normal:
                        ServiceLocator.Settings.Preview.ClientSize = ClientSize;
                        break;

                    case WindowState.Maximized:
                    case WindowState.FullScreen:
                        ServiceLocator.Settings.Preview.MaxClientSize = ClientSize;
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


                Position = ServiceLocator.Settings.Preview.Position;
                ClientSize = ServiceLocator.Settings.Preview.ClientSize;

                break;
            case Key.Left:
                ServiceLocator.ThumbnailNavigationManager.MovePrevious();
                break;
            case Key.Right:
                ServiceLocator.ThumbnailNavigationManager.MoveNext();
                break;
        }
    }

    public void LoadImage(string path)
    {
        if (_viewModel.PreviewImage != null)
        {
            _viewModel.PreviewImage.Dispose();
        }

        _viewModel.Metadata = null;

        if (File.Exists(path))
        {
            _viewModel.PreviewImage = new Bitmap(path);
            _viewModel.Metadata = MetadataViewModel.FromFileParameters(Metadata.ReadFromFile(path));
            _viewModel.IsMetadataVisible = true;
        }
    }
}