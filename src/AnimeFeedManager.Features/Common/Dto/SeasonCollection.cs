﻿namespace AnimeFeedManager.Features.Common.Dto;

public sealed record Feed(bool Available, bool Completed, string? Title);

public abstract record BaseAnime(string Id, string Title, string Synopsis, string? Url);

public sealed record SimpleAnime(string Id, string Title, string Synopsis, string? Url, DateTime? AirDate);

public sealed record FeedAnime(string Id, string Title, string Synopsis, string? Url, Feed FeedInformation);


public record SeasonCollection(ushort Year, string Season, FeedAnime[] Animes);

public record ShortSeasonCollection(ushort Year, string Season, SimpleAnime[] Animes);

public record EmptySeasonCollection() : SeasonCollection(0, string.Empty, System.Array.Empty<FeedAnime>());

public record EmptyShortSeasonCollection() : ShortSeasonCollection(0, string.Empty, System.Array.Empty<SimpleAnime>());