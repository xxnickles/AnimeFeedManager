namespace AnimeFeedManager.WebApp.Components.Notifications;

public readonly record struct NotificationBodyPayload(UiNotification Notification, NotificationFor Target);