using System;
using System.Windows;
using System.Windows.Controls;
using Diffusion.Toolkit.Models;

namespace Diffusion.Toolkit.Controls;

public class ThumbnailTemplateSelector : DataTemplateSelector
{
    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        FrameworkElement elem = container as FrameworkElement;
        if (elem == null)
        {
            return null;
        }
        if (item == null)
        {
            throw new ApplicationException();
        }

        if (item is ImageEntry imageEntry)
        {
            var key = imageEntry.EntryType switch
            {
                EntryType.File => "FileDataTemplate",
                EntryType.Folder => "FolderDataTemplate",
                _ => throw new ArgumentOutOfRangeException()
            };

            return elem.FindResource(key) as DataTemplate;
        }

        throw new ApplicationException();
    }
}