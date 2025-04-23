using Diffusion.Database;
using Diffusion.Toolkit.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using Diffusion.Database.Models;
using Diffusion.Civitai.Models;
using Microsoft.WindowsAPICodePack.Dialogs;

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
                        SearchImages(null);
                        return;
                    }
                }

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
            var root = ServiceLocator.MainModel.Folders.FirstOrDefault(f =>
                f.Depth == 0 && path.StartsWith(f.Path, StringComparison.InvariantCultureIgnoreCase));

            var currentNode = root;

            while (currentNode != null && !currentNode.Path.Equals(path, StringComparison.InvariantCultureIgnoreCase))
            {
                if (currentNode.State == FolderState.Collapsed)
                {
                    await ToggleFolder(currentNode);
                }

                var children = ServiceLocator.FolderService.GetVisualChildren(currentNode);

                var enumerator = children.GetEnumerator();

                var found = false;
                while (enumerator.MoveNext())
                {
                    currentNode = enumerator.Current;
                    if (path.StartsWith(currentNode.Path, StringComparison.InvariantCultureIgnoreCase))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    currentNode = null;
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

        private Point _start;
        private bool _dragStarted;

        private void Folder_OnClick(object sender, MouseButtonEventArgs e)
        {
            this._start = e.GetPosition(null);

            if (sender is Grid grid)
            {
                grid.Focus();
            }

            var folder = ((FrameworkElement)sender).DataContext as FolderViewModel;


            var selectedFolders = ServiceLocator.FolderService.SelectedFolders.ToList();

            var isInSelection = selectedFolders.Contains(folder);
            var isSingleSelection = selectedFolders.Count == 1;
            var isRoot = folder.Depth == 0;
            var isAvailable = !folder.IsUnavailable;
            var isScanned = folder.IsScanned;
            var isExcluded = folder.IsExcluded;
            var isArchived = folder.IsArchived;

            _model.NavigationSection.FoldersSection.CanCreateFolder = isSingleSelection && isAvailable;
            _model.NavigationSection.FoldersSection.CanDelete = !isRoot && isAvailable;
            _model.NavigationSection.FoldersSection.CanRename = !isRoot && isSingleSelection && isAvailable;
            _model.NavigationSection.FoldersSection.CanRemove = !isRoot && isSingleSelection;
            _model.NavigationSection.FoldersSection.CanShowInExplorer = isSingleSelection && isAvailable;
            _model.NavigationSection.FoldersSection.CanArchive = isScanned && isAvailable && !isExcluded && !isArchived;
            _model.NavigationSection.FoldersSection.CanUnarchive =
                isScanned && isAvailable && !isExcluded && isArchived;

            _model.NavigationSection.FoldersSection.CanArchiveTree = isSingleSelection && isScanned && isAvailable && !isExcluded;
            _model.NavigationSection.FoldersSection.CanUnarchiveTree = isSingleSelection && isScanned && isAvailable && !isExcluded;

            _model.NavigationSection.FoldersSection.CanExclude = !isRoot && isAvailable && !isExcluded;
            _model.NavigationSection.FoldersSection.CanUnexclude = !isRoot && isAvailable && isExcluded;

            _model.NavigationSection.FoldersSection.CanExcludeTree = isSingleSelection && !isRoot && isAvailable;
            _model.NavigationSection.FoldersSection.CanUnexcludeTree = isSingleSelection && !isRoot && isAvailable;


            if (e.LeftButton == MouseButtonState.Pressed && (e.OriginalSource is Grid or TextBlock))
            {
                _dragStarted = true;
            }

            if (e.ChangedButton == MouseButton.Left)
            {
                if (e.ClickCount == 2)
                {
                    if (!folder.IsUnavailable)
                    {
                        _ = ToggleFolder(folder);
                    }

                    e.Handled = true;
                    return;
                }

                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    folder.IsSelected = !folder.IsSelected;
                }
                else
                {
                    if (!isInSelection)
                    {
                        ServiceLocator.FolderService.ClearSelection();
                    }

                    if (!folder.IsUnavailable)
                    {
                        OpenFolder(folder);
                    }

                    folder.IsSelected = true;
                }

                e.Handled = true;
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                if (selectedFolders.Contains(folder))
                {
                    return;
                }

                ServiceLocator.FolderService.ClearSelection();

                folder.IsSelected = true;
            }

        }

        public void ClearResults()
        {
            foreach (var image in _model.Images)
            {
                image.Thumbnail = null;
                image.IsEmpty = true;
            }

            _model.Page = 0;
            _model.Pages = 0;
            _model.Results = GetLocalizedText("Search.NoResults");

            ThumbnailListView.ReloadThumbnailsView();
            ThumbnailListView.ClearSelection();
        }

        public void OpenFolder(FolderViewModel? folder)
        {
            try
            {
                // Go Home
                if (folder == null)
                {
                    //_model.GoHome.Execute(null);
                    _model.FolderPath = RootFolders;
                    SearchImages(null);
                    return;
                }

                if (folder.IsUnavailable)
                {
                    ClearResults();
                    return;
                }

                if (_model.FolderPath == folder.Path)
                    return;

                //var subFolders = folder.Children;

                //if (subFolders == null)
                //{
                //    if (folder.IsUnavailable) return;
                //    subFolders = ServiceLocator.FolderService.GetSubFolders(folder);
                //    folder.HasChildren = subFolders.Any();
                //    folder.Children = subFolders;
                //}


                _model.MainModel.CurrentFolder = folder;
                _model.MainModel.ActiveView = "Folders";

                SetView("folders");

                _model.FolderPath = folder.Path;

                if (!folder.IsUnavailable && !Directory.Exists(folder.Path))
                {
                    folder.IsUnavailable = true;
                    ServiceLocator.DataStore.SetUnavailable(new[] { folder.Id }, true);
                }

                SearchImages(null);
            }
            catch (Exception e)
            {
                _model.IsBusy = false;
            }

        }

        private async Task ToggleFolder(FolderViewModel folder, FolderState? forceState = null)
        {
            var parentIndex = ServiceLocator.MainModel.Folders.IndexOf(folder);
            var parentDepth = folder.Depth;

            void Collapse()
            {
                if (folder.Children != null)
                {
                    CollapseVisualTree(folder);

                    //model.Children = null;
                }

                folder.State = FolderState.Collapsed;
            }

            async Task Expand()
            {
                var subFolders = folder.Children;

                if (subFolders == null)
                {
                    folder.IsBusy = true;

                    subFolders = await Task.Run(() =>
                    {
                        try
                        {
                            return ServiceLocator.FolderService.GetSubFolders(folder);
                        }
                        finally
                        {
                            Dispatcher.Invoke(() => { folder.IsBusy = false; });
                        }
                    });

                    folder.Children = subFolders;

                    if (subFolders.Any())
                    {
                        foreach (var subFolder in subFolders)
                        {
                            ServiceLocator.FolderService.InsertChild(parentIndex, parentDepth, subFolder);
                        }
                    }

                }
                else
                {
                    foreach (var child in folder.Children)
                    {
                        if (!ServiceLocator.MainModel.Folders.Contains(child))
                        {
                            ServiceLocator.FolderService.InsertChild(parentIndex, parentDepth, child);
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


            switch (forceState)
            {
                case FolderState.Collapsed:
                    Collapse();
                    return;
                case FolderState.Expanded:
                    await Expand();
                    return;
            }

            if (folder.State == FolderState.Collapsed)
            {
                await Expand();
            }
            else
            {
                Collapse();
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

        private void NotScanned_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_model.MainModel.CurrentFolder != null)
            {
                _ = ServiceLocator.ScanningService.ScanFolder(_model.MainModel.CurrentFolder, true);
            }
        }

        private void Folder_Move(object sender, MouseEventArgs e)
        {
            //Point mpos = e.GetPosition(null);
            //Vector diff = this._start - mpos;

            //if (_dragStarted && e.LeftButton == MouseButtonState.Pressed && (e.OriginalSource is Grid or TextBlock) &&
            //    (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
            //     Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            //{
            //    var selectedFolders = ServiceLocator.FolderService.SelectedFolders.ToList();

            //    if (!selectedFolders.Any())
            //    {
            //        return;
            //    }

            //    if (selectedFolders.Any(d => d.Parent == null))
            //    {
            //        return;
            //    }


            //    DataObject dataObject = new DataObject();
            //    dataObject.SetData(DataFormats.FileDrop, selectedFolders.Select(t => t.Path).ToArray());
            //    dataObject.SetData(DragAndDrop.DragFolders, selectedFolders.Select(t => t.Id).ToArray());

            //    DragDrop.DoDragDrop((DependencyObject)sender, dataObject, DragDropEffects.Move | DragDropEffects.Copy);
            //}
        }

        private void Folder_Release(object sender, MouseButtonEventArgs e)
        {
            _dragStarted = false;
        }

        private void DropImagesOnFolder(object sender, DragEventArgs e)
        {
            var folder = (FolderViewModel)((FrameworkElement)sender).DataContext;

            // (e.Effects & DragDropEffects.Move) != 0

            if (e.Data.GetDataPresent(DragAndDrop.DragFolders))
            {
                int[] folders = (int[])e.Data.GetData(DragAndDrop.DragFolders);


            }
            else if (e.Data.GetDataPresent(DragAndDrop.DragFiles))
            {
                ImageEntry[] files = (ImageEntry[])e.Data.GetData(DragAndDrop.DragFiles);

                var imagePaths = files.Where(d => d.EntryType == EntryType.File)
                    .Select(d => new ImagePath() { Id = d.Id, Path = d.Path }).ToList();

                ServiceLocator.FileService.MoveFiles(imagePaths, folder.Path, false).ContinueWith(d =>
                {
                    if (d.IsCompletedSuccessfully)
                    {
                        if (d.Result)
                        {
                            SearchImages(null);
                        }
                    }
                });
            }
        }

        private void Folder_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is ContentPresenter { DataContext: FolderViewModel folder })
            {
                if (e.Key == Key.Enter)
                {
                    ServiceLocator.FolderService.ClearSelection();
                    OpenFolder(folder);
                    ServiceLocator.MainModel.CurrentFolder = folder;
                    folder.IsSelected = true;
                }
                else if (e.Key == Key.Left)
                {
                    _ = ToggleFolder(folder, FolderState.Collapsed);
                }
                else if (e.Key == Key.Right)
                {
                    _ = ToggleFolder(folder, FolderState.Expanded);
                }
                else if (e.Key == Key.PageUp)
                {
                    var index = ServiceLocator.MainModel.Folders.IndexOf(folder);

                    index -= 10;
                    if (index < 0)
                    {
                        index = 0;
                    }

                    var currentFolder = ServiceLocator.MainModel.Folders[index];

                    ServiceLocator.FolderService.ClearSelection();
                    ServiceLocator.MainModel.CurrentFolder = currentFolder;
                    currentFolder.IsSelected = true;

                }
                else if (e.Key == Key.PageDown)
                {
                    var index = ServiceLocator.MainModel.Folders.IndexOf(folder);

                    index += 10;
                    if (index > ServiceLocator.MainModel.Folders.Count - 1)
                    {
                        index = ServiceLocator.MainModel.Folders.Count - 1;
                    }

                    var currentFolder = ServiceLocator.MainModel.Folders[index];

                    ServiceLocator.FolderService.ClearSelection();
                    ServiceLocator.MainModel.CurrentFolder = currentFolder;
                    currentFolder.IsSelected = true;

                }
            }

        }

        private void AddRootFolder_OnClick(object sender, RoutedEventArgs e)
        {
            var window = ServiceLocator.WindowService.CurrentWindow;

            using var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;

            if (dialog.ShowDialog(window) == CommonFileDialogResult.Ok)
            {
                var path = dialog.FileName;

                if (ServiceLocator.FolderService.RootFolders.Any(d => path.StartsWith(d.Path + "\\", StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show(window,
                        "The selected folder is already included in the path of one of the existing folders",
                        "Add folder", MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }
                else if (ServiceLocator.FolderService.RootFolders.Any(d => d.Path.Equals(path, StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                }
                else if (ServiceLocator.FolderService.RootFolders.Any(d => d.Path.StartsWith(path, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show(window,
                        "One or more of the existing folders is included the path of the selected folder",
                        "Add folder", MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var recursiveScan = MessageBox.Show(window,
                    "Do you want this folder to be scanned recursively?",
                    "Add folder", MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                _ = ServiceLocator.FolderService.ApplyFolderChanges(new[]
                {
                    new FolderChange()
                    {
                        Path = path,
                        FolderType = FolderType.Root,
                        ChangeType = ChangeType.Add,
                        Recursive = recursiveScan == MessageBoxResult.Yes,
                    }
                }, true);
            }
        }
    }
}
