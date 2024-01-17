using System.Threading.Tasks;
using System.Windows.Input;

namespace Diffusion.Toolkit.Classes;

public interface IAsyncCommand<in T> : ICommand
{
    Task ExecuteAsync(T? parameter);
    bool CanExecute(T? parameter);
}