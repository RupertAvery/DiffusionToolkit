
using System.Windows.Controls;

namespace Diffusion.Toolkit.Classes;

public interface INavigatorService
{
    void Goto(string url);
    void Back();
    void RegisterRoute(string path, Page page);

}