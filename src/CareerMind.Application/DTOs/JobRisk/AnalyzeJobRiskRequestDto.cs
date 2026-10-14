using System.ComponentModel.DataAnnotations;

namespace CareerMind.Application.DTOs.JobRisk
{
    /// <summary>Input DTO for a job scam risk analysis request.</summary>
    public class AnalyzeJobRiskRequestDto
    {
        [Required]
        [MaxLength(200)]
        public string JobTitle { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [MaxLength(8000)]
        public string JobDescription { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? SalaryDetails { get; set; }

        [MaxLength(2000)]
        public string? ApplicationInstructions { get; set; }

        [MaxLength(1000)]
        public string? EmployerInformation { get; set; }
    }
}
