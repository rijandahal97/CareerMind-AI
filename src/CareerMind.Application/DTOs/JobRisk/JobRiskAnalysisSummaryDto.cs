using System;
using System.Collections.Generic;

namespace CareerMind.Application.DTOs.JobRisk
{
    /// <summary>Summary row used when listing a user's previous analyses.</summary>
    public class JobRiskAnalysisSummaryDto
    {
        public Guid AnalysisId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public int RiskScore { get; set; }
        public string RiskCategory { get; set; } = string.Empty;
        public int WarningSigCount { get; set; }
        public DateTime AnalyzedAt { get; set; }
    }
}
