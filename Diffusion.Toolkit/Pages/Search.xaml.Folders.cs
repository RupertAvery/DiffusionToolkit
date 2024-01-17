using Diffusion.Toolkit.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Diffusion.Toolkit.Pages
{
    public partial class Search
    {


        private void Folder_OnClick(object sender, MouseButtonEventArgs e)
        {
            var model = ((FolderViewModel)((Button)sender).DataContext);

            OpenFolder(model);
        }

        public void OpenFolder(FolderViewModel folder)
        {
            if (_currentModeSettings.CurrentFolder == folder.Path)
                return;

            var subFolders = folder.Children;

            if (subFolders == null)
            {
                subFolders = new ObservableCollection<FolderViewModel>(GetSubFolders(folder));
                folder.HasChildren = subFolders.Any();
                folder.Children = subFolders;
            }

            if (_model.MainModel.CurrentFolder != null)
            {
                _model.MainModel.CurrentFolder.IsSelected = false;
            }

            _model.MainModel.CurrentFolder = folder;

            folder.IsSelected = true;

            _model.NavigationSection.FoldersSection.CanDelete = folder.Depth > 0;
            _model.NavigationSection.FoldersSection.CanRename = folder.Depth > 0;

            _model.MainModel.ActiveView = "Folders";
            SetMode("folders");
            _model.FolderPath = folder.Path;
            _currentModeSettings.CurrentFolder = folder.Path;

            SearchImages(null);
        }

        private IEnumerable<FolderViewModel> GetSubFolders(FolderViewModel folder)
        {
            var directories = Directory.GetDirectories(folder.Path, "*", new EnumerationOptions()
            {
                IgnoreInaccessible = true
            });

            return directories.Select(path => new FolderViewModel()
            {
                Parent = folder,
                HasChildren = true,
                Visible = true,
                Depth = folder.Depth + 1,
                Name = path.EndsWith("\\") ? "Root" : Path.GetFileName(path),
                Path = path
            });
        }

        private void Folder_OnDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var model = ((FolderViewModel)((Button)sender).DataContext);

            if (model.State == FolderState.Collapsed)
            {
                var subFolders = model.Children;

                if (subFolders == null)
                {
                    subFolders = new ObservableCollection<FolderViewModel>(GetSubFolders(model));
                    model.HasChildren = subFolders.Any();
                    model.Children = subFolders;

                    if (subFolders.Any())
                    {
                        var insertPoint = _model.MainModel.Folders.IndexOf(model) + 1;

                        foreach (var subFolder in subFolders.Reverse())
                        {
                            _model.MainModel.Folders.Insert(insertPoint, subFolder);
                        }
                    }
                }
                else
                {
                    foreach (var child in model.Children.Reverse())
                    {
                        if (!_model.MainModel.Folders.Contains(child))
                        {
                            var insertPoint = _model.MainModel.Folders.IndexOf(model) + 1;

                            _model.MainModel.Folders.Insert(insertPoint, child);
                        }
                        else
                        {
                            child.Visible = true;
                            ExpandVisualTree(child);
                        }
                    }
                }



                model.State = FolderState.Expanded;
            }
            else
            {
                if (model.Children != null)
                {
                    CollapseVisualTree(model);

                    //model.Children = null;
                }

                model.State = FolderState.Collapsed;
            }
            e.Handled = true;
        }

        private void CollapseVisualTree(FolderViewModel folder)
        {
            if (folder.Children != null)
            {
                foreach (var child in folder.Children)
                {
                    child.Visible = false;
                    CollapseVisualTree(child);
                }
            }
        }

        private void ExpandVisualTree(FolderViewModel folder)
        {
            if (folder.State == FolderState.Expanded && folder.Children != null)
            {
                foreach (var child in folder.Children)
                {
                    child.Visible = true;
                    ExpandVisualTree(child);
                }
            }
        }

        public void RefreshFolder(FolderViewModel model)
        {
            var subFolders = GetSubFolders(model).ToList();

            if (model.HasChildren)
            {
                var addedFolders = subFolders.Except(model.Children);
                var removedFolders = model.Children.Except(subFolders);

                var insertPoint = _model.MainModel.Folders.IndexOf(model) + 1;

                foreach (var folder in addedFolders)
                {
                    _model.MainModel.Folders.Insert(insertPoint, folder);
                }

                foreach (var folder in removedFolders)
                {
                    _model.MainModel.Folders.Remove(folder);
                }

                model.HasChildren = subFolders.Any();
            }
            else
            {
                model.HasChildren = subFolders.Any();
                model.Children = new ObservableCollection<FolderViewModel>(subFolders);
            }

        }

    }
}
