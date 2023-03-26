using MediatR;
using AnimeFeedManager.Core.Utils;
using Unit = LanguageExt.Unit;

namespace AnimeFeedManager.Application.MoviesSubscriptions.Commands;

public sealed record MergeSubscriptionCmd
    (string Subscriber, string Title, DateTime NotificationDate) : IRequest<Either<DomainError, Unit>>;

public class MergeSubscriptionHandler : IRequestHandler<MergeSubscriptionCmd, Either<DomainError, Unit>>
{
    private readonly IMoviesSubscriptionRepository _subscriptionRepository;

    public MergeSubscriptionHandler(IMoviesSubscriptionRepository subscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
    }

    public Task<Either<DomainError, Unit>> Handle(MergeSubscriptionCmd request, CancellationToken cancellationToken)
    {
        return Validate(request)
            .ToEither(nameof(MergeSubscriptionCmd))
            .BindAsync(Persit);
    }

    private Validation<ValidationError, MergeSubscriptionCmd> Validate(MergeSubscriptionCmd request)
    {
        return (ValidationHelpers.EmailMustBeValid(request.Subscriber),
                ValidationHelpers.TitleMustBeValid(request.Title))
            .Apply((subscriber, title) => new MergeSubscriptionCmd(subscriber, title, request.NotificationDate));
    }

    private Task<Either<DomainError, Unit>> Persit(MergeSubscriptionCmd request)
    {
        return _subscriptionRepository.Merge(Map(request));
    }

    private static MoviesSubscriptionStorage Map(MergeSubscriptionCmd request)
    {
        return new MoviesSubscriptionStorage
        {
            PartitionKey = request.Subscriber,
            RowKey = request.Title,
            Processed = false,
            DateToNotify = request.NotificationDate
        };
    }
}