﻿using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using AnimeFeedManager.Common;
using AnimeFeedManager.Common.Notifications;
using AnimeFeedManager.Storage.Domain;
using AnimeFeedManager.Storage.Infrastructure;
using AnimeFeedManager.Storage.Interface;

namespace AnimeFeedManager.Storage.Repositories;

public class NotificationsRepository : INotificationsRepository
{
    private readonly TableClient _tableClient;
    private readonly JsonSerializerOptions _serializerOptions;

    public NotificationsRepository(ITableClientFactory<NotificationStorage> tableClientFactory)
    {
        _tableClient = tableClientFactory.GetClient();
        _tableClient.CreateIfNotExistsAsync().GetAwaiter().GetResult();
        _serializerOptions = new JsonSerializerOptions()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };
    }

    public Task<Either<DomainError, ImmutableList<NotificationStorage>>> GetForUser(string userId)
    {
        return TableUtils.ExecuteLimitedQuery(() =>
            _tableClient.QueryAsync<NotificationStorage>(n => n.PartitionKey == userId),
            nameof(NotificationStorage), 200);
    }

    public Task<Either<DomainError, ImmutableList<NotificationStorage>>> GetForAdmin(string userId)
    {
        return TableUtils.ExecuteLimitedQuery(() =>
                _tableClient.QueryAsync<NotificationStorage>(n =>
                    n.PartitionKey == userId || n.PartitionKey == UserRoles.Admin ),
            nameof(NotificationStorage), 200);
    }

    public Task<Either<DomainError, Unit>> Merge<T>(string id,string userId, NotificationFor @for, NotificationType type, T payload)
    {
        var notificationStorage = new NotificationStorage
        {
            PartitionKey = @for != NotificationFor.Admin ? userId : UserRoles.Admin,
            RowKey = id,
            Payload = JsonSerializer.Serialize(payload, _serializerOptions),
            Type = type.Value,
            For = @for.Value
        };
        return TableUtils
            .TryExecute(() => _tableClient.UpsertEntityAsync(notificationStorage), nameof(NotificationStorage))
            .MapAsync(_ => unit);
    }
}