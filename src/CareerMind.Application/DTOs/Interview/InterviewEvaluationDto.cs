using System;

namespace CareerMind.Application.DTOs.Interview
{
    public class InterviewEvaluationDto
    {
        public double Score { get; set; }
        public double RelevanceScore { get; set; }
        public double TechnicalAccuracyScore { get; set; }
        public double CommunicationScore { get; set; }
        public double CompletenessScore { get; set; }
        public double RoleAlignmentScore { get; set; }

        public string Feedback { get; set; } = string.Empty;
        public string Strengths { get; set; } = string.Empty;
        public string Weaknesses { get; set; } = string.Empty;
        public string ImprovementSuggestion { get; set; } = string.Empty;
    }
}
