namespace Application.Interfaces;

public interface INotificationService
{
    Task NotifyUserAsync(string userId, string eventName, object payload);
}
