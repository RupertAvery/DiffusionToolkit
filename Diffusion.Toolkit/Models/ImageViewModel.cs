using Diffusion.Database.Models;
using Diffusion.Toolkit.Classes;
using Diffusion.Toolkit.Controls;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Diffusion.Common;
using Diffusion.Toolkit.Services;
using Node = Diffusion.IO.Node;

namespace Diffusion.Toolkit.Models;

public class ImageViewModel : BaseNotify
{
    private string _prompt;
    private string _negativePrompt;
    private string _otherParameters;

    private string _modelHash;

    public ImageViewModel()
    {
        CopyPathCommand = new RelayCommand<object>(ServiceLocator.ContextMenuService.CopyPath);
        CopyPromptCommand = new RelayCommand<object>(ServiceLocator.ContextMenuService.CopyPrompt);
        CopyNegativePromptCommand = new RelayCommand<object>(ServiceLocator.ContextMenuService.CopyNegative);
        //_model.CurrentImage.CopySeed = new RelayCommand<object>(CopySeed);
        //_model.CurrentImage.CopyHash = new RelayCommand<object>(CopyHash);
        CopyOthersCommand = new RelayCommand<object>(ServiceLocator.ContextMenuService.CopyOthers);
        CopyParametersCommand = new RelayCommand<object>(ServiceLocator.ContextMenuService.CopyParameters);
        //ShowInExplorerCommand = new RelayCommand<object>(ServiceLocator.ContextMenuService.ShowInExplorer);
    }

    public MainModel MainModel => ServiceLocator.MainModel;

    public int Id { get; set; }

    public bool IsMessageVisible
    {
        get;
        set => SetField(ref field, value);
    }

    public BitmapSource? Image
    {
        get;
        set => SetField(ref field, value);
    }

    public string Path
    {
        get;
        set => SetField(ref field, value);
    }

    public string? Prompt
    {
        get => _prompt;
        set => SetField(ref _prompt, value);
    }

    public string? NegativePrompt
    {
        get => _negativePrompt;
        set => SetField(ref _negativePrompt, value);
    }

    public string? OtherParameters
    {
        get => _otherParameters;
        set => SetField(ref _otherParameters, value);
    }

    public decimal CFGScale
    {
        get;
        set => SetField(ref field, value);
    }

    public int Height
    {
        get;
        set => SetField(ref field, value);
    }

    public int Width
    {
        get;
        set => SetField(ref field, value);
    }

    public string ModelName
    {
        get;
        set => SetField(ref field, value);
    }


    public string Date
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand CopyPromptCommand
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand SearchModelCommand
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand CopyPathCommand
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand ShowInExplorerCommand
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand DeleteCommand
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand FavoriteCommand
    {
        get;
        set => SetField(ref field, value);
    }


    public ICommand CopyNegativePromptCommand
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand CopyOthersCommand
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand CopyParametersCommand
    {
        get;
        set => SetField(ref field, value);
    }

    public bool Favorite
    {
        get;
        set => SetField(ref field, value);
    }

    public int? Rating
    {
        get;
        set => SetField(ref field, value);
    }

    public bool ForDeletion
    {
        get;
        set => SetField(ref field, value);
    }

    public bool NSFW
    {
        get;
        set => SetField(ref field, value);
    }

    public bool HasError
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand ShowInThumbnails
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsParametersVisible
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand ToggleParameters
    {
        get;
        set => SetField(ref field, value);
    }

    public long Seed
    {
        get;
        set => SetField(ref field, value);
    }

    public string? ModelHash
    {
        get => _modelHash;
        set => SetField(ref _modelHash, value);
    }

    public string? AestheticScore
    {
        get;
        set => SetField(ref field, value);
    }

    public IEnumerable<Album> Albums
    {
        get;
        set => SetField(ref field, value);
    }

    public string? Sampler
    {
        get;
        set => SetField(ref field, value);
    }

    public int Steps
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsLoading
    {
        get;
        set => SetField(ref field, value);
    }

    public string Message
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand OpenAlbumCommand
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand RemoveFromAlbumCommand
    {
        get;
        set => SetField(ref field, value);
    }

    public string? Workflow
    {
        get;
        set => SetField(ref field, value);
    }

    public ImageType Type
    {
        get;
        set => SetField(ref field, value);
    }

    public IReadOnlyCollection<Node> Nodes
    {
        get;
        set => SetField(ref field, value);
    }

    public IReadOnlyCollection<ImageTagView> ImageTags
    {
        get;
        set => SetField(ref field, value);
    }
    
    public string ErrorMessage
    {
        get;
        set => SetField(ref field, value);
    }
}


public class ImageTagView : BaseNotify
{
    public int Id { get; set; }
    public string Name { get; set; }

    public bool IsTicked
    {
        get; 
        set => SetField(ref field, value); 
    }
}

public class TagFilterView : BaseNotify
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int TagCount { get; set; }

    public bool IsTicked
    {
        get;
        set => SetField(ref field, value);
    }
}