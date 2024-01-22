using System;
using Diffusion.Toolkit.Models;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Diffusion.Toolkit.Classes;
using System.Windows.Controls.Primitives;

namespace Diffusion.Toolkit.Controls
{
    public class MessagePopupHandle
    {
        private readonly MessagePopup _popup;
        private Action _closing;

        public MessagePopupHandle(MessagePopup popup)
        {
            _popup = popup;
        }

        public MessagePopupHandle ContinueWith(Action closing)
        {
            _closing = closing;
            return this;
        }

        public void Close()
        {
            _popup.Close();
            _closing?.Invoke();
        }
    }

    /// <summary>
    /// Interaction logic for MessagePopup.xaml
    /// </summary>
    public partial class MessagePopup : UserControl
    {
        private readonly MessagePopupManager _manager;
        private int _timeout;
        private MessagePopupModel _model;
        private TaskCompletionSource<PopupResult> _tcs;

        private Timer t;
        private Timer t2;

        public void Cancel()
        {
            _model.IsVisible = false;
            _tcs.SetResult(_defaultResult);
            Close();
        }

        public void Close()
        {
            t = new Timer(Callback, null, 1000, Timeout.Infinite);
        }

        public string? Text
        {
            get => _model.Input;
            set => _model.Input = value;
        }

        private void Callback(object? state)
        {
            t?.Dispose();
            t2?.Dispose();
            _manager.Close(this);
        }


        private void Callback2(object? state)
        {
            if (_timeout > 0) _timeout--;
            if (_timeout == 0)
            {
                t2.Dispose();
                _model.IsVisible = false;
                _tcs.SetResult(PopupResult.OK);
                Close();
            }
        }

        public MessagePopup(MessagePopupManager manager, UIElement placementTarget, int timeout) : this(manager, placementTarget, timeout, false)
        {
        }


        public MessagePopup(MessagePopupManager manager, UIElement placementTarget, int timeout, bool showInput)
        {
            _manager = manager;
            _timeout = timeout;
            InitializeComponent();

            if (timeout > 0)
            {
                t2 = new Timer(Callback2, null, 1000, 1000);
            }

            _model = new MessagePopupModel();

            _tcs = new TaskCompletionSource<PopupResult>();

            _model.ShowInput = showInput;

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

            _defaultResult = PopupResult.OK;

            //_tcs = new TaskCompletionSource<PopupResult>();

            return _tcs.Task;
        }

        public MessagePopupHandle ShowMessage(string message, string title, int width = 400, int height = 200)
        {
            _model.Width = width;
            _model.Height = height;

            _model.IsVisible = true;
            _model.Title = title;
            _model.Message = message;

            _model.HasOk = false;
            _model.HasCancel = false;
            _model.HasYes = false;
            _model.HasNo = false;

            _defaultResult = PopupResult.OK;

            //_tcs = new TaskCompletionSource<PopupResult>();

            return new MessagePopupHandle(this);
        }

        private PopupResult _defaultResult;

        public Task<PopupResult> Show(string message, string title, PopupButtons buttons, PopupResult defaultResult)
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

            _defaultResult = defaultResult;


            return _tcs.Task;
        }

        public Task<PopupResult> ShowMedium(string message, string title, PopupButtons buttons, PopupResult defaultResult)
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

            _defaultResult = defaultResult;

            return _tcs.Task;
        }

        public Task<PopupResult> ShowCustom(string message, string title, PopupButtons buttons, PopupResult defaultResult, int width, int height)
        {
            _model.Width = width;
            _model.Height = height;

            _model.IsVisible = true;
            _model.Title = title;
            _model.Message = message;

            _model.HasOk = buttons.HasFlag(PopupButtons.OK);
            _model.HasCancel = buttons.HasFlag(PopupButtons.Cancel);
            _model.HasYes = buttons.HasFlag(PopupButtons.Yes);
            _model.HasNo = buttons.HasFlag(PopupButtons.No);

            _defaultResult = defaultResult;

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

        private void UIElement_OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    _model.IsVisible = false;
                    _tcs.SetResult(PopupResult.OK);
                    Close();
                    break;
            }
        }

        private void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_model.ShowInput)
            {
                InputTextBox.Focus();
                Keyboard.Focus(InputTextBox);
            }
        }
    }
}
