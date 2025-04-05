using System;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Diffusion.Toolkit.Pages;
using Diffusion.Toolkit.Services;

namespace Diffusion.Toolkit.Controls
{
    public class PageChangedEventArgs
    {
        public int Page { get; set; }
        public CursorPosition CursorPosition { get; set; }
        public Action? OnCompleted { get; set; }
    }

    public partial class ThumbnailView
    {
        public void GoFirstPage(Action? onCompleted)
        {
            Model.Page = 1;

            var args = new PageChangedEventArgs()
            {
                Page = Model.Page,
                OnCompleted = onCompleted

            };

            PageChangedCommand?.Execute(args);
        }

        public void GoLastPage(Action? onCompleted)
        {
            Model.Page = Model.Pages;

            var args = new PageChangedEventArgs()
            {
                Page = Model.Page,
                OnCompleted = onCompleted
            };

            PageChangedCommand?.Execute(args);
        }

        public void GoPrevPage(Action? onCompleted, bool gotoEnd = false)
        {
            if (Model.Page > 1)
            {
                Model.Page--;
                //currentItemIndex = Model.PageSize - 1;

                var args = new PageChangedEventArgs()
                {
                    Page = Model.Page,
                    CursorPosition = gotoEnd ? CursorPosition.End : CursorPosition.Start,
                    OnCompleted = onCompleted
                };

                PageChangedCommand?.Execute(args);
            }

        }

        public void GoNextPage(Action? onCompleted)
        {
            if (Model.Page < Model.Pages)
            {
                Model.Page++;
                //currentItemIndex = 0;

                var args = new PageChangedEventArgs()
                {
                    Page = Model.Page,
                    CursorPosition = CursorPosition.Start,
                    OnCompleted = onCompleted
                };

                PageChangedCommand?.Execute(args);
            }
        }

        public void SetPagingEnabled()
        {
            Model.SetPagingEnabled(Model.Page);
        }

        public void ClearSelection()
        {
            ThumbnailListView.SelectedItems.Clear();
        }

        public void ReloadThumbnailsView()
        {
            var wrapPanel = GetChildOfType<WrapPanel>(this)!;

            if (wrapPanel == null || wrapPanel.Children.Count == 0)
                return;

            var scrollViewer = GetChildOfType<ScrollViewer>(this)!;

            var offset = scrollViewer.VerticalOffset;

            var height = scrollViewer.ViewportHeight;

            var item = wrapPanel.Children[0] as ListViewItem;

            var preloadSize = item.ActualHeight * 2;

            double top = 0;
            double left = 0;
            var maxHeight = item.ActualHeight;

            for (var i = 0; i < wrapPanel.Children.Count; i++)
            {
                item = wrapPanel.Children[i] as ListViewItem;
                
                if (top + item.ActualHeight >= (offset - preloadSize) && top <= (offset + height + preloadSize))
                {
                    if (item?.DataContext is ImageEntry { LoadState: LoadState.Unloaded } imageEntry)
                    {
                        ServiceLocator.ThumbnailService.QueueImage(imageEntry);
                    }
                }

                if (item.ActualHeight > maxHeight)
                {
                    maxHeight = item.ActualHeight;
                }

                left += item.ActualWidth;

                if (left + item.ActualWidth > wrapPanel.ActualWidth)
                {
                    top += maxHeight;
                    maxHeight = item.ActualHeight;
                    left = 0;
                }
            }
        }

        private void ScrollViewer_OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            _debounceRedrawThumbnails();
        }

    }

}
