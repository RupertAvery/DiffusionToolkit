using System.Windows.Media.Imaging;
using Diffusion.IO;
using Diffusion.Toolkit.Thumbnails;

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
                var job = new ThumbnailJob()
                {
                    Path = FileParameters.Path, 
                    Height = FileParameters.Height,
                    Width = FileParameters.Width
                };

                _ = ThumbnailLoader.Instance.QueueAsync(job, (d) =>
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