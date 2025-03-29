using System.Threading.Tasks;
using System.Windows.Threading;
using Diffusion.Toolkit.Controls;
using Diffusion.Toolkit.Models;

namespace Diffusion.Toolkit.Services;

public class MessageService
{
    private MessagePopupManager _messagePopupManager;

    public MessageService(MessagePopupManager messagePopupManager)
    {
        _messagePopupManager = messagePopupManager;
    }

    public Task<(PopupResult, string?)> ShowInput(string message, string title, string? defaultText = null)
    {
        return _messagePopupManager.ShowInput(message, title, defaultText);
    }

    public MessagePopupHandle ShowMessage(string message, string title, int timeout = 0)
    {
        return _messagePopupManager.ShowMessage(message, title, timeout);
    }

    public Task<PopupResult> Show(string message, string title, int timeout = 0)
    {
        return _messagePopupManager.Show(message, title, timeout);
    }

    public Task<PopupResult> Show(string message, string title, PopupButtons buttons, int timeout = 0)
    {
        return _messagePopupManager.Show(message, title, buttons, timeout);
    }

    public Task<PopupResult> ShowMedium(string message, string title, PopupButtons buttons, int timeout = 0)
    {
        return _messagePopupManager.ShowMedium(message, title, buttons, timeout);
    }

    public Task<PopupResult> ShowCustom(string message, string title, PopupButtons buttons, int width, int height, int timeout = 0)
    {
        return _messagePopupManager.ShowCustom(message, title, buttons, width, height, timeout);
    }

}