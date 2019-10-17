﻿using System.Collections.Immutable;
using System.Threading.Tasks;
using AnimeFeedManager.Core.Domain;
using AnimeFeedManager.Core.Error;
using LanguageExt;

namespace AnimeFeedManager.Services.Collectors.LiveChart
{
    public interface IExternalLibraryProvider
    {
        Task<Either<DomainError, ImmutableList<AnimeInfo>>> GetLibrary();
    }
}