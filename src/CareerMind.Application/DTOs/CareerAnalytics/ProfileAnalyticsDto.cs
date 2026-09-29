using System.Collections.Generic;

namespace CareerMind.Application.DTOs.CareerAnalytics
{
    public class ProfileAnalyticsDto
    {
        public decimal CompletionPercentage { get; set; }
        public List<string> CompletedSections { get; set; } = new List<string>();
        public List<string> MissingSections { get; set; } = new List<string>();
        public int TotalProfileSections { get; set; }
        public string CompletionStatus { get; set; } = string.Empty;
    }
}
