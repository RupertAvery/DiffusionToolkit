using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using Diffusion.Toolkit.Services;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Diffusion.Toolkit
{
    public partial class MainWindow
    {
        private void CloseToast(object sender, MouseButtonEventArgs e)
        {
            ServiceLocator.ToastService.DismissToast();
        }

        public CustomPopupPlacement[] GetPopupPlacement(Size popupSize, Size targetSize, Point offset)
        {
            PresentationSource source = PresentationSource.FromVisual(this);

            double dpiX = 96, dpiY = 96;
            if (source != null)
            {
                dpiX = 96.0 * source.CompositionTarget.TransformToDevice.M11;
                dpiY = 96.0 * source.CompositionTarget.TransformToDevice.M22;
            }

            var width = ActualWidth * dpiX / 96.0;
            var height = ActualHeight * dpiY / 96.0;

            var point = new Point(width - popupSize.Width, height - popupSize.Height - 50);
            return new[] { new CustomPopupPlacement(point, PopupPrimaryAxis.None) };
        }

        public CustomPopupPlacement[] GetQueryPopupPlacement(Size popupSize, Size targetSize, Point offset)
        {
            PresentationSource source = PresentationSource.FromVisual(this);

            double dpiX = 96, dpiY = 96;
            if (source != null)
            {
                dpiX = 96.0 * source.CompositionTarget.TransformToDevice.M11;
                dpiY = 96.0 * source.CompositionTarget.TransformToDevice.M22;
            }

            var width = ActualWidth * dpiX / 96.0;
            var height = ActualHeight * dpiY / 96.0;

            var point = new Point(width / 2 - popupSize.Width / 2, 50);
            return new[] { new CustomPopupPlacement(point, PopupPrimaryAxis.None) };
        }

        private void OpenQueryBar()
        {
            QueryPopup.IsOpen = true;
            QueryInput.SelectionStart = 0;
            QueryInput.SelectionLength = QueryInput.Text.Length;
            QueryInput.Focus();
        }

        private void QueryBar_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenQueryBar();
            e.Handled = true;
        }

        private void QueryInput_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                QueryPopup.IsOpen = false;
                _search.SetQuery(QueryInput.Text);
                _search.SearchImages();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                QueryPopup.IsOpen = false;
                e.Handled = true;
            }
        }

        private void QueryFilter_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            _search.ShowFilter();
            e.Handled = true;
        }

        private void QuerySettings_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            _search.OpenSearchSettings();
            e.Handled = true;
        }

        private void QueryRefresh_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            _search.Refresh();
            e.Handled = true;
        }

        private void QueryClear_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            QueryInput.Text = "";
            _search.ClearQueryFilter();
            e.Handled = true;
        }

        private void QueryHelp_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            _search.OpenSearchHelp();
            e.Handled = true;
        }
    }
}