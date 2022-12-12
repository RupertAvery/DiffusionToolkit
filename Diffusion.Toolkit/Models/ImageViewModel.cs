using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Diffusion.Toolkit.Models;

public class ImageViewModel : BaseNotify
{
    private ICommand _copyOthersCommand;
    private ICommand _copyNegativePromptCommand;
    private ICommand _copyPathCommand;
    private ICommand _copyPromptCommand;
    private ICommand _copyParametersCommand;
    private ICommand _showInExplorerCommand;
    private string _path;
    private string _prompt;
    private string _negativePrompt;
    private string _otherParameters;
    private BitmapSource? _image;
    private bool _favorite;
    private string _modelName;
    private string _date;
    private int? _rating;
    private ICommand _showInThumbnails;
    private bool _isParametersVisible;
    private ICommand _deleteCommand;
    private ICommand _favoriteCommand;
    private bool _nsfw;

    public BitmapSource? Image
    {
        get => _image;
        set => SetField(ref _image, value);
    }

    public string Path
    {
        get => _path;
        set => SetField(ref _path, value);
    }

    public string Prompt
    {
        get => _prompt;
        set => SetField(ref _prompt, value);
    }

    public string NegativePrompt
    {
        get => _negativePrompt;
        set => SetField(ref _negativePrompt, value);
    }

    public string OtherParameters
    {
        get => _otherParameters;
        set => SetField(ref _otherParameters, value);
    }


    public string ModelName
    {
        get => _modelName;
        set => SetField(ref _modelName, value);
    }


    public string Date
    {
        get => _date;
        set => SetField(ref _date, value);
    }

    public ICommand CopyPromptCommand
    {
        get => _copyPromptCommand;
        set => SetField(ref _copyPromptCommand, value);
    }

    public ICommand CopyPathCommand
    {
        get => _copyPathCommand;
        set => SetField(ref _copyPathCommand, value);
    }

    public ICommand ShowInExplorerCommand
    {
        get => _showInExplorerCommand;
        set => SetField(ref _showInExplorerCommand, value);
    }

    public ICommand DeleteCommand
    {
        get => _deleteCommand;
        set => SetField(ref _deleteCommand, value);
    }

    public ICommand FavoriteCommand
    {
        get => _favoriteCommand;
        set => SetField(ref _favoriteCommand, value);
    }


    public ICommand CopyNegativePromptCommand
    {
        get => _copyNegativePromptCommand;
        set => SetField(ref _copyNegativePromptCommand, value);
    }


    public ICommand CopyOthersCommand
    {
        get => _copyOthersCommand;
        set => SetField(ref _copyOthersCommand, value);
    }


    public ICommand CopyParametersCommand
    {
        get => _copyParametersCommand;
        set => SetField(ref _copyParametersCommand, value);
    }

    public bool Favorite
    {
        get => _favorite;
        set => SetField(ref _favorite, value);
    }

    public int? Rating
    {
        get => _rating;
        set => SetField(ref _rating, value);
    }

    public bool NSFW
    {
        get => _nsfw;
        set => SetField(ref _nsfw, value);
    }

    public ICommand ShowInThumbnails
    {
        get => _showInThumbnails;
        set => SetField(ref _showInThumbnails, value);
    }

    public bool IsParametersVisible
    {
        get => _isParametersVisible;
        set => SetField(ref _isParametersVisible, value);
    }
}