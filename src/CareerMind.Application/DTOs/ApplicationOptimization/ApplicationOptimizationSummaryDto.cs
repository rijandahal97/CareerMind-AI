using System;
using CareerMind.Domain.Enums;

namespace CareerMind.Application.DTOs.ApplicationOptimization
{
    public class ApplicationOptimizationSummaryDto
    {
        public Guid Id { get; set; }
        public Guid JobId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string EmployerName { get; set; } = string.Empty;
        public double? ApplicationReadinessScore { get; set; }
        public ApplicationStatus ApplicationStatus { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
