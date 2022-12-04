using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Diffusion.Toolkit.Models;

public class ImageViewModel : BaseNotify
{
    private ICommand _copyOthersCommand;
    private ICommand _copyNegativePromptCommand;
    private ICommand _copyPathCommand;
    private ICommand _copyPromptCommand;
    private ICommand _copyParameters;
    private ICommand _openInExplorerCommand;
    private string _path;
    private string _prompt;
    private string _negativePrompt;
    private string _otherParameters;
    private BitmapSource? _image;
    private bool _favorite;
    private string _modelName;

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

    public ICommand OpenInExplorerCommand
    {
        get => _openInExplorerCommand;
        set => SetField(ref _openInExplorerCommand, value);
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


    public ICommand CopyParameters
    {
        get => _copyParameters;
        set => SetField(ref _copyParameters, value);
    }

    public bool Favorite
    {
        get => _favorite;
        set => SetField(ref _favorite, value);
    }
}