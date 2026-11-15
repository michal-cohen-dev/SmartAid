
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
    IFormFile image,
    CancellationToken cancellationToken)
    {
        if (image == null || image.Length == 0)
        {
            return BadRequest(new
            {
                message = "Please upload an image."
            });
        }

        var result = await _imageAnalysisService.AnalyzeImageAsync(
            image,
            cancellationToken);

        return Ok(result);
    }
}
