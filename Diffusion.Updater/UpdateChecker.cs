namespace Diffusion.Updater;

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



    public async Task<bool> CheckForUpdate()
    {
        LatestRelease = await GetLatestRelease();

        var localVersion = SemanticVersionHelper.GetLocalVersion();

        SemanticVersion.TryParse(LatestRelease.tag_name, out var releaseVersion);

        return releaseVersion > localVersion;
    }

}