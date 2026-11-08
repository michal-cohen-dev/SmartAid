
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
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
            Console.WriteLine($"TomTom API key loaded: {!string.IsNullOrWhiteSpace(_apiKey)}");
            Console.WriteLine($"TomTom API key length: {_apiKey.Length}");
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

                Options = new TomTomOptions
                {
                    DepartAt = "now",
                    Traffic = "live",
                    TravelMode = "car",
                    RouteType = "fastest"
                }
            };

            var requestUrl =
                $"routing/matrix/2?key={Uri.EscapeDataString(_apiKey)}";

            using var response = await _httpClient.PostAsJsonAsync(
                requestUrl,
                requestBody,
                JsonOptions,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"TomTom request failed. HTTP status: " +
                    $"{(int)response.StatusCode} ({response.StatusCode}).",
                    null,
                    response.StatusCode);
            }

            var matrixResponse =
                await response.Content.ReadFromJsonAsync<TomTomMatrixResponse>(
                    JsonOptions,
                    cancellationToken);

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

        private static void ValidateLocation(TomTomLocation location)
        {
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
