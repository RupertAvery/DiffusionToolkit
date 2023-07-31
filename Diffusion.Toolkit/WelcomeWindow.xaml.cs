using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using Diffusion.Common;
using Diffusion.Toolkit.Classes;

namespace Diffusion.Toolkit
{
    public class WelcomeModel : BaseNotify
    {
        private bool _dontShowWelcomeOnStartup;
        private string _version;
        private ICommand _escape;

        public bool DontShowWelcomeOnStartup
        {
            get => _dontShowWelcomeOnStartup;
            set => SetField(ref _dontShowWelcomeOnStartup, value);
        }

        public string Version
        {
            get => _version;
            set => SetField(ref _version, value);
        }

        public ICommand Escape
        {
            get => _escape;
            set => SetField(ref _escape, value);
        }
    }

    /// <summary>
    /// Interaction logic for Tips.xaml
    /// </summary>
    public partial class WelcomeWindow : BorderlessWindow
    {
        private readonly Settings _settings;
        private readonly WelcomeModel _model;

        public WelcomeWindow(Settings settings)
        {
            _settings = settings;

            InitializeComponent();

            var semanticVersion = SemanticVersionHelper.GetLocalVersion();

            _model = new WelcomeModel
            {
                DontShowWelcomeOnStartup = settings.DontShowWelcomeOnStartup,
                Version = semanticVersion.ToString(),
                Escape = new RelayCommand<object>(o => Close())
            };

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
