using System.Linq;
using System.Windows.Media.Imaging;
using Diffusion.IO;
using static System.Net.WebRequestMethods;

namespace Diffusion.Toolkit;

public class ImageEntry : BaseNotify
{
    private BitmapSource? _thumbnail;
    private string? _fileName;


    public string? FileName
    {
        get => _fileName;
        set => SetField(ref _fileName, value);
    }

    public BitmapSource? Thumbnail
    {
        get
        {
            if (_thumbnail == null)
            {
                _ = ThumbnailLoader.Instance.Queue(FileParameters, (d) =>
                {
                    _thumbnail = d;
                    OnPropertyChanged();
                });
            }
            return _thumbnail;
        }
        //set => SetField(ref _thumbnail, value);
    }


    public FileParameters FileParameters { get; set; }





}