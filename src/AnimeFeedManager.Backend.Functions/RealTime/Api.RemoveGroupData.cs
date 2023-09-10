﻿using AnimeFeedManager.Features.Common.RealTimeNotifications;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Backend.Functions.RealTime;

public class RemoveDataOutput
{
    [SignalROutput(HubName = HubNames.Notifications, ConnectionStringSetting = "SignalRConnectionString")]
    public SignalRGroupAction? GroupOutput { get; set; }

    public HttpResponseData? HttpResponse { get; set; }
}

public class RemoveGroupData
{
    private readonly ILogger<RemoveGroupData> _logger;

    public RemoveGroupData(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<RemoveGroupData>();
    }

    [Function("RemoveGroupData")]
    public async Task<RemoveDataOutput> Add(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "notifications/remove")]
        HttpRequestData req)
    {
        var payload =
            await JsonSerializer.DeserializeAsync(req.Body, HubInfoContext.Default.HubInfo);

        ArgumentNullException.ThrowIfNull(payload);
        _logger.LogInformation("Removing {Connection} from group", payload.ConnectionId);
        var groupAction = new SignalRGroupAction(SignalRGroupActionType.Remove)
        {
            GroupName = HubGroups.AdminGroup,
            ConnectionId = payload.ConnectionId
        };

        return new RemoveDataOutput
        {
            GroupOutput = groupAction,
            HttpResponse = await req.Ok()
        };
    }
}