using Diffusion.Toolkit.MdStyles;
using Diffusion.Toolkit.Models;
using Diffusion.Toolkit.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Diffusion.Toolkit.Pages
{
    public partial class Search
    {
        private void PreviewPane_OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            //ExtOnKeyUp(this, e);
        }

        private void PreviewPane_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            ExtOnKeyDown(this, e);
        }

        private void Album_OnClick(object sender, RoutedEventArgs e)
        {
            var albumModel = ((AlbumModel)((FrameworkElement)sender).DataContext);
            OpenAlbum(albumModel);
        }

        private void AlbumCheck_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedAlbums = ServiceLocator.MainModel.Albums.Where(d => d.IsTicked).ToList();
            ServiceLocator.MainModel.SelectedAlbumsCount = selectedAlbums.Count;
            ServiceLocator.MainModel.HasSelectedAlbums = selectedAlbums.Any();

            SearchImages(null);
        }


        private void Model_OnClick(object sender, RoutedEventArgs e)
        {
            var modelModel = ((Toolkit.Models.ModelViewModel)((Button)sender).DataContext);

            _model.MainModel.CurrentModel = modelModel;

            foreach (var model in _model.MainModel.ImageModels)
            {
                model.IsTicked = false;
            }

            modelModel.IsTicked = true;

            SearchImages(null);
        }

        private void FilterPopup_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                _model.IsFilterVisible = false;
            }
        }

        private void PreviewPane_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OnCurrentImageOpen?.Invoke(_model.CurrentImage);
        }

        private void UIElement_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ItemsControl itemsControl)
            {
                var scrollViewer = FindParentScrollViewer(itemsControl);
                if (scrollViewer != null)
                {
                    bool canScrollUp = scrollViewer.VerticalOffset > 0;
                    bool canScrollDown = scrollViewer.VerticalOffset < scrollViewer.ScrollableHeight;

                    // If it can't scroll in the direction, let parent handle it
                    if ((e.Delta > 0 && !canScrollUp) || (e.Delta < 0 && !canScrollDown))
                    {
                        e.Handled = true;
                        var parentEvent = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                        {
                            RoutedEvent = UIElement.MouseWheelEvent,
                            Source = sender
                        };
                        NavigationScrollViewer.RaiseEvent(parentEvent);


                    }
                    else
                    {
                        e.Handled = false;
                    }
                }
            }
        }

        private ScrollViewer FindParentScrollViewer(DependencyObject current)
        {
            while (current != null)
            {
                if (current is ScrollViewer sv)
                    return sv;

                current = VisualTreeHelper.GetParent(current);
                if (current == null && current is FrameworkElement fe)
                    current = fe.Parent;
            }

            return null;
        }

        private void HideSearchSettings_OnClick(object sender, RoutedEventArgs e)
        {
            CloseSearchSettings();
        }

        public void OpenSearchSettings()
        {
            _model.IsSearchSettingsVisible = true;
        }

        private void CloseSearchSettings()
        {
            _model.IsSearchSettingsVisible = false;
        }


        public void OpenSearchHelp()
        {
            _model.SearchHelpMarkdown = ResourceHelper.GetString("Diffusion.Toolkit.SearchHelp.md");
            _model.SearchHelpStyle = CustomStyles.BetterGithub;
            _model.IsSearchHelpVisible = true;
        }

        private void CloseSearchHelp()
        {
            _model.IsSearchHelpVisible = false;
        }

        private void SearchSettingsPopup_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CloseSearchSettings();
            }
        }

        private void Query_OnClick(object sender, RoutedEventArgs e)
        {
            var queryModel = ((QueryModel)((FrameworkElement)sender).DataContext);

            ServiceLocator.MainModel.CurrentQuery = queryModel;

            foreach (var query in ServiceLocator.MainModel.Queries)
            {
                query.IsSelected = false;
            }

            queryModel.IsSelected = true;

            var q = ServiceLocator.DataStore.GetQuery(queryModel.Id);

            SearchImages(q);
        }

        private void EmptyTrash_OnClick(object sender, RoutedEventArgs e)
        {
            _ = ServiceLocator.FileService.RemoveImagesTaggedForDeletion();
        }

        private void SearchHelpPopup_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CloseSearchHelp();
            }
        }

        private void HideSearchHelp_OnClick(object sender, RoutedEventArgs e)
        {
            CloseSearchHelp();
        }

    }
}
