using Avalonia.Controls.Notifications;
using BrowseScape.Core.Interfaces;
using Notification = BrowseScape.Core.Models.Notification;

namespace BrowseScape.Shell.Services
{
  public class NotificationService : INotificationService
  {
    private readonly INotificationManager _notificationManager;

    public NotificationService(INotificationManager notificationManager)
    {
      _notificationManager = notificationManager;
    }
    public void Show(Notification notification)
    {
      _notificationManager.Show(new Avalonia.Controls.Notifications.Notification(notification.Title, notification.Message, (NotificationType)notification.Type, notification.Expiration, notification.OnClick, notification.OnClose));
    }
  }
}
