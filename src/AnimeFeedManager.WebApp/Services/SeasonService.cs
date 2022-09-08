using System.Collections.Immutable;
using AnimeFeedManager.Common.Dto;

namespace AnimeFeedManager.WebApp.Services;

public interface ISeasonFetcherService
{
    Task<ImmutableList<SeasonInfoDto>> GetAvailableSeasons(CancellationToken cancellationToken = default);
}

public class SeasonService : ISeasonFetcherService
{
    private readonly HttpClient _httpClient;

    public SeasonService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ImmutableList<SeasonInfoDto>> GetAvailableSeasons(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("api/seasons", cancellationToken);
        return await response.MapToList<SeasonInfoDto>();
    }
}