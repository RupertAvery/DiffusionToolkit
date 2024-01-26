using Diffusion.Database;
using Diffusion.Toolkit.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Diffusion.Toolkit.Behaviors;

namespace Diffusion.Toolkit.Pages
{
    public partial class Search
    {

        private void ExpandToPath(string path)
        {
            var root = _model.MainModel.Folders.FirstOrDefault(f => f.Depth == 0 &&  path.StartsWith(f.Path, StringComparison.InvariantCultureIgnoreCase));

            var currentNode = root;

            while(currentNode != null && !currentNode.Path.Equals(path, StringComparison.InvariantCultureIgnoreCase))
            {
                if (currentNode.State == FolderState.Collapsed)
                {
                    ToggleFolder(currentNode);
                }

                if (currentNode.Children != null)
                {
                    var nextNode = currentNode.Children.FirstOrDefault(f => path.StartsWith(f.Path, StringComparison.InvariantCultureIgnoreCase));
                    if (nextNode != null)
                    {
                        currentNode = nextNode;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            if (currentNode == null) return;

            if (_model.MainModel.CurrentFolder != null)
            {
                _model.MainModel.CurrentFolder.IsSelected = false;
            }

            _model.MainModel.CurrentFolder = currentNode;

            currentNode.IsSelected = true;
        }


        //private void Expander_Click(object sender, RoutedEventArgs e)
        //{
        //    var folder = ((Button)sender).DataContext as FolderViewModel;

        //    ToggleFolder(folder);

        //    e.Handled = true;
        //}

        private void Expander_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var folder = ((FrameworkElement)sender).DataContext as FolderViewModel;

            if (folder.Status == FolderStatus.Online)
            {
                ToggleFolder(folder);
            }

            e.Handled = true;
        }

        private void Folder_OnClick(object sender, MouseButtonEventArgs e)
        {
            var model = ((FolderViewModel)((Button)sender).DataContext);

            if (model.Status == FolderStatus.Online)
            {
                OpenFolder(model);
            }
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
            var folder = ((FolderViewModel)((Button)sender).DataContext);

            if (folder.Status == FolderStatus.Online)
            {
                ToggleFolder(folder);
            }


            e.Handled = true;
        }

        private void ToggleFolder(FolderViewModel folder)
        {
            if (folder.State == FolderState.Collapsed)
            {
                var subFolders = folder.Children;

                if (subFolders == null)
                {
                    subFolders = new ObservableCollection<FolderViewModel>(GetSubFolders(folder));
                    folder.HasChildren = subFolders.Any();
                    folder.Children = subFolders;

                    if (subFolders.Any())
                    {
                        var insertPoint = _model.MainModel.Folders.IndexOf(folder) + 1;

                        foreach (var subFolder in subFolders.Reverse())
                        {
                            _model.MainModel.Folders.Insert(insertPoint, subFolder);
                        }
                    }
                }
                else
                {
                    foreach (var child in folder.Children.Reverse())
                    {
                        if (!_model.MainModel.Folders.Contains(child))
                        {
                            var insertPoint = _model.MainModel.Folders.IndexOf(folder) + 1;

                            _model.MainModel.Folders.Insert(insertPoint, child);
                        }
                        else
                        {
                            child.Visible = true;
                            ExpandVisualTree(child);
                        }
                    }
                }



                folder.State = FolderState.Expanded;
            }
            else
            {
                if (folder.Children != null)
                {
                    CollapseVisualTree(folder);

                    //model.Children = null;
                }

                folder.State = FolderState.Collapsed;
            }
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

            // TODO: prevent updating of state and MainModel.Folders if no visual update is required

            if (model.HasChildren)
            {
                var addedFolders = subFolders.Except(model.Children);
                var removedFolders = model.Children.Except(subFolders);

                var insertPoint = _model.MainModel.Folders.IndexOf(model) + 1;

                foreach (var folder in addedFolders)
                {
                    model.Children.Add(folder);
                    _model.MainModel.Folders.Insert(insertPoint, folder);
                }

                foreach (var folder in removedFolders)
                {
                    model.Children.Remove(folder);
                    _model.MainModel.Folders.Remove(folder);
                }

                model.HasChildren = subFolders.Any();
            }
            else
            {
                model.HasChildren = subFolders.Any();
                model.Children = new ObservableCollection<FolderViewModel>(subFolders);

                var insertPoint = _model.MainModel.Folders.IndexOf(model) + 1;

                foreach (var folder in subFolders)
                {
                    _model.MainModel.Folders.Insert(insertPoint, folder);
                }

            }

            if (model.HasChildren)
            {
                model.State = FolderState.Expanded;
            }
        }


    }
}
