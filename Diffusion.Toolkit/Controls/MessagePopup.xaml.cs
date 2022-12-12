using Diffusion.Toolkit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Diffusion.Toolkit.Classes;

namespace Diffusion.Toolkit.Controls
{
    /// <summary>
    /// Interaction logic for MessagePopup.xaml
    /// </summary>
    public partial class MessagePopup : UserControl
    {
        private readonly MessagePopupManager _manager;
        private MessagePopupModel _model;
        private TaskCompletionSource<PopupResult> _tcs;

        private Timer t;

        public void Close()
        {
            t = new Timer(Callback, null, 1000, Timeout.Infinite);
        }

        private void Callback(object? state)
        {
            t.Dispose();
            _manager.Close(this);
        }

        public MessagePopup(MessagePopupManager manager, UIElement placementTarget)
        {
            _manager = manager;
            InitializeComponent();

            _model = new MessagePopupModel();

            _tcs = new TaskCompletionSource<PopupResult>();

            _model.PlacementTarget = placementTarget;

            _model.OKCommand = new RelayCommand<object>((o) =>
            {
                _model.IsVisible = false;
                _tcs.SetResult(PopupResult.OK);
                Close();
            });

            _model.CancelCommand = new RelayCommand<object>((o) =>
            {
                _model.IsVisible = false;
                _tcs.SetResult(PopupResult.Cancel);
                Close();
            });

            _model.YesCommand = new RelayCommand<object>((o) =>
            {
                _model.IsVisible = false;
                _tcs.SetResult(PopupResult.Yes);
                Close();
            });

            _model.NoCommand = new RelayCommand<object>((o) =>
            {
                _model.IsVisible = false;
                _tcs.SetResult(PopupResult.No);
                Close();
            });

            DataContext = _model;
        }


        private void Clear()
        {
            _model.HasOk = false;
            _model.HasCancel = false;
            _model.HasYes = false;
            _model.HasNo = false;
        }

        public Task<PopupResult> Show(string message, string title)
        {
            _model.Width = 400;
            _model.Height = 200;

            _model.IsVisible = true;
            _model.Title = title;
            _model.Message = message;

            _model.HasOk = true;
            _model.HasCancel = false;
            _model.HasYes = false;
            _model.HasNo = false;

            _tcs = new TaskCompletionSource<PopupResult>();

            return _tcs.Task;
        }

        public Task<PopupResult> Show(string message, string title, PopupButtons buttons)
        {
            _model.Width = 400;
            _model.Height = 200;

            _model.IsVisible = true;
            _model.Title = title;
            _model.Message = message;

            _model.HasOk = buttons.HasFlag(PopupButtons.OK);
            _model.HasCancel = buttons.HasFlag(PopupButtons.Cancel);
            _model.HasYes = buttons.HasFlag(PopupButtons.Yes);
            _model.HasNo = buttons.HasFlag(PopupButtons.No);


            return _tcs.Task;
        }

        public Task<PopupResult> ShowMedium(string message, string title, PopupButtons buttons)
        {
            _model.Width = 500;
            _model.Height = 300;

            _model.IsVisible = true;
            _model.Title = title;
            _model.Message = message;

            _model.HasOk = buttons.HasFlag(PopupButtons.OK);
            _model.HasCancel = buttons.HasFlag(PopupButtons.Cancel);
            _model.HasYes = buttons.HasFlag(PopupButtons.Yes);
            _model.HasNo = buttons.HasFlag(PopupButtons.No);

            return _tcs.Task;
        }

        public void Show()
        {
            _model.IsVisible = true;
        }

        public void Hide()
        {
            _model.IsVisible = false;
        }
    }
}
