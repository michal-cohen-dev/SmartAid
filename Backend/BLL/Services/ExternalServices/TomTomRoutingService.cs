
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using BLL.Services.Interfaces;

namespace BLL.Services.ExternalServices
{
    public sealed class TomTomRoutingService : ITomTomRoutingService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        private static readonly JsonSerializerOptions JsonOptions =
            new(JsonSerializerDefaults.Web);

        public TomTomRoutingService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;

            _apiKey = configuration["TomTom:ApiKey"]
                ?? throw new InvalidOperationException(
                    "TomTom API key is missing. Configure TomTom:ApiKey.");

            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                throw new InvalidOperationException(
                    "TomTom API key cannot be empty.");
            }

            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<IReadOnlyList<TomTomRouteResult>>
            CalculateRoutesAsync(
                IReadOnlyCollection<TomTomLocation> origins,
                TomTomLocation destination,
                CancellationToken cancellationToken = default)
        {
            if (origins == null || origins.Count == 0)
            {
                throw new ArgumentException(
                    "At least one origin is required.",
                    nameof(origins));
            }

            if (origins.Count > 100)
            {
                throw new ArgumentException(
                    "The synchronous Matrix API supports up to 100 routes.",
                    nameof(origins));
            }

            ValidateLocation(destination);

            foreach (var origin in origins)
            {
                ValidateLocation(origin);
            }

            var requestBody = new TomTomMatrixRequest
            {
                Origins = origins
                    .Select(location => new TomTomPointWrapper
                    {
                        Point = new TomTomPoint
                        {
                            Latitude = location.Latitude,
                            Longitude = location.Longitude
                        }
                    })
                    .ToList(),

                Destinations = new List<TomTomPointWrapper>
                {
                    new()
                    {
                        Point = new TomTomPoint
                        {
                            Latitude = destination.Latitude,
                            Longitude = destination.Longitude
                        }
                    }
                },

                Options = CreateOptions()
            };

            return await SendMatrixRequestAsync(
                requestBody,
                cancellationToken);
        }

        public async Task<IReadOnlyList<TomTomRouteResult>>
            CalculateRoutesToMultipleDestinationsAsync(
                TomTomLocation origin,
                IReadOnlyCollection<TomTomLocation> destinations,
                CancellationToken cancellationToken = default)
        {
            if (destinations == null || destinations.Count == 0)
            {
                throw new ArgumentException(
                    "At least one destination is required.",
                    nameof(destinations));
            }

            if (destinations.Count > 100)
            {
                throw new ArgumentException(
                    "The synchronous Matrix API supports up to 100 routes.",
                    nameof(destinations));
            }

            ValidateLocation(origin);

            foreach (var destination in destinations)
            {
                ValidateLocation(destination);
            }

            var requestBody = new TomTomMatrixRequest
            {
                Origins = new List<TomTomPointWrapper>
                {
                    new()
                    {
                        Point = new TomTomPoint
                        {
                            Latitude = origin.Latitude,
                            Longitude = origin.Longitude
                        }
                    }
                },

                Destinations = destinations
                    .Select(location => new TomTomPointWrapper
                    {
                        Point = new TomTomPoint
                        {
                            Latitude = location.Latitude,
                            Longitude = location.Longitude
                        }
                    })
                    .ToList(),

                Options = CreateOptions()
            };

            return await SendMatrixRequestAsync(
                requestBody,
                cancellationToken);
        }

        private async Task<IReadOnlyList<TomTomRouteResult>>
            SendMatrixRequestAsync(
                TomTomMatrixRequest requestBody,
                CancellationToken cancellationToken)
        {
            var requestUrl =
                $"routing/matrix/2?key={Uri.EscapeDataString(_apiKey)}";

            using var response = await _httpClient.PostAsJsonAsync(
                requestUrl,
                requestBody,
                JsonOptions,
                cancellationToken);

            var responseBody = await response.Content.ReadAsStringAsync(
                cancellationToken);

            var contentType =
                response.Content.Headers.ContentType?.MediaType;

            var responsePreview = responseBody.Length > 500
                ? responseBody[..500]
                : responseBody;

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"TomTom request failed. " +
                    $"HTTP: {(int)response.StatusCode} ({response.StatusCode}). " +
                    $"Content-Type: {contentType ?? "missing"}. " +
                    $"Response preview: {responsePreview}",
                    null,
                    response.StatusCode);
            }

            if (string.IsNullOrWhiteSpace(contentType) ||
                !contentType.Contains(
                    "json",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"TomTom returned a non-JSON response. " +
                    $"HTTP: {(int)response.StatusCode} ({response.StatusCode}). " +
                    $"Content-Type: {contentType ?? "missing"}. " +
                    $"Response preview: {responsePreview}");
            }

            TomTomMatrixResponse? matrixResponse;

            try
            {
                matrixResponse =
                    JsonSerializer.Deserialize<TomTomMatrixResponse>(
                        responseBody,
                        JsonOptions);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException(
                    $"TomTom returned invalid JSON. " +
                    $"HTTP: {(int)response.StatusCode}. " +
                    $"Response preview: {responsePreview}",
                    ex);
            }

            if (matrixResponse?.Data == null)
            {
                throw new InvalidOperationException(
                    "TomTom returned an empty or invalid response.");
            }

            return matrixResponse.Data
                .Select(cell => new TomTomRouteResult
                {
                    OriginIndex = cell.OriginIndex,
                    DestinationIndex = cell.DestinationIndex,
                    DistanceInMeters = cell.RouteSummary?.LengthInMeters,
                    TravelTimeInSeconds =
                        cell.RouteSummary?.TravelTimeInSeconds,
                    TrafficDelayInSeconds =
                        cell.RouteSummary?.TrafficDelayInSeconds,
                    ErrorCode = cell.DetailedError?.Code
                })
                .ToList();
        }

        private static TomTomOptions CreateOptions()
        {
            return new TomTomOptions
            {
                DepartAt = "now",
                Traffic = "live",
                TravelMode = "car",
                RouteType = "fastest"
            };
        }

        private static void ValidateLocation(TomTomLocation location)
        {
            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            if (location.Latitude < -90 || location.Latitude > 90)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(location),
                    "Latitude must be between -90 and 90.");
            }

            if (location.Longitude < -180 || location.Longitude > 180)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(location),
                    "Longitude must be between -180 and 180.");
            }
        }

        // Request models

        private sealed class TomTomMatrixRequest
        {
            [JsonPropertyName("origins")]
            public List<TomTomPointWrapper> Origins { get; set; } = new();

            [JsonPropertyName("destinations")]
            public List<TomTomPointWrapper> Destinations { get; set; } = new();

            [JsonPropertyName("options")]
            public TomTomOptions Options { get; set; } = new();
        }

        private sealed class TomTomPointWrapper
        {
            [JsonPropertyName("point")]
            public TomTomPoint Point { get; set; } = new();
        }

        private sealed class TomTomPoint
        {
            [JsonPropertyName("latitude")]
            public double Latitude { get; set; }

            [JsonPropertyName("longitude")]
            public double Longitude { get; set; }
        }

        private sealed class TomTomOptions
        {
            [JsonPropertyName("departAt")]
            public string DepartAt { get; set; } = "now";

            [JsonPropertyName("traffic")]
            public string Traffic { get; set; } = "live";

            [JsonPropertyName("travelMode")]
            public string TravelMode { get; set; } = "car";

            [JsonPropertyName("routeType")]
            public string RouteType { get; set; } = "fastest";
        }

        // Response models

        private sealed class TomTomMatrixResponse
        {
            [JsonPropertyName("data")]
            public List<TomTomMatrixCell> Data { get; set; } = new();
        }

        private sealed class TomTomMatrixCell
        {
            [JsonPropertyName("originIndex")]
            public int OriginIndex { get; set; }

            [JsonPropertyName("destinationIndex")]
            public int DestinationIndex { get; set; }

            [JsonPropertyName("routeSummary")]
            public TomTomRouteSummary? RouteSummary { get; set; }

            [JsonPropertyName("detailedError")]
            public TomTomDetailedError? DetailedError { get; set; }
        }

        private sealed class TomTomRouteSummary
        {
            [JsonPropertyName("lengthInMeters")]
            public int LengthInMeters { get; set; }

            [JsonPropertyName("travelTimeInSeconds")]
            public int TravelTimeInSeconds { get; set; }

            [JsonPropertyName("trafficDelayInSeconds")]
            public int TrafficDelayInSeconds { get; set; }
        }

        private sealed class TomTomDetailedError
        {
            [JsonPropertyName("code")]
            public string? Code { get; set; }
        }
    }

    // Public models used by the rest of the BLL

    public sealed record TomTomLocation(
        double Latitude,
        double Longitude);

    public sealed class TomTomRouteResult
    {
        public int OriginIndex { get; set; }

        public int DestinationIndex { get; set; }

        public int? DistanceInMeters { get; set; }

        public int? TravelTimeInSeconds { get; set; }

        public int? TrafficDelayInSeconds { get; set; }

        public string? ErrorCode { get; set; }
    }
}
