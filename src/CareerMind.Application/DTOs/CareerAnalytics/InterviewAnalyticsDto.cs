using System.Collections.Generic;

namespace CareerMind.Application.DTOs.CareerAnalytics
{
    public class InterviewAnalyticsDto
    {
        public int TotalInterviewSessions { get; set; }
        public int CompletedSessions { get; set; }
        public decimal AverageReadinessScore { get; set; }
        public decimal LatestReadinessScore { get; set; }
        public decimal? TechnicalPerformance { get; set; }
        public decimal? BehavioralPerformance { get; set; }
        
        public List<string> CommonWeaknesses { get; set; } = new List<string>();
        public List<string> RecommendedImprovementAreas { get; set; } = new List<string>();
    }
}
