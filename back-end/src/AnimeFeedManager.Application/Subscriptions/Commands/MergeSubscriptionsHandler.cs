﻿using System.Threading;
using System.Threading.Tasks;
using AnimeFeedManager.Core.Domain;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Core.Utils;
using AnimeFeedManager.Storage.Interface;
using LanguageExt;

namespace AnimeFeedManager.Application.Subscriptions.Commands;

public sealed record MergeSubscriptionCmd(string Subscriber, string AnimeId) : MediatR.IRequest<Either<DomainError, Unit>>;

public class MergeSubscriptionsHandler : MediatR.IRequestHandler<MergeSubscriptionCmd, Either<DomainError, Unit>>
{
    private readonly ISubscriptionRepository _subscriptionRepository;

    public MergeSubscriptionsHandler(ISubscriptionRepository subscriptionRepository) =>
        _subscriptionRepository = subscriptionRepository;

    public Task<Either<DomainError, Unit>> Handle(MergeSubscriptionCmd request, CancellationToken cancellationToken)
    {
        return Validate(request)
            .ToEither(nameof(MergeSubscriptionCmd))
            .BindAsync(Persist);
    }

    private Validation<ValidationError, Subscription> Validate(MergeSubscriptionCmd request) =>
        (Helpers.SubscriberMustBeValid(request.Subscriber), Helpers.IdListMustHaveElements(request.AnimeId))
        .Apply((subscriber, id) => new Subscription(subscriber, id));


    private Task<Either<DomainError, Unit>> Persist(Subscription subscription)
    {
        return _subscriptionRepository.Merge(Helpers.MapToStorage(subscription));
    }
}