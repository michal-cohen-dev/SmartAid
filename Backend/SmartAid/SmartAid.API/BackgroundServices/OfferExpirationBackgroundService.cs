
using BLL.Services.Interfaces;
using DAL.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SmartAid.API.BackgroundServices
{
    public class OfferExpirationBackgroundService : BackgroundService
    {
        private const int MaxActiveOffers = 5;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OfferExpirationBackgroundService> _logger;

        public OfferExpirationBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<OfferExpirationBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(
                TimeSpan.FromSeconds(10));

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var repository = scope.ServiceProvider
                        .GetRequiredService<IRequestOfferRepository>();

                    var offerService = scope.ServiceProvider
                        .GetRequiredService<IRequestOfferService>();

                    var matchingService = scope.ServiceProvider
                        .GetRequiredService<IVolunteerMatchingService>();

                    var expirationTime = DateTime.UtcNow.AddMinutes(-2);

                    var requestIds =
                        repository.GetRequestIdsWithExpiredPendingOffers(
                            expirationTime);

                    foreach (var requestId in requestIds)
                    {
                        stoppingToken.ThrowIfCancellationRequested();

                        try
                        {
                            var eligibleVolunteers =
                                await matchingService.GetEligibleVolunteersAsync(
                                    requestId,
                                    stoppingToken);

                            if (eligibleVolunteers.Count < MaxActiveOffers)
                            {
                                _logger.LogInformation(
                                    "Keeping pending offers for help request {RequestId}. Only {EligibleCount} eligible volunteers exist. Minimum required for expiration: {MinimumCount}.",
                                    requestId,
                                    eligibleVolunteers.Count,
                                    MaxActiveOffers);

                                continue;
                            }

                            int expiredCount =
                                repository.ExpirePendingOffersForRequest(
                                    requestId,
                                    expirationTime);

                            if (expiredCount == 0)
                                continue;

                            _logger.LogInformation(
                                "Expired {ExpiredCount} offers for help request {RequestId}. Eligible volunteers: {EligibleCount}.",
                                expiredCount,
                                requestId,
                                eligibleVolunteers.Count);

                            int createdOffers =
                                await offerService.CreateOffersForRequestAsync(
                                    requestId,
                                    stoppingToken);

                            _logger.LogInformation(
                                "Created {CreatedCount} new offers for help request {RequestId} after expiration.",
                                createdOffers,
                                requestId);
                        }
                        catch (OperationCanceledException)
                            when (stoppingToken.IsCancellationRequested)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(
                                ex,
                                "Failed to process offer expiration for help request {RequestId}.",
                                requestId);
                        }
                    }
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error while processing expired request offers.");
                }
            }
        }
    }
}
