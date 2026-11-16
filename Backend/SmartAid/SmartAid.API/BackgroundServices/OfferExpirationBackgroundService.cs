
using BLL.Services.Interfaces;
using DAL.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SmartAid.API.BackgroundServices
{
    public class OfferExpirationBackgroundService : BackgroundService
    {
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

                    var expirationTime = DateTime.UtcNow.AddMinutes(-2);

                    var expiredRequestIds =
                        repository.ExpirePendingOffers(expirationTime);

                    if (expiredRequestIds.Count == 0)
                        continue;

                    _logger.LogInformation(
                        "Expired offers found for {Count} help requests.",
                        expiredRequestIds.Count);

                    foreach (var requestId in expiredRequestIds)
                    {
                        stoppingToken.ThrowIfCancellationRequested();

                        try
                        {
                            int createdOffers =
                                await offerService.CreateOffersForRequestAsync(
                                    requestId,
                                    stoppingToken);

                            _logger.LogInformation(
                                "Created {Count} new offers for help request {RequestId} after expiration.",
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
                                "Failed to create the next offers for help request {RequestId}.",
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
