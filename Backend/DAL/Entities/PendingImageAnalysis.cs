using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DAL.Entities;

public class PendingImageAnalysis
{
    public Guid Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string RiskLevel { get; set; } = string.Empty;

    public bool VolunteerRequired { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime ExpiresAt { get; set; }

    public bool IsUsed { get; set; }
}
