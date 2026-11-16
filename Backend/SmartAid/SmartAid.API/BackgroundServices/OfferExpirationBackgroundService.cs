
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

                    var expirationTime = DateTime.UtcNow.AddMinutes(-2);

                    var expiredCount =
                        repository.ExpirePendingOffers(expirationTime);

                    if (expiredCount > 0)
                    {
                        _logger.LogInformation(
                            "Expired {Count} pending request offers.",
                            expiredCount);
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
                        "Error while expiring request offers.");
                }
            }
        }
    }
}
