using System;

namespace CareerMind.Application.DTOs.Job
{
    public class SavedJobDto
    {
        public Guid JobId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? EmploymentType { get; set; }
        public DateTime SavedAt { get; set; }
    }
}
