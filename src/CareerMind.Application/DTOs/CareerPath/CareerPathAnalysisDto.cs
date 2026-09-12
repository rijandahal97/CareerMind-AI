using System.Collections.Generic;

namespace CareerMind.Application.DTOs.CareerPath
{
    public class CareerPathAnalysisDto
    {
        public decimal OverallCareerReadiness { get; set; }
        public List<CareerRecommendationDto> RecommendedCareers { get; set; } = new();
        public CareerRecommendationDto? BestCareer { get; set; }
        
        public List<CareerPathStageDto> CareerPath { get; set; } = new();
        public NextCareerMoveDto? NextCareerMove { get; set; }
        
        public List<string> ExplainableInsights { get; set; } = new();
    }
}
