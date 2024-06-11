using System;

namespace DiffusionToolkit.AvaloniaApp.Services;

public class NotificationService
{
    public event EventHandler<string> Notify;

    public void SetNotification(string text)
    {
        Notify?.Invoke(this, text);
    }
}