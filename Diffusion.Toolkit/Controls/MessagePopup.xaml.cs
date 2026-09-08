using System;
using Diffusion.Toolkit.Models;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Diffusion.Toolkit.Classes;
using System.Windows.Controls.Primitives;
using System.Globalization;
using System.Windows.Media;

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

        public async Task CloseAsync()
        {
            try
            {
                await _popup.WaitUntilReady();
            }
            finally
            {
                _popup.Close();
                _closing?.Invoke();
            }
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
            _manager.Close(this);
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
            _semaphore = new SemaphoreSlim(0);

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


            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private bool _unloaded = false;

        public async Task WaitUntilReady()
        {
            if (!_unloaded)
            {
                await _semaphore.WaitAsync();
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _semaphore.Dispose();
            _unloaded = true;
        }

        private SemaphoreSlim _semaphore;

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _semaphore.Release();
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
            _model.Height = CalculateHeight(message);

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
            _model.Height = CalculateHeight(message);

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
        private bool _selectAll;

        private double CalculateHeight(string message)
        {
            var titleSpace = 36;
            var buttonSpace = 36;

            var borders = 15 + 15;

            var textBlockBottomMargin = 15;

            var sideMargins = 10 + 10 + borders;
            var margins = 10 + 10 + borders;

            double inputHeight = 0;

            if (_model.ShowInput)
            {
                InputTextBox.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                inputHeight = InputTextBox.DesiredSize.Height;
            }

            var textHeight = MeasureTextHeightWithTextBlock(message, _model.Width - sideMargins, this.textBlock.FontSize);
            var altTextHeight = MeasureString(message, _model.Width - sideMargins, this.textBlock.FontSize);

            return altTextHeight + titleSpace + buttonSpace + margins + textBlockBottomMargin + inputHeight;
        }

        public Task<PopupResult> Show(string message, string title, PopupButtons buttons, PopupResult defaultResult,
            bool selectAll = true)
        {
            _model.Width = 400;
            _model.Height = CalculateHeight(message);

            _model.IsVisible = true;
            _model.Title = title;
            _model.Message = message;

            _model.HasOk = buttons.HasFlag(PopupButtons.OK);
            _model.HasCancel = buttons.HasFlag(PopupButtons.Cancel);
            _model.HasYes = buttons.HasFlag(PopupButtons.Yes);
            _model.HasNo = buttons.HasFlag(PopupButtons.No);

            _selectAll = selectAll;

            _defaultResult = defaultResult;


            return _tcs.Task;
        }

        private double MeasureString(string candidate, double maxWidth, double fontSize)
        {
            var formattedText = new FormattedText(
                candidate,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(this.textBlock.FontFamily, this.textBlock.FontStyle, this.textBlock.FontWeight, this.textBlock.FontStretch),
                fontSize,
                Brushes.Black,
                new NumberSubstitution(),
                TextFormattingMode.Display,
                VisualTreeHelper.GetDpi(this.textBlock).PixelsPerDip);

            formattedText.MaxTextWidth = maxWidth;
            formattedText.Trimming = TextTrimming.None;

            return formattedText.Height;
        }

        public double MeasureTextHeightWithTextBlock(string text, double constrainedWidth, double fontSize)
        {
            var textBlock = new TextBlock
            {
                Text = text,
                FontSize = fontSize,
                TextWrapping = TextWrapping.Wrap,
                Width = constrainedWidth
            };

            textBlock.Measure(new Size(constrainedWidth, double.PositiveInfinity));

            return textBlock.DesiredSize.Height;
        }

        public Task<PopupResult> ShowMedium(string message, string title, PopupButtons buttons, PopupResult defaultResult)
        {
            _model.Width = 500;
            _model.Height = CalculateHeight(message);

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
            _model.Height = CalculateHeight(message);

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

                if (_selectAll && Text is { Length: > 0 })
                {
                    InputTextBox.Text = Text;
                    InputTextBox.SelectionStart = 0;
                    InputTextBox.SelectionLength = Text.Length;

                    //Task.Delay(200).ContinueWith((t) =>
                    //{
                    //    Dispatcher.Invoke(() =>
                    //    {
                    //        InputTextBox.SelectionStart = 0;
                    //        InputTextBox.SelectionLength = Text.Length;
                    //    });
                    //});
                }
            }
        }
    }
}
