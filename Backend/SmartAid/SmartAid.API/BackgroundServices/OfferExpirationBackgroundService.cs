
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

                            // כל המתנדבים שכבר קיבלו הצעה לקריאה הזאת,
                            // ללא קשר לסטטוס ההצעה.
                            var alreadyOfferedVolunteerIds =
                                repository.GetOfferedVolunteerIdsForRequest(
                                    requestId)
                                .ToHashSet();

                            // סופרים רק מתנדבים מתאימים שמעולם לא קיבלו
                            // הצעה עבור הקריאה הזאת.
                            int alternativeVolunteersCount =
                                eligibleVolunteers.Count(match =>
                                    !alreadyOfferedVolunteerIds.Contains(
                                        match.Volunteer.Id));

                            // אם אין לפחות 5 מתנדבים חדשים,
                            // משאירים את ההצעות הקיימות פעילות.
                            if (alternativeVolunteersCount < MaxActiveOffers)
                            {
                                _logger.LogInformation(
                                    "Keeping pending offers for request {RequestId}. " +
                                    "Only {AlternativeCount} new eligible volunteers are available.",
                                    requestId,
                                    alternativeVolunteersCount);

                                continue;
                            }

                            // יש מספיק מתנדבים חדשים:
                            // מפוגגים את ההצעות הישנות.
                            int expiredCount =
                                repository.ExpirePendingOffersForRequest(
                                    requestId,
                                    expirationTime);

                            if (expiredCount == 0)
                                continue;

                            _logger.LogInformation(
                                "Expired {ExpiredCount} old offers for request {RequestId}. " +
                                "{AlternativeCount} new eligible volunteers are available.",
                                expiredCount,
                                requestId,
                                alternativeVolunteersCount);

                            // שולחים הצעות רק למתנדבים שמעולם לא קיבלו
                            // הצעה עבור הקריאה הזאת.
                            int createdOffers =
                                await offerService.CreateOffersForRequestAsync(
                                    requestId,
                                    stoppingToken,
                                    onlyNeverOffered: true);

                            _logger.LogInformation(
                                "Created {CreatedCount} new offers for request {RequestId}.",
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
                                "Failed to process offer expiration for request {RequestId}.",
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
