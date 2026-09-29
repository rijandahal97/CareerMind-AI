using System.Collections.Generic;

namespace CareerMind.Application.DTOs.CareerAnalytics
{
    public class JobMatchingAnalyticsDto
    {
        public int TotalJobsEvaluated { get; set; }
        public decimal AverageMatchScore { get; set; }
        public decimal HighestMatchScore { get; set; }
        public int StrongMatches { get; set; }
        public int ModerateMatches { get; set; }
        public int WeakMatches { get; set; }
        public List<string> FrequentlyMissingSkills { get; set; } = new List<string>();
    }
}
