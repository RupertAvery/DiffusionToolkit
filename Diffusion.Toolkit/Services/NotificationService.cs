using System;
using System.Windows.Threading;

namespace Diffusion.Toolkit.Services
{
    public class NotificationService
    {
        public event EventHandler<string> Notify;

        public void SetNotification(string text)
        {
            Notify?.Invoke(this, text);
        }

        public void Toast(string copiedPathToClipboard)
        {
            throw new NotImplementedException();
        }
    }
}
