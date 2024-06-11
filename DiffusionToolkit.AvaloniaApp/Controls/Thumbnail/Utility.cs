using System;
using System.Threading;
using System.Threading.Tasks;

namespace DiffusionToolkit.AvaloniaApp.Controls.Thumbnail;

public static class Utility
{
    public static Action Debounce(this Action func, int milliseconds = 300)
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

    public static Action<T1> Debounce<T1>(this Action<T1> func, int milliseconds = 300)
    {
        CancellationTokenSource? cancelTokenSource = null;

        return (arg1) =>
        {
            cancelTokenSource?.Cancel();
            cancelTokenSource = new CancellationTokenSource();

            Task.Delay(milliseconds, cancelTokenSource.Token)
                .ContinueWith(t =>
                {
                    if (t.IsCompletedSuccessfully)
                    {
                        func(arg1);
                    }
                }, TaskScheduler.Default);
        };
    }

    public static Action<T1, T2> Debounce<T1, T2>(this Action<T1, T2> func, int milliseconds = 300)
    {
        CancellationTokenSource? cancelTokenSource = null;

        return (arg1, arg2) =>
        {
            cancelTokenSource?.Cancel();
            cancelTokenSource = new CancellationTokenSource();

            Task.Delay(milliseconds, cancelTokenSource.Token)
                .ContinueWith(t =>
                {
                    if (t.IsCompletedSuccessfully)
                    {
                        func(arg1, arg2);
                    }
                }, TaskScheduler.Default);
        };
    }
}