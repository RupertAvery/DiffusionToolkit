using System.IO;
using System.Windows;
using Diffusion.Toolkit.MdStyles;

namespace Diffusion.Toolkit
{
    public class TipsModel : BaseNotify
    {
        private string _markdown;
        private Style _style;

        public string Markdown
        {
            get => _markdown;
            set => SetField(ref _markdown, value);
        }

        public Style Style
        {
            get => _style;
            set => SetField(ref _style, value);
        }
    }


    /// <summary>
    /// Interaction logic for Tips.xaml
    /// </summary>
    public partial class TipsWindow : Window
    {

        public TipsWindow()
        {
            InitializeComponent();
            var tips = new TipsModel();
            tips.Markdown = File.ReadAllText("Tips.md");
            tips.Style = CustomStyles.BetterGithub;

            //Markdown engine = new Markdown();
            //engine.DocumentStyle = CustomStyles.BetterGithub;
            //FlowDocument document = engine.Transform(markdown);
            //RichTextBox.Document = document;
            DataContext = tips;
        }


        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer", "https://github.com/RupertAvery/DiffusionToolkit/blob/master/Diffusion.Toolkit/Tips.md");
        }
    }
}
