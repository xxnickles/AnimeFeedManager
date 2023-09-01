﻿using AnimeFeedManager.Features.Tv.Subscriptions.Types;

namespace AnimeFeedManager.Features.Tv.Subscriptions.IO;

public interface IRemoveInterestedSeries
{
    public Task<Either<DomainError, Unit>> Remove(UserId userId, NoEmptyString series, CancellationToken token);
}

public sealed class RemoveInterestedSeries : IRemoveInterestedSeries
{
    private readonly ITableClientFactory<InterestedStorage> _clientFactory;

    public RemoveInterestedSeries(ITableClientFactory<InterestedStorage> clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public Task<Either<DomainError, Unit>> Remove(UserId userId, NoEmptyString series, CancellationToken token)
    {
        return _clientFactory.GetClient()
            .BindAsync(client => Delete(client, userId, series, token));
    }

    private static Task<Either<DomainError, Unit>> Delete(TableClient client, UserId userId, NoEmptyString series,
        CancellationToken token)
    {
        return TableUtils.TryExecute(() => client.DeleteEntityAsync(userId, series, cancellationToken: token))
            .MapAsync(_ => unit);
    }
}