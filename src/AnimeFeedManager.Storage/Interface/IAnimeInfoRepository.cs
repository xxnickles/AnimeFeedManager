﻿using System.Collections.Immutable;
using AnimeFeedManager.Storage.Domain;

namespace AnimeFeedManager.Storage.Interface;

public interface IAnimeInfoRepository
{
    Task<Either<DomainError, ImmutableList<AnimeInfoWithImageStorage>>> GetBySeason(Season season, int year);
    Task<Either<DomainError, ImmutableList<AnimeInfoWithImageStorage>>> GetIncomplete();
    Task<Either<DomainError, ImmutableList<AnimeInfoStorage>>> GetAll();
    Task<Either<DomainError, Unit>> Merge(AnimeInfoStorage animeInfo);
    Task<Either<DomainError, Unit>> AddImageUrl(ImageStorage image);
}