﻿using AnimeFeedManager.Features.Movies.Scrapping.Types.Storage;
using AnimeFeedManager.Features.Notifications.Types;
using AnimeFeedManager.Features.Ovas.Scrapping.Types.Storage;
using AnimeFeedManager.Features.Seasons.Types;
using AnimeFeedManager.Features.State.Types;
using AnimeFeedManager.Features.Tv.Scrapping.Titles.Types;
using AnimeFeedManager.Features.Tv.Subscriptions.Types;
using AnimeFeedManager.Features.Tv.Types;
using AnimeFeedManager.Features.Users.Types;

namespace AnimeFeedManager.Features.Infrastructure.TableStorage;

public sealed class TableClientFactory<T> : ITableClientFactory<T> where T : ITableEntity
{
    private readonly TableServiceClient _serviceClient;

    public TableClientFactory(
        TableServiceClient serviceClient)
    {
        _serviceClient = serviceClient;
    }

    public async Task<Either<DomainError,TableClient>> GetClient()
    {
        try
        {
            var client = _serviceClient.GetTableClient(TableNameFactory(typeof(T)));
            await client.CreateIfNotExistsAsync();
            return client;
        }
        catch (Exception e)
        {
            return ExceptionError.FromException(e);
        }
      
    }
    
    private static string TableNameFactory(Type type)
    {
        return type.Name switch
        {
            nameof(AnimeInfoStorage) => AzureTableMap.StoreTo.AnimeLibrary,
            nameof(AnimeInfoWithImageStorage) => AzureTableMap.StoreTo.AnimeLibrary,
            nameof(SubscriptionStorage) => AzureTableMap.StoreTo.Subscriptions,
            nameof(SeasonStorage) => AzureTableMap.StoreTo.AvailableSeasons,
            nameof(InterestedStorage) => AzureTableMap.StoreTo.InterestedSeries,
            nameof(TitlesStorage) => AzureTableMap.StoreTo.FeedTitles,
            // nameof(ProcessedTitlesStorage) => AzureTable.TableMap.ProcessedTitles,   
            nameof(UserStorage) => AzureTableMap.StoreTo.Users,
            nameof(OvaStorage) => AzureTableMap.StoreTo.OvaLibrary,
            // nameof(OvasSubscriptionStorage) => AzureTable.TableMap.OvaSubscriptions,
            nameof(MovieStorage) => AzureTableMap.StoreTo.MovieLibrary,
            // nameof(MoviesSubscriptionStorage) => AzureTable.TableMap.MovieSubscriptions,
            nameof(NotificationStorage) => AzureTableMap.StoreTo.Notifications,
            nameof(StateUpdateStorage) => AzureTableMap.StoreTo.StateUpdates,
            _ => throw new ArgumentException($"There is not a defined table for the type {type.FullName}")
        };
    }
}