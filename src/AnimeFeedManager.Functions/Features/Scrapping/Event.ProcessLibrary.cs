using System.Collections.Immutable;
using AnimeFeedManager.Application.AnimeLibrary.Queries;
using AnimeFeedManager.Application.Feed.Commands;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Common.Helpers;
using AnimeFeedManager.Common.Notifications;
using AnimeFeedManager.Functions.Models;
using AnimeFeedManager.Services.Collectors.Interface;
using AnimeFeedManager.Storage.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Scrapping;

public class ProcessLibraryOutput
{
    [QueueOutput(QueueNames.TitleProcess)] public string? TitleMessage { get; set; }

    [QueueOutput(QueueNames.AnimeLibrary)] public IEnumerable<string>? AnimeMessages { get; set; }

    [QueueOutput(QueueNames.ImageProcess)] public IEnumerable<string>? ImagesMessages { get; set; }

    [QueueOutput(QueueNames.AvailableSeasons)]
    public string? SeasonMessage { get; set; }
}

public class ProcessLibrary
{
    private readonly IDomainPostman _domainPostman;
    private readonly IFeedProvider _feedProvider;
    private readonly IMediator _mediator;
    private readonly ILogger<ProcessLibrary> _logger;

    public ProcessLibrary(
        IDomainPostman domainPostman,
        IFeedProvider feedProvider,
        IMediator mediator,
        ILoggerFactory loggerFactory)
    {
        _domainPostman = domainPostman;
        _feedProvider = feedProvider;
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<ProcessLibrary>();
    }

    [Function("ProcessLibrary")]
    public async Task<ProcessLibraryOutput> Run(
        [QueueTrigger(QueueNames.LibraryUpdate, Connection = "AzureWebJobsStorage")]
        LibraryUpdate startProcess)
    {

        return startProcess.Type switch
        {
            LibraryUpdateType.Full => await ProcessFullLibrary(),
            LibraryUpdateType.Titles => await ProcessTitles(),
            _ => new ProcessLibraryOutput()
        };
    }


    private async Task<ProcessLibraryOutput> ProcessTitles()
    {
        _logger.LogInformation("Processing update of the feed titles only");
        var titleStoreResult = await _feedProvider.GetTitles()
            .BindAsync(feedTitles => _mediator.Send(new AddTitlesCmd(feedTitles)));

        return titleStoreResult.Match(_ =>
        {
            _domainPostman.SendMessage(new TitlesUpdateNotification(
                IdHelpers.GetUniqueId(),
                TargetAudience.Admins,
                NotificationType.Information,
                $"Latest feed titles have been updated"));

            return new ProcessLibraryOutput
            {
                TitleMessage = ProcessResult.Ok,
            };
        }, e =>
        {
            _logger.LogError("An error occurred while processing feed titles update {S}", e.ToString());
            _domainPostman.SendMessage(new TitlesUpdateNotification(
                IdHelpers.GetUniqueId(),
                TargetAudience.Admins,
                NotificationType.Error,
                $"An error occurred before storing feed titles."));

            return new ProcessLibraryOutput
            {
                AnimeMessages = null,
                ImagesMessages = null,
                TitleMessage = ProcessResult.Failure,
                SeasonMessage = null
            };
        });
    }

    private async Task<ProcessLibraryOutput> ProcessFullLibrary()
    {
        _logger.LogInformation("Processing update of the full library");

        var result = await _feedProvider.GetTitles()
            .BindAsync(CollectLibrary);


        return result.Match(
            v =>
            {
                _logger.LogInformation("Titles have been updated and Series information has been collected");

                _domainPostman.SendMessage(new SeasonProcessNotification(
                    IdHelpers.GetUniqueId(),
                    TargetAudience.Admins,
                    NotificationType.Information,
                    v.Season,
                    $"{v.Animes.Count} series of {v.Season.Season}-{v.Season.Year} will be stored"));


                _domainPostman.SendDelayedMessage(new SeasonProcessNotification(
                        IdHelpers.GetUniqueId(),
                        TargetAudience.All,
                        NotificationType.Update,
                        v.Season,
                        $"Season information for {v.Season.Season}-{v.Season.Year} has been updated recently"),
                    new MinutesDelay(1));

                return new ProcessLibraryOutput
                {
                    AnimeMessages = v.Animes.Select(Serializer.ToJson),
                    ImagesMessages = v.Images.Select(Serializer.ToJson),
                    SeasonMessage = Serializer.ToJson(v.Season),
                    TitleMessage = ProcessResult.Ok,
                };
            },
            e =>
            {
                _logger.LogError("An error occurred while processing library update {S}", e.ToString());
                _domainPostman.SendMessage(new SeasonProcessNotification(
                    IdHelpers.GetUniqueId(),
                    TargetAudience.Admins,
                    NotificationType.Error,
                    new NullSeasonInfo(),
                    $"An error occurred before storing series."));

                return new ProcessLibraryOutput
                {
                    AnimeMessages = null,
                    ImagesMessages = null,
                    TitleMessage = ProcessResult.Failure,
                    SeasonMessage = null
                };
            });
    }

    private Task<Either<DomainError, LibraryForStorage>> CollectLibrary(ImmutableList<string> feedTitles)
    {
        return _mediator.Send(new AddTitlesCmd(feedTitles))
            .BindAsync(_ => _mediator.Send(new GetLibraryQry(feedTitles)));
    }
}