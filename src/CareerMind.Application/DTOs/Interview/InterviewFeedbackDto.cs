using System;

namespace CareerMind.Application.DTOs.Interview
{
    public class InterviewFeedbackDto
    {
        public double Score { get; set; }
        public string Strengths { get; set; } = string.Empty;
        public string Weaknesses { get; set; } = string.Empty;
        public string WhatWasMissing { get; set; } = string.Empty;
        public string WhatToImprove { get; set; } = string.Empty;
        public string TopicsToRevise { get; set; } = string.Empty;
        public string ExampleImprovementDirection { get; set; } = string.Empty;
    }
}
