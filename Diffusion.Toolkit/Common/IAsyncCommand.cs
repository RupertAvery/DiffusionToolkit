using System.Threading.Tasks;
using System.Windows.Input;

namespace Diffusion.Toolkit.Classes;

public interface IAsyncCommand : ICommand
{
    Task ExecuteAsync();
    bool CanExecute();
}