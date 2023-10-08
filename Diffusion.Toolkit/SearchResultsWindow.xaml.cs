using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Diffusion.Toolkit.Classes;

namespace Diffusion.Toolkit
{
    /// <summary>
    /// Interaction logic for SearchResultsWindowxaml.xaml
    /// </summary>
    public partial class SearchResultsWindow : BorderlessWindow
    {
        private SearchResultsModel _model;

        public SearchResultsWindow()
        {
            InitializeComponent();
            _model = new SearchResultsModel();
            DataContext = _model;

            _model.Escape = new RelayCommand<object>(Escape);
        }

        private void Escape(object obj)
        {
            Close();
        }
    }
}
