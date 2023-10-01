﻿using AnimeFeedManager.Features.Ovas.Library;
using AnimeFeedManager.Functions.ResponseExtensions;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Ovas.Series;

public sealed class GetLibrary
{
    private readonly OvasLibraryGetter _ovasLibraryGetter;
    private readonly ILogger _logger;
    
    public GetLibrary(
        OvasLibraryGetter ovasLibraryGetter, 
        ILoggerFactory loggerFactory )
    {
        _ovasLibraryGetter = ovasLibraryGetter;
        _logger = loggerFactory.CreateLogger<GetLibrary>();
    }
    
    [Function("GetSeasonOvasLibrary")]
    public async Task<HttpResponseData> RunSeason(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ovas/{year:int}/{season}")]
        HttpRequestData req,
        string season,
        ushort year)
    {
        return await _ovasLibraryGetter.GetForSeason(season,year)
            .ToResponse(req,_logger);
    }
}