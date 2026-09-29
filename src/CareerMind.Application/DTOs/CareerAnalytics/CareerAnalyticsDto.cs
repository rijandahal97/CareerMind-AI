using System.Collections.Generic;

namespace CareerMind.Application.DTOs.CareerAnalytics
{
    public class CareerAnalyticsDto
    {
        public decimal OverallProgressScore { get; set; }
        
        public ProfileAnalyticsDto ProfileAnalytics { get; set; }
        public SkillAnalyticsDto SkillAnalytics { get; set; }
        public JobMatchingAnalyticsDto JobMatchingAnalytics { get; set; }
        public ResumeAnalyticsDto ResumeAnalytics { get; set; }
        public InterviewAnalyticsDto InterviewAnalytics { get; set; }
        public ApplicationAnalyticsDto ApplicationAnalytics { get; set; }
        public ActionPlanAnalyticsDto ActionPlanAnalytics { get; set; }

        public List<CareerInsightDto> Insights { get; set; } = new List<CareerInsightDto>();
    }
}
