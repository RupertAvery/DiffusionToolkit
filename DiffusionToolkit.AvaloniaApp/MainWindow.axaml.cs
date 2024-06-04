using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Diffusion.Database;
using DiffusionToolkit.AvaloniaApp.ViewModels;

namespace DiffusionToolkit.AvaloniaApp
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _viewModel;

        public MainWindow()
        {
            _viewModel = new MainWindowViewModel();
            InitializeComponent();
            DataContext = _viewModel;
        }
        
    }
}