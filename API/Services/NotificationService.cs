using API.Hubs;
using Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace API.Services;

public class NotificationService(IHubContext<NotificationHub> hubContext) : INotificationService
{
    public async Task NotifyUserAsync(string userId, string eventName, object payload)
    {
        await hubContext.Clients.Group(userId).SendAsync(eventName, payload);
    }
}