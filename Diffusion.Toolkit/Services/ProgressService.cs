using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Diffusion.Toolkit.Localization;
using Diffusion.Toolkit.Models;

namespace Diffusion.Toolkit.Services;

public class ProgressService
{
    private CancellationTokenSource _progressCancellationTokenSource;
    private Dispatcher _dispatcher;

    public CancellationToken CancellationToken => _progressCancellationTokenSource.Token;

    private string GetLocalizedText(string key)
    {
        return (string)JsonLocalizationProvider.Instance.GetLocalizedObject(key, null, CultureInfo.InvariantCulture);
    }

    public ProgressService(Dispatcher dispatcher)
    {
        _dispatcher = dispatcher;
        //_dispatcher = Dispatcher.CurrentDispatcher;
    }

    public async Task<bool> TryStartTask()
    {
        if (ServiceLocator.MainModel.IsBusy)
        {
            await _dispatcher.InvokeAsync(async () => { await ServiceLocator.MessageService.Show(GetLocalizedText("Common.MessageBox.OperationInProgress"), GetLocalizedText("Common.MessageBox.Title"), PopupButtons.YesNo); });
            return false;
        }

        _progressCancellationTokenSource = new CancellationTokenSource();

        _dispatcher.Invoke(() =>
        {
            ServiceLocator.MainModel.IsBusy = true;
        });

        return true;
    }

    public void CompleteTask()
    {
        _dispatcher.Invoke(() =>
        {
            ServiceLocator.MainModel.IsBusy = false;
        });
    }

    public async Task CancelTask()
    {
        var dialogResult = await ServiceLocator.MessageService.Show(GetLocalizedText("Common.MessageBox.ConfirmCancelOperation"), GetLocalizedText("Common.MessageBox.Cancel"), PopupButtons.YesNo);

        if (dialogResult == PopupResult.Yes)
        {
            _progressCancellationTokenSource.Cancel();
        }
    }
}