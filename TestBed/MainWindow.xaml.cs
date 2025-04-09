using System.Collections;
using System.Drawing;
using System.IO;
using System.Security.Policy;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Diffusion.Toolkit;

namespace TestBed
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TestBedModel model;

        public MainWindow()
        {
            InitializeComponent();

            var source = "D:\\Backup\\final";

            var files = Directory.GetFiles(source, "*.png");

            var width = 256;
            var height = 256;
            var size = 256;

            model = new TestBedModel()
            {
                ImageEntries = files.Select(d => new ImageEntry(0) { Path = d }).ToList()
            };

            DataContext = model;
        }

        //Select(d =>
        //{
        //    var bitmap = new BitmapImage();
        //    bitmap.BeginInit();

        //    if (width > height)
        //    {
        //        bitmap.DecodePixelWidth = size;
        //    }
        //    else
        //    {
        //        bitmap.DecodePixelHeight = size;
        //    }
        //    bitmap.StreamSource = new FileStream(d, FileMode.Open, FileAccess.Read, FileShare.Read);
        //    bitmap.EndInit();
        //    bitmap.Freeze();
        //    return bitmap;
        //}).Select(d => new ImageEntry(0) { Thumbnail = d }).ToList()

        private void ScrollBar_OnScroll(object sender, ScrollEventArgs e)
        {
            ThumbnailPanel.ScrollTo(((ScrollBar)sender).Value);
        }
    }

    public class TestBedModel : BaseNotify
    {
        private IEnumerable<ImageEntry> _imageEntries;
        private double _scrollHeight;

        public IEnumerable<ImageEntry> ImageEntries
        {
            get => _imageEntries;
            set => SetField(ref _imageEntries, value);
        }

        public double ScrollTop { get; set; }

        public double ScrollHeight
        {
            get => _scrollHeight;
            set => SetField(ref _scrollHeight, value);
        }
    }
}