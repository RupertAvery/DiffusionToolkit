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
            InitializeComponent();
        }


        public MainWindow(DataStore dataStore)
        {
            _viewModel = new MainWindowViewModel(dataStore);
            InitializeComponent();
            DataContext = _viewModel;
        }

        private void InputElement_OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _viewModel.Search();
            }
        }
    }
}