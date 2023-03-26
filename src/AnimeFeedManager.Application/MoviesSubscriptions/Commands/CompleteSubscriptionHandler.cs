using AnimeFeedManager.Core.Utils;
using MediatR;
using Unit = LanguageExt.Unit;

namespace AnimeFeedManager.Application.MoviesSubscriptions.Commands;

public sealed record CompleteSubscriptionCmd
    (string Subscriber, string Title) : IRequest<Either<DomainError, Unit>>;

public class CompleteSubscriptionHandler : IRequestHandler<CompleteSubscriptionCmd, Either<DomainError, Unit>>
{
    private readonly IMoviesSubscriptionRepository _subscriptionRepository;

    public CompleteSubscriptionHandler(IMoviesSubscriptionRepository subscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
    }

    public Task<Either<DomainError, Unit>> Handle(CompleteSubscriptionCmd request, CancellationToken cancellationToken)
    {
        return Validate(request)
            .ToEither(nameof(CompleteSubscriptionCmd))
            .BindAsync(Persist);
    }

    private Validation<ValidationError, CompleteSubscriptionCmd> Validate(CompleteSubscriptionCmd request)
    {
        return (ValidationHelpers.EmailMustBeValid(request.Subscriber),
                ValidationHelpers.TitleMustBeValid(request.Title))
            .Apply((subscriber, title) => new CompleteSubscriptionCmd(subscriber, title));
    }

    private Task<Either<DomainError, Unit>> Persist(CompleteSubscriptionCmd request)
    {
        return _subscriptionRepository.Complete(request.Subscriber, request.Title);
    }
}