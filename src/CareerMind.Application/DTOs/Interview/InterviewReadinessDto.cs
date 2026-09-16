using System;
using System.Collections.Generic;

namespace CareerMind.Application.DTOs.Interview
{
    public class InterviewReadinessDto
    {
        public Guid InterviewSessionId { get; set; }
        public double OverallReadinessScore { get; set; }

        public double TechnicalScore { get; set; }
        public double CommunicationScore { get; set; }
        public double RoleAlignmentScore { get; set; }
        public double CompletenessScore { get; set; }

        public List<string> Strengths { get; set; } = new List<string>();
        public List<string> Weaknesses { get; set; } = new List<string>();
        public List<string> Recommendations { get; set; } = new List<string>();
    }
}
