using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Diffusion.Toolkit
{
    public class WelcomeModel : BaseNotify
    {
        private bool _dontShowWelcomeOnStartup;

        public bool DontShowWelcomeOnStartup
        {
            get => _dontShowWelcomeOnStartup;
            set => SetField(ref _dontShowWelcomeOnStartup, value);
        }
    }

    /// <summary>
    /// Interaction logic for Tips.xaml
    /// </summary>
    public partial class WelcomeWindow : Window
    {
        private readonly Settings _settings;
        private readonly WelcomeModel _model;

        public WelcomeWindow(Settings settings)
        {
            _settings = settings;

            InitializeComponent();

            _model = new WelcomeModel();
            _model.DontShowWelcomeOnStartup = settings.DontShowWelcomeOnStartup;

            Closing += (sender, args) =>
            {
                settings.SetPristine();
                settings.DontShowWelcomeOnStartup = _model.DontShowWelcomeOnStartup;
            };

            DataContext = _model;
        }

        private void HyperLink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri)
            {
                UseShellExecute = true,
            });
            e.Handled = true;
        }
    }
}
