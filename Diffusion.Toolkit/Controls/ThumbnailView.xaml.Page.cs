using System;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace Diffusion.Toolkit.Controls
{
    public class PageChangedEventArgs
    {
        public int Page { get; set; }
        public bool GotoEnd { get; set; }
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
                    GotoEnd = gotoEnd,
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
                    OnCompleted = onCompleted
                };

                PageChangedCommand?.Execute(args);
            }
        }

        public void SetPagingEnabled()
        {
            Model.SetPagingEnabled(Model.Page);
        }

        public void ReloadThumbnailsView(double offset)
        {
            var wrapPanel = GetChildOfType<WrapPanel>(this)!;

            if (wrapPanel == null || wrapPanel.Children.Count == 0)
                return;

            var scrollViewer = GetChildOfType<ScrollViewer>(this)!;

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
                        imageEntry.QueueLoadThumbnail();
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

            //var start = ((int)offset / (int)item.ActualHeight * columnWidth);
            //var end = ((int)(offset + height) / (int)item.ActualHeight * columnWidth + columnWidth);

            //end = Math.Min(wrapPanel.Children.Count, end);

            //for (int i = start; i < end; i++)
            //{
            //    item = wrapPanel.Children[i] as ListViewItem;

            //    if (item?.DataContext is ImageEntry { LoadState: LoadState.Unloaded } imageEntry)
            //    {
            //        imageEntry.LoadThumbnail();
            //    }
            //}
        }

        private void ScrollViewer_OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            _debounceRedrawThumbnails(e.VerticalOffset);
            //ReloadThumbnailsView(e.VerticalOffset);
        }

    }

    public static class Utility
    {
        public static Action Debounce(Action func, int milliseconds = 300)
        {
            CancellationTokenSource? cancelTokenSource = null;

            return () =>
            {
                cancelTokenSource?.Cancel();
                cancelTokenSource = new CancellationTokenSource();

                Task.Delay(milliseconds, cancelTokenSource.Token)
                    .ContinueWith(t =>
                    {
                        if (t.IsCompletedSuccessfully)
                        {
                            func();
                        }
                    }, TaskScheduler.Default);
            };
        }

        public static Action<T> Debounce<T>(Action<T> func, int milliseconds = 300)
        {
            CancellationTokenSource? cancelTokenSource = null;

            return (arg) =>
            {
                cancelTokenSource?.Cancel();
                cancelTokenSource = new CancellationTokenSource();

                Task.Delay(milliseconds, cancelTokenSource.Token)
                    .ContinueWith(t =>
                    {
                        if (t.IsCompletedSuccessfully)
                        {
                            func(arg);
                        }
                    }, TaskScheduler.Default);
            };
        }
    }
}
