using System.IO;
using MdXaml;
using System.Windows;
using System.Windows.Documents;

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
            engine.DocumentStyle = MarkdownStyle.GithubLike;
            FlowDocument document = engine.Transform(markdown);
            RichTextBox.Document = document;
        }
    }
}
