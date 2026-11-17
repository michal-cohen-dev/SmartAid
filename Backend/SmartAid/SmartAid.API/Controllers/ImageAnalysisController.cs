
using BLL.Services.Interfaces;
using DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SmartAid.API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ImageAnalysisController : ControllerBase
{
    private readonly IImageAnalysisService _imageAnalysisService;

    public ImageAnalysisController(
        IImageAnalysisService imageAnalysisService)
    {
        _imageAnalysisService = imageAnalysisService;
    }

    [HttpPost("analyze")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ImageAnalysisResultDto>> Analyze(
        [FromForm] string? description,
IFormFile? image,
        CancellationToken cancellationToken)
    {
        bool hasDescription =
            !string.IsNullOrWhiteSpace(description);

        bool hasImage =
            image != null && image.Length > 0;

        if (!hasDescription && !hasImage)
        {
            return BadRequest(new
            {
                message = "יש להזין תיאור של הבעיה או להעלות תמונה."
            });
        }

        if (image != null && image.Length == 0)
        {
            return BadRequest(new
            {
                message = "הקובץ שהועלה ריק."
            });
        }

        if (hasDescription && description!.Length > 1000)
        {
            return BadRequest(new
            {
                message = "תיאור הבעיה לא יכול לעלות על 1000 תווים."
            });
        }

        var result = await _imageAnalysisService.AnalyzeAsync(
            description,
            image,
            cancellationToken);

        return Ok(result);
    }
}
