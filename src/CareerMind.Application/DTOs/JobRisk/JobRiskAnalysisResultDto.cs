using System;
using System.Collections.Generic;

namespace CareerMind.Application.DTOs.JobRisk
{
    /// <summary>
    /// Full result of a job scam risk analysis.
    /// The analysis is deterministic and rule-based. It highlights risk indicators,
    /// not proven facts. The disclaimer below must be surfaced to the user.
    /// </summary>
    public class JobRiskAnalysisResultDto
    {
        public Guid AnalysisId { get; set; }

        // ── Submitted data (echoed back) ─────────────────────────────────────────
        public string JobTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public DateTime AnalyzedAt { get; set; }

        // ── Score & category ─────────────────────────────────────────────────────
        /// <summary>Composite risk score 0–100. Higher = more warning signs detected.</summary>
        public int RiskScore { get; set; }

        /// <summary>Low (0–24) | Moderate (25–49) | High (50–74) | Very High (75–100).</summary>
        public string RiskCategory { get; set; } = string.Empty;

        // ── Explainability ───────────────────────────────────────────────────────
        /// <summary>Summary paragraph explaining what was found.</summary>
        public string Explanation { get; set; } = string.Empty;

        /// <summary>All indicators evaluated (both triggered and clean).</summary>
        public List<JobRiskIndicatorDto> Indicators { get; set; } = new();

        /// <summary>Subset of indicators that were triggered — the actual warning signs.</summary>
        public List<string> WarningSigns { get; set; } = new();

        // ── Guidance ─────────────────────────────────────────────────────────────
        /// <summary>Concrete steps the job seeker should take before applying.</summary>
        public List<string> VerificationSteps { get; set; } = new();

        // ── Caveats ──────────────────────────────────────────────────────────────
        /// <summary>
        /// Data-quality note. Risk indicators are based solely on text analysis.
        /// This analysis CANNOT guarantee a job is fraudulent or legitimate.
        /// </summary>
        public string ConfidenceNote { get; set; } = string.Empty;

        /// <summary>
        /// Regulatory / liability disclaimer always included in the response.
        /// </summary>
        public string Disclaimer { get; set; } =
            "This analysis is based on configurable text-pattern indicators and does not " +
            "use any external API or machine-learning model. It cannot guarantee that the " +
            "job posting is fraudulent or legitimate. Always conduct independent verification " +
            "before sharing personal information or making any financial decisions.";
    }
}
