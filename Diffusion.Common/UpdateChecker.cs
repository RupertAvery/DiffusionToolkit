using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Diffusion.Github;

namespace Diffusion.Common;

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

    public static Action<T1, T2> Debounce<T1, T2>(Action<T1, T2> func, int milliseconds = 300)
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
public class UpdateChecker
{
    public GithubClient Client { get; }
    private CancellationTokenSource _cts;

    public void Cancel()
    {
        _cts.Cancel();
    }

    public CancellationToken CancellationToken => _cts.Token;

    public Release LatestRelease { get; private set; }

    public UpdateChecker()
    {
        _cts = new CancellationTokenSource();
        Client = new GithubClient("RupertAvery", "DiffusionToolkit");
    }

    private async Task<Release> GetLatestRelease()
    {
        var releases = await Client.GetReleases(_cts.Token);

        return releases.OrderByDescending(r => r.published_at).First();
    }



    public async Task<bool> CheckForUpdate(string? path = null)
    {
        LatestRelease = await GetLatestRelease();

        var localVersion = SemanticVersionHelper.GetLocalVersion(path);

        SemanticVersion.TryParse(LatestRelease.tag_name, out var releaseVersion);

        return releaseVersion > localVersion;
    }

}