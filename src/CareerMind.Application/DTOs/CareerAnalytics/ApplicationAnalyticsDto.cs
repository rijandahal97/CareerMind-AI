using System.Collections.Generic;

namespace CareerMind.Application.DTOs.CareerAnalytics
{
    public class ApplicationAnalyticsDto
    {
        public int TotalOptimizedApplications { get; set; }
        public decimal AverageApplicationReadiness { get; set; }
        public int HighReadinessApplications { get; set; }
        public int ApplicationsNeedingImprovement { get; set; }
        
        public int CompletedSuggestions { get; set; }
        public int PendingSuggestions { get; set; }
        
        public List<string> CommonSuggestionCategories { get; set; } = new List<string>();
        public List<string> MainBlockers { get; set; } = new List<string>();
    }
}
