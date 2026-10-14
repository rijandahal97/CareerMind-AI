namespace CareerMind.Application.DTOs.JobRisk
{
    /// <summary>
    /// One risk indicator that was evaluated during analysis.
    /// Includes whether it fired and, if so, what evidence was found in the posting.
    /// </summary>
    public class JobRiskIndicatorDto
    {
        /// <summary>Machine-readable key, e.g. "upfront_fee_request".</summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>Human-readable label, e.g. "Requests Upfront Fee".</summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>Points contributed to the overall risk score (0 if did not fire).</summary>
        public int PointsAwarded { get; set; }

        /// <summary>Maximum points this indicator can contribute.</summary>
        public int MaxPoints { get; set; }

        /// <summary>Whether this indicator was triggered.</summary>
        public bool Triggered { get; set; }

        /// <summary>Short explanation of why it was (or was not) triggered.</summary>
        public string Rationale { get; set; } = string.Empty;

        /// <summary>Category grouping, e.g. "Financial", "Pressure", "Legitimacy".</summary>
        public string Category { get; set; } = string.Empty;
    }
}
