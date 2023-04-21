using System.IO;
using MdXaml;
using System.Windows;
using System.Windows.Documents;
using Diffusion.Toolkit.MdStyles;

namespace Diffusion.Toolkit
{

    /// <summary>
    /// Interaction logic for Tips.xaml
    /// </summary>
    public partial class TipsWindow : Window
    {

        public TipsWindow()
        {
            InitializeComponent();
            Markdown engine = new Markdown();
            var markdown = File.ReadAllText("Tips.md");
            engine.DocumentStyle = CustomStyles.BetterGithub;
            FlowDocument document = engine.Transform(markdown);
            RichTextBox.Document = document;
        }


        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer", "https://github.com/RupertAvery/DiffusionToolkit/blob/master/Diffusion.Toolkit/Tips.md");
        }
    }
}
