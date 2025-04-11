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
using Diffusion.Common;
using Diffusion.Toolkit.Behaviors;
using Diffusion.Toolkit.Services;

namespace Diffusion.Toolkit.Pages
{
    public partial class Search
    {
        private void FolderPath_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (_model.FolderPath != null && Directory.Exists(_model.FolderPath))
                {
                    if (ServiceLocator.FolderService.RootFolders.Select(d => d.Path)
                        .Any(d => _model.FolderPath.StartsWith(d)))
                    {
                        _currentModeSettings.CurrentFolderPath = _model.FolderPath;
                        SearchImages(null);
                        return;
                    }
                }

                _model.FolderPath = _currentModeSettings.CurrentFolderPath;

                ((TextBox)sender).SelectionStart = _model.FolderPath.Length;
                ((TextBox)sender).SelectionLength = 0;

            }
            // TODO: implement autocomplete
            //else if(e.Key is >= Key.A and <= Key.Z or >= Key.D0 and <= Key.D9)
            //{
            //    var textbox = (TextBox)sender;
            //    if (textbox.Text == null) return;
            //    var subdirs = Directory.GetDirectories(_currentModeSettings.CurrentFolderPath);

            //    if (textbox.Text.EndsWith("\\")) return;

            //    var currentText = textbox.Text;

            //    var subdir = subdirs.FirstOrDefault(d => d.StartsWith(textbox.Text));

            //    if (subdir != null)
            //    {
            //        textbox.Text = subdir;

            //        textbox.SelectionStart = currentText.Length;
            //        textbox.SelectionLength = subdir.Length;
            //    }
            //}
        }

        private async Task ExpandToPath(string path)
        {
            var root = _model.MainModel.Folders.FirstOrDefault(f => f.Depth == 0 && path.StartsWith(f.Path, StringComparison.InvariantCultureIgnoreCase));

            var currentNode = root;

            while (currentNode != null && !currentNode.Path.Equals(path, StringComparison.InvariantCultureIgnoreCase))
            {
                if (currentNode.State == FolderState.Collapsed)
                {
                    await ToggleFolder(currentNode);
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


        private async void Expander_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var folder = ((Button)sender).DataContext as FolderViewModel;

                await ToggleFolder(folder);

                e.Handled = true;
            }
            catch (Exception exception)
            {
                Logger.Log(exception.Message);
            }
        }

        private async void Expander_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var folder = ((FrameworkElement)sender).DataContext as FolderViewModel;

                if (!folder.IsUnavailable)
                {
                    await ToggleFolder(folder);
                }

                e.Handled = true;
            }
            catch (Exception exception)
            {
                Logger.Log(exception.Message);
            }
        }

        private void Folder_OnClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                var folder = ((FrameworkElement)sender).DataContext as FolderViewModel;

                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    folder.IsSelected = !folder.IsSelected;
                }
                else
                {
                    OpenFolder(folder);

                    foreach (var model in ServiceLocator.MainModel.Folders.Where(d => d.IsSelected))
                    {
                        model.IsSelected = false;
                    }

                    folder.IsSelected = true;
                }

                e.Handled = true;
            }
            else if (e.ChangedButton == MouseButton.Right)
            {

            }
        }

        public void OpenFolder(FolderViewModel folder)
        {
            try
            {

                if (_currentModeSettings.CurrentFolderPath == folder.Path)
                    return;

                var subFolders = folder.Children;

                if (subFolders == null)
                {
                    if (folder.IsUnavailable) return;
                    subFolders = ServiceLocator.FolderService.GetSubFolders(folder);
                    folder.HasChildren = subFolders.Any();
                    folder.Children = subFolders;
                }

                _model.MainModel.CurrentFolder = folder;

                _model.NavigationSection.FoldersSection.CanDelete = folder.Depth > 0;
                _model.NavigationSection.FoldersSection.CanRename = folder.Depth > 0;

                _model.MainModel.ActiveView = "Folders";

                SetView("folders");

                _model.FolderPath = folder.Path;
                _currentModeSettings.CurrentFolderPath = folder.Path;

                SearchImages(null);
            }
            catch (Exception e)
            {
                _model.IsBusy = false;
            }

        }

        private async Task Folder_OnDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var folder = ((FolderViewModel)((Button)sender).DataContext);

            if (!folder.IsUnavailable)
            {
                await ToggleFolder(folder);
            }

            e.Handled = true;
        }

        private async Task ToggleFolder(FolderViewModel folder)
        {
            if (folder.State == FolderState.Collapsed)
            {
                var subFolders = folder.Children;

                if (subFolders == null)
                {
                    await Task.Run(() =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            subFolders = ServiceLocator.FolderService.GetSubFolders(folder);
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
                        });
                    });

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

        public void RefreshFolder(FolderViewModel targetFolder)
        {
            var subFolders = ServiceLocator.FolderService.GetSubFolders(targetFolder).ToList();

            // TODO: prevent updating of state and MainModel.Folders if no visual update is required

            if (targetFolder.HasChildren)
            {
                var addedFolders = subFolders.Except(targetFolder.Children);
                var removedFolders = targetFolder.Children.Except(subFolders);

                var insertPoint = _model.MainModel.Folders.IndexOf(targetFolder) + 1;

                foreach (var folder in addedFolders)
                {
                    targetFolder.Children.Add(folder);
                    _model.MainModel.Folders.Insert(insertPoint, folder);
                }

                foreach (var folder in removedFolders)
                {
                    targetFolder.Children.Remove(folder);
                    _model.MainModel.Folders.Remove(folder);
                }

                targetFolder.HasChildren = subFolders.Any();
            }
            else
            {
                targetFolder.HasChildren = subFolders.Any();
                targetFolder.Children = new ObservableCollection<FolderViewModel>(subFolders);

                var insertPoint = _model.MainModel.Folders.IndexOf(targetFolder) + 1;

                foreach (var folder in subFolders)
                {
                    _model.MainModel.Folders.Insert(insertPoint, folder);
                }

            }

            if (targetFolder.HasChildren)
            {
                targetFolder.State = FolderState.Expanded;
            }
        }


        private void NotScanned_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_model.MainModel.CurrentFolder != null)
            {
                _ = ServiceLocator.ScanningService.ScanFolder(_model.MainModel.CurrentFolder);
            }
        }
    }
}
