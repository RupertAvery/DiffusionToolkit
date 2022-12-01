using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Diffusion.Toolkit;

public class VirtualImage : Image
{
    private static void OnPathChanged(DependencyObject defectImageControl, DependencyPropertyChangedEventArgs eventArgs)
    {
        var control = (VirtualImage)defectImageControl;
        control.Path = (string)eventArgs.NewValue;

        //using var fs = new FileStream(control._path, FileMode.Open, FileAccess.Read,
        //    FileShare.Read);

        ////memoryStream.Position = 0;
        //var bitmap = BitmapFrame.Create(fs, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
        //control.Source = bitmap;
    }


    //public MemoryStream memoryStream = null;
    public static readonly DependencyProperty PathProperty =
        DependencyProperty.Register(
            name: "Path",
            propertyType: typeof(string),
            ownerType: typeof(VirtualImage),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: null,
                FrameworkPropertyMetadataOptions.AffectsRender,
                new PropertyChangedCallback(OnPathChanged))
        );
    
    public string? Path
    {
        get => (string)GetValue(PathProperty);
        set => SetValue(PathProperty, value);
    }

    public VirtualImage() : base()
    {
        IsVisibleChanged += new DependencyPropertyChangedEventHandler(Handler);
    }

    void Handler(object sender, DependencyPropertyChangedEventArgs e)
    {
        var c = (VirtualImage)sender;

        if (IsVisible)
        {
            if(c.Path == null) return;

            using var fs = new FileStream(c.Path, FileMode.Open, FileAccess.Read,
                FileShare.Read);

            //memoryStream.Position = 0;
            var bitmap = BitmapFrame.Create(fs, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            Source = bitmap;
        }
        else
        {
            Source = null;
        }
    }
}