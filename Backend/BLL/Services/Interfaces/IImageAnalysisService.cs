using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using DTO;
using Microsoft.AspNetCore.Http;

namespace BLL.Services.Interfaces;

public interface IImageAnalysisService
{
    Task<ImageAnalysisResultDto> AnalyzeImageAsync(
        IFormFile image,
        CancellationToken cancellationToken = default);
}
