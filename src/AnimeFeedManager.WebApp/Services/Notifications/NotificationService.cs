﻿using System.Collections.Immutable;
using AnimeFeedManager.Common.Notifications;
using Blazored.LocalStorage;

namespace AnimeFeedManager.WebApp.Services.Notifications;

public interface INotificationService
{
    event Action? NotificationsUpdated;
    ImmutableList<ServerNotification> Notifications { get; }
    Task AddNotification(ServerNotification notification);
    Task LoadLocalNotifications();
    Task SetNotificationViewed(string id);
    Task SetAllNotificationViewed();
    Task RemoveAll();
    Task RemoveAdminNotifications();
}

public class NotificationService : INotificationService
{
    private readonly ILocalStorageService _localStorage;
    private const string NotificationsKey = "notifications";
    private const byte MaxNotifications = 5;

    public NotificationService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }
    
    public event Action? NotificationsUpdated;

    public ImmutableList<ServerNotification> Notifications { private set; get; } =
        ImmutableList<ServerNotification>.Empty;

    public async Task AddNotification(ServerNotification notification)
    {
        if (Notifications.Count >= MaxNotifications)
        {
            Notifications = Notifications.RemoveRange(0, Notifications.Count - MaxNotifications + 1);
        }

        Notifications = Notifications.Add(notification);
        await _localStorage.SetItemAsync(NotificationsKey, Notifications);
        NotificationsUpdated?.Invoke();
    }


    public async Task LoadLocalNotifications()
    {
        if (await _localStorage.ContainKeyAsync(NotificationsKey))
        {
            var storedNotifications =
                await _localStorage.GetItemAsync<IEnumerable<ServerNotification>>(NotificationsKey);
            Notifications = storedNotifications?.ToImmutableList() ?? ImmutableList<ServerNotification>.Empty;
        }
        else
        {
            Notifications = ImmutableList<ServerNotification>.Empty;
        }

        NotificationsUpdated?.Invoke();
    }

    public async Task SetNotificationViewed(string id)
    {
        var target = Notifications.FirstOrDefault(n => n.Id == id);
        if (target is not null)
        {
            Notifications = Notifications.Replace(target, target with { Read = true });
            await _localStorage.SetItemAsync(NotificationsKey, Notifications);
            NotificationsUpdated?.Invoke();
        }
    }

    public async Task SetAllNotificationViewed()
    {
        Notifications = Notifications.ConvertAll(n => n with { Read = true });
        await _localStorage.SetItemAsync(NotificationsKey, Notifications);
        NotificationsUpdated?.Invoke();
    }

    public async Task RemoveAll()
    {
        Notifications = ImmutableList<ServerNotification>.Empty;
        await _localStorage.RemoveItemAsync(NotificationsKey);
        NotificationsUpdated?.Invoke();
    }

    public async Task RemoveAdminNotifications()
    {
        if (Notifications.Any())
        {
            Notifications = Notifications.Where(n => n.Audience != TargetAudience.Admins).ToImmutableList();
            await _localStorage.SetItemAsync(NotificationsKey, Notifications);
            NotificationsUpdated?.Invoke();
        }
    }
}