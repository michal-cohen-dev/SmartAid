
using BLL.Services.Interfaces;
using DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace BLL.Services;

public class ImageAnalysisService : IImageAnalysisService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public ImageAnalysisService(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Gemini:ApiKey"]
            ?? throw new InvalidOperationException(
                "Gemini API key was not found.");
    }

    public async Task<ImageAnalysisResultDto> AnalyzeImageAsync(
        IFormFile image,
        CancellationToken cancellationToken = default)
    {
        if (image == null || image.Length == 0)
            throw new ArgumentException("No image was provided.");

        if (image.Length > 5 * 1024 * 1024)
            throw new ArgumentException("Image size exceeds 5MB.");

        var allowedTypes = new[]
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

        if (!allowedTypes.Contains(image.ContentType.ToLowerInvariant()))
            throw new ArgumentException("Unsupported image type.");

        // Convert the image to Base64
        using var memoryStream = new MemoryStream();
        await image.CopyToAsync(memoryStream, cancellationToken);

        var base64Image = Convert.ToBase64String(
            memoryStream.ToArray());

        // Instructions for Gemini
        var prompt = """
        You are the image analysis assistant for SmartAid,
        a community help application.

        Analyze the uploaded image and return a JSON object
        with exactly these properties:

        {
          "problemName": "Short name of the likely problem",
          "description": "Description of what can be seen",
          "category": "Problem category",
          "riskLevel": "Low, Medium, High, or Unknown",
          "recommendation": "What the user should do",
          "safeActions": ["Safe action 1", "Safe action 2"],
          "volunteerRequired": true,
          "confidence": 0.0
        }

        Important safety rules:
        - Do not claim certainty when the image is unclear.
        - Confidence must be a number between 0 and 1.
        - If you cannot identify the problem, say so clearly.
        - Never recommend DIY actions involving electricity,
          gas, structural damage, chemicals, machinery or heights.
        - For potentially dangerous situations, tell the user
          to stay away and seek qualified help.
        - Only suggest simple, low-risk actions when appropriate.
        - Do not invent details that are not visible.
        - The response must be in Hebrew.
        - Return JSON only, without Markdown or explanations
          outside the JSON object.
        """;

        // Build the request for Gemini
        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new object[]
                    {
                        new
                        {
                            text = prompt
                        },
                        new
                        {
                            inline_data = new
                            {
                                mime_type = image.ContentType,
                                data = base64Image
                            }
                        }
                    }
                }
            },
            generationConfig = new
            {
                responseMimeType = "application/json",
                temperature = 0.2
            }
        };

        var jsonRequest = JsonSerializer.Serialize(requestBody);

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
           "https://generativelanguage.googleapis.com/v1beta/models/gemini-3.5-flash-lite:generateContent");
        request.Headers.Add("x-goog-api-key", _apiKey);

        request.Content = new StringContent(
            jsonRequest,
            Encoding.UTF8,
            "application/json");

        // Send the request to Gemini
        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        var responseContent = await response.Content.ReadAsStringAsync(
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Gemini API error: {(int)response.StatusCode}");
        }

        // Extract Gemini's answer
        using var jsonDocument = JsonDocument.Parse(responseContent);

        var modelText = jsonDocument.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        if (string.IsNullOrWhiteSpace(modelText))
            throw new InvalidOperationException(
                "Gemini returned an empty response.");

        // Convert the JSON answer to our DTO
        var result = JsonSerializer.Deserialize<ImageAnalysisResultDto>(
            modelText,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return result ?? throw new InvalidOperationException(
            "Could not parse Gemini response.");
    }
}
