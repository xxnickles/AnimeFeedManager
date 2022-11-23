﻿using System.Collections.Immutable;
using AnimeFeedManager.Application.Mappers;
using AnimeFeedManager.Common;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Common.Helpers;
using AnimeFeedManager.Services.Collectors.Interface;
using MediatR;

namespace AnimeFeedManager.Application.TvAnimeLibrary.Queries;

public sealed record LibraryForStorage(
    ImmutableList<AnimeInfoStorage> Animes,
    ImmutableList<BlobImageInfoEvent> Images,
    SeasonInfoDto Season
);

public sealed record GetScrappedLibraryQry(ImmutableList<string> FeedTitles) : IRequest<Either<DomainError, LibraryForStorage>>;

public class GetScrappedLibraryHandler : IRequestHandler<GetScrappedLibraryQry, Either<DomainError, LibraryForStorage>>
{
    private readonly ITvSeriesProvider _tvSeriesProvider;

    public GetScrappedLibraryHandler(ITvSeriesProvider tvSeriesProvider)
    {
        _tvSeriesProvider = tvSeriesProvider;
    }

    public Task<Either<DomainError, LibraryForStorage>> Handle(GetScrappedLibraryQry request,
        CancellationToken cancellationToken)
    {
        return _tvSeriesProvider.GetLibrary(request.FeedTitles).MapAsync(Map);
    }

    private static LibraryForStorage Map(TvSeries source)
    {
        return new LibraryForStorage(
            AnimeInfoMappers.ProjectToStorageModel(source.SeriesList),
            Map(source.Images),
            source.Images.First().SeasonInfo.Map()
        );
    }

    private static ImmutableList<BlobImageInfoEvent> Map(ImmutableList<ImageInformation> source)
    {
        return source.ConvertAll(Map);
    }

    private static BlobImageInfoEvent Map(ImageInformation source)
    {
        var season = source.SeasonInfo.Map();
        var partition = IdHelpers.GenerateAnimePartitionKey(season.Season, (ushort) season.Year);
        var directory = $"{season.Year}/{season.Season}";
        return new BlobImageInfoEvent(
            partition,
            source.Id,
            directory,
            source.Name,
            source.Link ?? string.Empty,
            SeriesType.Tv
        );
    }
}