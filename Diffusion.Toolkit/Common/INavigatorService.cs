using System.Collections.Generic;
using System.Windows.Controls;

namespace Diffusion.Toolkit.Classes;

public interface INavigatorService
{
    void Goto(string url);
    void Back();
    void SetPages(Dictionary<string, Page> pages);
}