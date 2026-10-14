using System;
using System.Collections.Generic;

namespace CareerMind.Domain.Entities
{
    /// <summary>
    /// Persists the result of a user-initiated job scam risk analysis.
    /// Scores and findings are deterministic and fully explainable — no external API or ML model is used.
    /// </summary>
    public class JobRiskAnalysis : BaseEntity
    {
        // ── Who requested this ───────────────────────────────────────────────────
        public Guid CandidateProfileId { get; set; }
        public CandidateProfile CandidateProfile { get; set; } = null!;

        // ── Input (what the user submitted) ──────────────────────────────────────
        public string JobTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string JobDescription { get; set; } = string.Empty;
        public string? SalaryDetails { get; set; }
        public string? ApplicationInstructions { get; set; }
        public string? EmployerInformation { get; set; }

        // ── Results ──────────────────────────────────────────────────────────────
        /// <summary>0 (very low risk) to 100 (very high risk)</summary>
        public int RiskScore { get; set; }

        /// <summary>Low | Moderate | High | Very High</summary>
        public string RiskCategory { get; set; } = string.Empty;

        /// <summary>JSON-serialised List&lt;string&gt; of indicator keys that fired.</summary>
        public string TriggeredIndicatorsJson { get; set; } = "[]";

        /// <summary>JSON-serialised List&lt;WarningSignRecord&gt;</summary>
        public string WarningSummaryJson { get; set; } = "[]";

        /// <summary>Plain-text overall explanation shown to the user.</summary>
        public string Explanation { get; set; } = string.Empty;

        /// <summary>JSON-serialised List&lt;string&gt; — practical steps the user can take.</summary>
        public string VerificationStepsJson { get; set; } = "[]";

        /// <summary>Confidence note / data-quality caveat.</summary>
        public string ConfidenceNote { get; set; } = string.Empty;
    }
}
