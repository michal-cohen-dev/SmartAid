
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

        using var memoryStream = new MemoryStream();
        await image.CopyToAsync(memoryStream, cancellationToken);

        var base64Image = Convert.ToBase64String(
            memoryStream.ToArray());

        var prompt = """
        You are the image analysis assistant for SmartAid,
        a community vehicle assistance application.

        Analyze the uploaded image and return a JSON object
        with exactly these properties:

        {
          "problemName": "Short name of the likely problem",
          "description": "Description of what can be seen",
          "category": "The selected help type name in Hebrew",
          "helpTypeId": 1,
          "riskLevel": "Low, Medium, High, or Unknown",
          "recommendation": "What the user should do",
          "safeActions": ["Safe action 1"],
          "volunteerRequired": true,
          "confidence": 0.0
        }

        Select exactly one HelpTypeId from this list:

        1 - התנעת רכב וסיוע במצבר
        2 - החלפת גלגל
        3 - פתיחת רכב נעול
        4 - חילוץ רכב תקוע
        5 - אספקת דלק
        6 - בדיקת וניפוח לחץ אוויר בצמיגים
        7 - סיוע במילוי נוזלים ברכב
        8 - תיאום גרר
        9 - תקלה מכנית ברכב
        10 - תקלה במערכת החשמל של הרכב
        11 - חשד להתחממות הרכב

        If the image does not provide enough information
        to select a category, use:
        helpTypeId 0,
        category "לא ניתן לזהות",
        confidence 0.

        IMPORTANT: UNDERSTAND THE DIFFERENCE BETWEEN
        RISK LEVEL AND PROBLEM COMPLEXITY.

        RiskLevel describes whether it is safe for an
        ordinary community volunteer to approach and assist.

        It does NOT describe how complicated, expensive,
        unusual or difficult the vehicle problem is.

        RiskLevel rules:

        1. High:
        Use High ONLY when there are clear indications
        of immediate danger to the user or volunteer.

        Examples include:
        - Visible fire or flames.
        - Significant smoke indicating a possible hazard.
        - Visible fuel leakage or spill.
        - Exposed electrical wires or visible sparking.
        - A vehicle in a dangerous traffic situation.
        - A visibly unstable vehicle or another clear
          immediate safety hazard.

        In these cases, volunteer assistance must not
        be recommended.

        2. Medium:
        Use Medium when the problem requires assistance
        or caution, but there is no clear immediate danger
        preventing a community volunteer from safely helping.

        A technically complicated problem does NOT
        automatically mean High risk.

        If a volunteer can safely provide reasonable
        assistance, use Medium rather than High,
        even if the problem is complicated.

        3. Low:
        Use Low for simple situations where assistance
        can reasonably be provided safely.

        4. Unknown:
        Use Unknown when the image is unclear or there
        is insufficient information to assess safety.

        Do not guess the risk level.

        VolunteerRequired rules:

        - Set volunteerRequired to false if riskLevel
          is High or Unknown.
        - For Low or Medium, set volunteerRequired to true
          only if a community volunteer can reasonably
          provide safe assistance.
        - Set volunteerRequired to false if the situation
          requires a qualified professional instead.
        - Do not confuse a need for professional expertise
          with immediate danger. A complicated problem
          is not automatically High risk.

        Additional safety rules:

        - Do not claim certainty when the image is unclear.
        - Confidence must be a number between 0 and 1.
        - Never recommend DIY actions involving electricity,
          fuel systems, chemicals, machinery or other hazards.
        - For potentially dangerous situations, advise the
          user to stay away and seek qualified professional
          assistance.
        - Do not invent details that are not visible.
        - Do not assume that an image proves a situation
          is safe.
        - If there is insufficient visual information,
          use Unknown rather than guessing.
        - The response must be in Hebrew, except for the
          riskLevel values, which must be Low, Medium,
          High, or Unknown.
        - Return JSON only, without Markdown or explanations
          outside the JSON object.
        """;

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new object[]
                    {
                        new { text = prompt },
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

        var result = JsonSerializer.Deserialize<ImageAnalysisResultDto>(
            modelText,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (result == null)
            throw new InvalidOperationException(
                "Could not parse Gemini response.");

        // Validate the selected help type.
        if (result.HelpTypeId < 1 || result.HelpTypeId > 11)
        {
            result.HelpTypeId = null;
            result.Category = "לא ניתן לזהות";
            result.Confidence = 0;
        }

        // Validate the risk level.
        var validRiskLevels = new[]
        {
            "Low",
            "Medium",
            "High",
            "Unknown"
        };

        if (string.IsNullOrWhiteSpace(result.RiskLevel) ||
            !validRiskLevels.Contains(
                result.RiskLevel,
                StringComparer.OrdinalIgnoreCase))
        {
            result.RiskLevel = "Unknown";
        }

        // Normalize the risk level.
        result.RiskLevel = validRiskLevels.First(
            risk => string.Equals(
                risk,
                result.RiskLevel,
                StringComparison.OrdinalIgnoreCase));

        // High or Unknown must never allow volunteer assistance.
        if (result.RiskLevel == "High" ||
            result.RiskLevel == "Unknown")
        {
            result.VolunteerRequired = false;
        }

        // Validate confidence.
        if (result.Confidence < 0 || result.Confidence > 1)
        {
            result.Confidence = 0;
        }

        return result;
    }
}
