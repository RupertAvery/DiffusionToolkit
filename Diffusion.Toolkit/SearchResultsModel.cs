using System.Windows.Input;
using Diffusion.Toolkit.Models;

namespace Diffusion.Toolkit;

public class SearchResultsModel : BaseNotify
{
    public ICommand Escape { get; set; }
}