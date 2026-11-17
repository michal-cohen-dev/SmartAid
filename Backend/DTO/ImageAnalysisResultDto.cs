
using System.Collections.Generic;

namespace DTO;

public class ImageAnalysisResultDto
{
    public string ProblemName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public int? HelpTypeId { get; set; }

    public string RiskLevel { get; set; } = string.Empty;

    public string Recommendation { get; set; } = string.Empty;

    public List<string> SafeActions { get; set; } = new();

    public bool VolunteerRequired { get; set; }

    public double Confidence { get; set; }

    public List<int> SuggestedEquipmentTypeIds { get; set; } = new();
}
