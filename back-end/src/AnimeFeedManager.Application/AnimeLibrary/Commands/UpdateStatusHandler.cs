﻿using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Services.Collectors.HorribleSubs;
using AnimeFeedManager.Storage.Domain;
using AnimeFeedManager.Storage.Interface;
using LanguageExt;
using MediatR;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AnimeFeedManager.Application.AnimeLibrary.Commands
{
    public class UpdateStatusHandler : IRequestHandler<UpdateStatus, Either<DomainError, ImmutableList<AnimeInfoStorage>>>
    {
        private readonly IFeedTitlesProvider _feedTitlesProvider;
        private readonly IAnimeInfoRepository _animeInfoRepository;

        public UpdateStatusHandler(IFeedTitlesProvider feedTitlesProvider, IAnimeInfoRepository animeInfoRepository)
        {
            _feedTitlesProvider = feedTitlesProvider;
            _animeInfoRepository = animeInfoRepository;
        }

        public Task<Either<DomainError, ImmutableList<AnimeInfoStorage>>> Handle(UpdateStatus request, CancellationToken cancellationToken)
        {
           return _feedTitlesProvider.GetTitles().BindAsync(UpdateStatus);
        }

        private async Task<Either<DomainError, ImmutableList<AnimeInfoStorage>>> UpdateStatus(ImmutableList<string> animes)
        {
            return await _animeInfoRepository.GetIncomplete()
                 .MapAsync(al => al.Where(x => !string.IsNullOrEmpty(x.FeedTitle) && !animes.Contains(x.FeedTitle)))
                 .MapAsync(al => al.Select(MarkAsCompleted))
                 .MapAsync(al => al.ToImmutableList());
        }

        private AnimeInfoStorage MarkAsCompleted(AnimeInfoStorage original)
        {
            original.Completed = true;
            return original;
        }

    }
}