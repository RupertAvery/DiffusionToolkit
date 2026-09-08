using System.Collections.Generic;
using Diffusion.Toolkit.Models;

namespace Diffusion.Toolkit.Pages;

public class ModelsModel : BaseNotify
{
    public IEnumerable<ModelViewModel> Models
    {
        get;
        set => SetField(ref field, value);
    }

    public IEnumerable<ModelViewModel> FilteredModels
    {
        get;
        set => SetField(ref field, value);
    }

    public string Search
    {
        get;
        set => SetField(ref field, value);
    }

    public ModelViewModel SelectedModel
    {
        get;
        set => SetField(ref field, value);
    }
}