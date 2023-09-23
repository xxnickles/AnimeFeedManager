﻿using AnimeFeedManager.Features.Common.Domain.Errors;
using AnimeFeedManager.Features.Common.Domain.Types;
using AnimeFeedManager.Features.Notifications.Types;

namespace AnimeFeedManager.Features.Notifications.IO;

public interface IGetNotifications
{
    Task<Either<DomainError, ImmutableList<NotificationStorage>>> GetForUser(string userId, CancellationToken token);

    Task<Either<DomainError, ImmutableList<NotificationStorage>>> GetForAdmin(string userId, CancellationToken token);
}

public class GetNotifications : IGetNotifications
{
    private readonly ITableClientFactory<NotificationStorage> _tableClientFactory;

    public GetNotifications(ITableClientFactory<NotificationStorage> tableClientFactory)
    {
        _tableClientFactory = tableClientFactory;
    }

    public Task<Either<DomainError, ImmutableList<NotificationStorage>>> GetForUser(string userId,
        CancellationToken token)
    {
        return _tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.ExecuteLimitedQuery(() =>
                client.QueryAsync<NotificationStorage>(n => n.PartitionKey == userId, cancellationToken: token)));
    }

    public Task<Either<DomainError, ImmutableList<NotificationStorage>>> GetForAdmin(string userId,
        CancellationToken token)
    {
        return _tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.ExecuteLimitedQuery(() =>
                client.QueryAsync<NotificationStorage>(
                    n => n.PartitionKey == userId || n.PartitionKey == UserRoles.Admin, cancellationToken: token)));
    }
}