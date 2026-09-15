using System.Collections.Generic;
using CareerMind.Application.DTOs.CareerGap;
using CareerMind.Application.DTOs.Job;

namespace CareerMind.Application.DTOs.CareerCommandCenter
{
    public class CareerCommandCenterDto
    {
        public CareerReadinessDto Readiness { get; set; } = new CareerReadinessDto();
        public CareerActionDto NextBestAction { get; set; } = new CareerActionDto();
        public List<CareerBlockerDto> TopBlockers { get; set; } = new List<CareerBlockerDto>();
        public CareerActionPlanDto ActionPlan { get; set; } = new CareerActionPlanDto();
        public CareerGoalSummaryDto GoalSummary { get; set; } = new CareerGoalSummaryDto();
        public List<SkillRoadmapItemDto> TopSkillsToDevelop { get; set; } = new List<SkillRoadmapItemDto>();
        public List<JobMatchResultDto> RecommendedJobs { get; set; } = new List<JobMatchResultDto>();
        public ResumeHealthSummaryDto ResumeHealth { get; set; } = new ResumeHealthSummaryDto();
    }
}
