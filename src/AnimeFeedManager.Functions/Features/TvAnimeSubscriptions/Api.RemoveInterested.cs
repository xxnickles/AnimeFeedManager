using AnimeFeedManager.Application.TvSubscriptions.Commands;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Functions.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.TvAnimeSubscriptions;

public class RemoveInterested
{
    private readonly IMediator _mediator;
    private readonly ILogger<RemoveInterested> _logger;

    public RemoveInterested(IMediator mediator, ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<RemoveInterested>();
    }

    [Function("RemoveInterested")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "tv/removeInterested")]
        HttpRequestData req)
    {
        var dto = await Serializer.FromJson<TvSubscriptionDto>(req.Body);
        ArgumentNullException.ThrowIfNull(dto);
        
        return await req
            .WithAuthenticationCheck(new DeleteInterestedCmd(dto.UserId, dto.Series))
            .BindAsync(r => _mediator.Send(r)).ToResponse(req, _logger);
    }
}