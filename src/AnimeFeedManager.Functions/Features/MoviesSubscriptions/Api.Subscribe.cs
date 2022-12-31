using AnimeFeedManager.Application.MoviesSubscriptions.Commands;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Functions.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.MoviesSubscriptions;

public class Subscribe
{
    private readonly IMediator _mediator;
    private readonly ILogger<Subscribe> _logger;

    public Subscribe(IMediator mediator, ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<Subscribe>();
    }

    [Function("MoviesSubscription")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", "put", Route = "Movies/subscriptions")]
        HttpRequestData req)
    {
        var dto = await Serializer.FromJson<ShortSeriesSubscriptionDto>(req.Body);
        ArgumentNullException.ThrowIfNull(dto);
        return await req
            .WithAuthenticationCheck(new MergeSubscriptionCmd(dto.UserId, dto.Series, dto.NotificationDate))
            .BindAsync(command => _mediator.Send(command))
            .ToResponse(req, _logger);
    }
}