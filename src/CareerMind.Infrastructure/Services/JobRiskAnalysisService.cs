using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CareerMind.Application.DTOs.JobRisk;
using CareerMind.Application.Interfaces;
using CareerMind.Domain.Entities;
using CareerMind.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareerMind.Infrastructure.Services
{
    /// <summary>
    /// Deterministic, rule-based job scam risk scoring engine.
    ///
    /// ALGORITHM OVERVIEW
    /// ==================
    /// Each <see cref="RiskIndicator"/> carries a configurable point weight (MaxPoints).
    /// The engine evaluates every indicator against the submitted text fields.
    /// Triggered indicators contribute their full MaxPoints to the raw score. The raw
    /// score is then normalised to a 0-100 scale.
    ///
    /// IMPORTANT LIMITATIONS
    /// =====================
    /// • Text-pattern matching can produce false positives (e.g. a real government
    ///   agency asking for ID verification may superficially resemble a scam pattern).
    /// • This engine does NOT use any external API, machine learning, or web search.
    /// • A high risk score means more warning signs were detected, NOT that the posting
    ///   is confirmed fraudulent. A low score does NOT certify legitimacy.
    /// </summary>
    public class JobRiskAnalysisService : IJobRiskAnalysisService
    {
        private readonly CareerMindDbContext _context;

        // ── Configurable indicator definitions ───────────────────────────────────
        // MaxPoints values reflect relative severity. They are intentionally spread
        // across the 0-100 scale so that no single indicator alone pushes the score
        // to "Very High".
        private static readonly List<RiskIndicator> Indicators = new()
        {
            // --- Financial red flags (high weight) --------------------------------
            new("upfront_fee",            "Requests Upfront Fee or Payment",            25, "Financial",
                new[] { "registration fee", "training fee", "processing fee", "pay to apply",
                        "payment required", "starter kit cost", "equipment fee", "uniform fee",
                        "background check fee", "pay upfront", "wire transfer", "send money" }),

            new("unusual_payment",         "Unusual Payment Method Requested",           15, "Financial",
                new[] { "western union", "moneygram", "gift card", "bitcoin", "cryptocurrency",
                        "zelle", "cashapp", "venmo me", "money order", "wire funds" }),

            new("overpayment_scheme",      "Potential Overpayment / Check Scam",         20, "Financial",
                new[] { "overpayment", "excess check", "return the difference",
                        "send back", "cashier check", "cheque" }),

            // --- Unrealistic promises (medium weight) -----------------------------
            new("unrealistic_salary",      "Unrealistic Salary Promise",                 15, "Promises",
                new[] { "earn $10000 per week", "earn $5000 per week", "make $500 per day",
                        "unlimited income", "six figures for", "guaranteed income", "$1000 per day",
                        "easy money", "no experience needed but $" }),

            new("guaranteed_job",          "Guaranteed Hiring Without Assessment",       10, "Promises",
                new[] { "guaranteed job", "guaranteed hire", "everyone gets hired",
                        "no interview needed", "instant offer", "immediate hire no questions" }),

            // --- Pressure tactics (medium weight) ---------------------------------
            new("urgency_pressure",        "Artificial Urgency / Pressure Tactics",      10, "Pressure",
                new[] { "act now", "24 hours only", "respond immediately",
                        "limited spots", "first come first serve",
                        "offer expires today", "must reply within", "last chance" }),

            new("personal_info_early",     "Requests Sensitive Personal Info Early",     15, "Privacy",
                new[] { "social security number", "ssn", "passport number", "bank account number",
                        "routing number", "credit card number", "date of birth before interview",
                        "drivers license number" }),

            // --- Missing employer information (low-medium weight) ------------------
            new("no_company_address",      "No Company Address or Contact Details",       5, "Legitimacy",
                new[] { string.Empty }), // evaluated by custom logic, not keyword list

            new("vague_description",       "Vague or Generic Job Description",            5, "Legitimacy",
                new[] { string.Empty }), // custom logic

            // --- Suspicious application instructions (medium weight) ---------------
            new("suspicious_apply_method", "Suspicious Application Method",              10, "Application",
                new[] { "whatsapp me", "text only", "do not call hr",
                        "personal email only", "yahoo.com", "gmail.com recruiter",
                        "hotmail.com recruiter", "apply via telegram", "apply via whatsapp" }),

            new("work_from_home_scam",    "Vague 'Work from Home' with No Details",      5, "Application",
                new[] { "work from home unlimited", "make money from home no experience",
                        "stay at home earn", "anyone can do it from home" }),
        };

        public JobRiskAnalysisService(CareerMindDbContext context)
        {
            _context = context;
        }

        // ── Public interface ──────────────────────────────────────────────────────

        public async Task<JobRiskAnalysisResultDto> AnalyzeJobRiskAsync(
            Guid candidateUserId,
            AnalyzeJobRiskRequestDto request)
        {
            var candidate = await RequireCandidateAsync(candidateUserId);

            // Build the composite text corpus to search
            var corpus = BuildCorpus(request);

            // Evaluate every indicator
            var evaluated = EvaluateIndicators(request, corpus);

            // Compute score (0-100)
            int maxPossible = Indicators.Sum(i => i.MaxPoints);
            int rawScore    = evaluated.Sum(i => i.PointsAwarded);
            int score       = maxPossible > 0
                ? (int)Math.Round(rawScore * 100.0 / maxPossible)
                : 0;
            score = Math.Clamp(score, 0, 100);

            string category = ClassifyRisk(score);
            var    warningSigns = evaluated.Where(i => i.Triggered).Select(i => i.Label).ToList();
            var    steps        = BuildVerificationSteps(evaluated, request);
            string explanation  = BuildExplanation(score, category, evaluated, request);
            string confidence   = BuildConfidenceNote(request);

            // Persist
            var entity = new JobRiskAnalysis
            {
                CandidateProfileId      = candidate.Id,
                JobTitle                = request.JobTitle,
                CompanyName             = request.CompanyName,
                JobDescription          = request.JobDescription,
                SalaryDetails           = request.SalaryDetails,
                ApplicationInstructions = request.ApplicationInstructions,
                EmployerInformation     = request.EmployerInformation,
                RiskScore               = score,
                RiskCategory            = category,
                TriggeredIndicatorsJson = JsonSerializer.Serialize(evaluated.Where(i => i.Triggered).Select(i => i.Key).ToList()),
                WarningSummaryJson      = JsonSerializer.Serialize(warningSigns),
                Explanation             = explanation,
                VerificationStepsJson   = JsonSerializer.Serialize(steps),
                ConfidenceNote          = confidence
            };

            _context.JobRiskAnalyses.Add(entity);
            await _context.SaveChangesAsync();

            return ToResultDto(entity, evaluated);
        }

        public async Task<List<JobRiskAnalysisSummaryDto>> GetRiskAnalysisHistoryAsync(Guid candidateUserId)
        {
            var candidate = await RequireCandidateAsync(candidateUserId);

            return await _context.JobRiskAnalyses
                .Where(r => r.CandidateProfileId == candidate.Id && !r.IsDeleted)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new JobRiskAnalysisSummaryDto
                {
                    AnalysisId    = r.Id,
                    JobTitle      = r.JobTitle,
                    CompanyName   = r.CompanyName,
                    RiskScore     = r.RiskScore,
                    RiskCategory  = r.RiskCategory,
                    WarningSigCount = r.WarningSummaryJson == "[]" ? 0
                        : JsonSerializer.Deserialize<List<string>>(r.WarningSummaryJson)!.Count,
                    AnalyzedAt    = r.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<JobRiskAnalysisResultDto> GetRiskAnalysisAsync(Guid candidateUserId, Guid analysisId)
        {
            var candidate = await RequireCandidateAsync(candidateUserId);

            var entity = await _context.JobRiskAnalyses
                .FirstOrDefaultAsync(r => r.Id == analysisId && !r.IsDeleted);

            if (entity == null)
                throw new KeyNotFoundException("Risk analysis not found.");

            if (entity.CandidateProfileId != candidate.Id)
                throw new UnauthorizedAccessException("You do not have access to this analysis.");

            // Reconstruct indicator DTOs from stored data (no re-scoring needed)
            var triggeredKeys = JsonSerializer.Deserialize<List<string>>(entity.TriggeredIndicatorsJson)
                ?? new List<string>();

            var indicators = Indicators.Select(ind => new JobRiskIndicatorDto
            {
                Key          = ind.Key,
                Label        = ind.Label,
                MaxPoints    = ind.MaxPoints,
                PointsAwarded = triggeredKeys.Contains(ind.Key) ? ind.MaxPoints : 0,
                Triggered    = triggeredKeys.Contains(ind.Key),
                Rationale    = triggeredKeys.Contains(ind.Key)
                    ? $"Pattern(s) matching '{ind.Label}' were detected in the job posting text."
                    : "No matching patterns detected.",
                Category     = ind.Category
            }).ToList();

            return ToResultDto(entity, indicators);
        }

        // ── Scoring helpers ───────────────────────────────────────────────────────

        private List<JobRiskIndicatorDto> EvaluateIndicators(
            AnalyzeJobRiskRequestDto request, string corpus)
        {
            var results = new List<JobRiskIndicatorDto>();

            foreach (var indicator in Indicators)
            {
                bool triggered;
                string rationale;

                switch (indicator.Key)
                {
                    // Custom logic indicators (keyword list is irrelevant)
                    case "no_company_address":
                        triggered = string.IsNullOrWhiteSpace(request.EmployerInformation)
                            && !ContainsAddressPattern(corpus);
                        rationale = triggered
                            ? "No employer information or contact address was provided."
                            : "Employer information or address details appear present.";
                        break;

                    case "vague_description":
                        triggered = request.JobDescription.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length < 60;
                        rationale = triggered
                            ? "Job description is very short (fewer than 60 words), which may indicate a vague or incomplete listing."
                            : "Job description appears sufficiently detailed.";
                        break;

                    default:
                        triggered = MatchesKeywords(corpus, indicator.Keywords);
                        var matched = MatchedKeyword(corpus, indicator.Keywords);
                        rationale = triggered
                            ? $"Detected phrase(s) similar to \"{matched}\" in the submission."
                            : "No matching patterns detected for this indicator.";
                        break;
                }

                results.Add(new JobRiskIndicatorDto
                {
                    Key           = indicator.Key,
                    Label         = indicator.Label,
                    MaxPoints     = indicator.MaxPoints,
                    PointsAwarded = triggered ? indicator.MaxPoints : 0,
                    Triggered     = triggered,
                    Rationale     = rationale,
                    Category      = indicator.Category
                });
            }

            return results;
        }

        private static bool MatchesKeywords(string corpus, string[] keywords)
            => keywords.Any(k => !string.IsNullOrEmpty(k)
                && corpus.Contains(k, StringComparison.OrdinalIgnoreCase));

        private static string MatchedKeyword(string corpus, string[] keywords)
            => keywords.FirstOrDefault(k => !string.IsNullOrEmpty(k)
                && corpus.Contains(k, StringComparison.OrdinalIgnoreCase)) ?? string.Empty;

        private static bool ContainsAddressPattern(string corpus)
        {
            // Heuristic: presence of common full address words.
            // Short abbreviations like "st." and "ave." are intentionally excluded
            // because they match too many non-address words (e.g. "cost.", "stave.").
            var addressWords = new[] { "street", "avenue", "road", "blvd", "suite", "floor", "building" };
            return addressWords.Any(w => corpus.Contains(w, StringComparison.OrdinalIgnoreCase));
        }

        private static string BuildCorpus(AnalyzeJobRiskRequestDto r)
            => string.Join(" ",
                r.JobTitle,
                r.CompanyName,
                r.JobDescription,
                r.SalaryDetails ?? string.Empty,
                r.ApplicationInstructions ?? string.Empty,
                r.EmployerInformation ?? string.Empty);

        private static string ClassifyRisk(int score) => score switch
        {
            <= 24 => "Low",
            <= 49 => "Moderate",
            <= 74 => "High",
            _     => "Very High"
        };

        private static List<string> BuildVerificationSteps(
            List<JobRiskIndicatorDto> evaluated,
            AnalyzeJobRiskRequestDto request)
        {
            var steps = new List<string>
            {
                $"Search for '{request.CompanyName}' on LinkedIn, Companies House (UK), or your country's official business registry.",
                "Confirm the job posting also appears on the employer's official website under their Careers section.",
                "Look for reviews of the employer on Glassdoor, Indeed, or Trustpilot.",
                "Never pay any fee to a recruiter or employer as a condition for applying or starting work.",
                "If asked for personal documents, verify the recruiter's identity via a video call and official company email first."
            };

            if (evaluated.Any(e => e.Key == "personal_info_early" && e.Triggered))
                steps.Add("Do NOT share your SSN, passport, or bank details until you have a signed employment contract from a verified employer.");

            if (evaluated.Any(e => e.Key == "suspicious_apply_method" && e.Triggered))
                steps.Add("Apply only through official channels (company website, LinkedIn Easy Apply). Be cautious of WhatsApp-only or personal-Gmail recruiters.");

            if (evaluated.Any(e => e.Key == "overpayment_scheme" && e.Triggered))
                steps.Add("Beware of overpayment check scams: if sent a cheque and asked to 'return the difference', stop — this is a classic fraud pattern.");

            if (evaluated.Any(e => e.Key == "unusual_payment" && e.Triggered))
                steps.Add("A legitimate employer will never ask you to pay via gift cards, cryptocurrency, or wire transfer.");

            return steps;
        }

        private static string BuildExplanation(
            int score, string category,
            List<JobRiskIndicatorDto> evaluated,
            AnalyzeJobRiskRequestDto request)
        {
            var triggered = evaluated.Where(e => e.Triggered).ToList();
            if (!triggered.Any())
                return $"The posting for '{request.JobTitle}' at '{request.CompanyName}' scored {score}/100 " +
                       $"(Risk: {category}). No risk indicator patterns were detected in the text provided. " +
                       "This does not confirm the posting is legitimate — always perform independent verification.";

            var labels = string.Join(", ", triggered.Select(t => $"'{t.Label}'"));
            return $"The posting for '{request.JobTitle}' at '{request.CompanyName}' scored {score}/100 " +
                   $"(Risk: {category}). The following {triggered.Count} risk indicator(s) were detected: " +
                   $"{labels}. These indicators are based on text patterns commonly associated with fraudulent " +
                   "postings and do NOT confirm that this specific posting is a scam. " +
                   "Proceed with caution and complete all recommended verification steps before applying.";
        }

        private static string BuildConfidenceNote(AnalyzeJobRiskRequestDto request)
        {
            var missing = new List<string>();
            if (string.IsNullOrWhiteSpace(request.SalaryDetails))          missing.Add("salary details");
            if (string.IsNullOrWhiteSpace(request.ApplicationInstructions)) missing.Add("application instructions");
            if (string.IsNullOrWhiteSpace(request.EmployerInformation))     missing.Add("employer information");

            if (!missing.Any())
                return "All input fields were provided, which improves indicator coverage. " +
                       "However, results depend entirely on the text submitted and cannot reflect real-world facts.";

            return $"The following fields were not provided: {string.Join(", ", missing)}. " +
                   "Missing fields reduce indicator coverage and may cause the overall risk score to underestimate actual risk.";
        }

        // ── Persistence helpers ───────────────────────────────────────────────────

        private async Task<CandidateProfile> RequireCandidateAsync(Guid userId)
        {
            var candidate = await _context.CandidateProfiles
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (candidate == null)
                throw new KeyNotFoundException("Candidate profile not found.");

            return candidate;
        }

        private static JobRiskAnalysisResultDto ToResultDto(
            JobRiskAnalysis entity,
            List<JobRiskIndicatorDto> indicators)
        {
            var warningSigns = JsonSerializer.Deserialize<List<string>>(entity.WarningSummaryJson)
                ?? new List<string>();
            var steps = JsonSerializer.Deserialize<List<string>>(entity.VerificationStepsJson)
                ?? new List<string>();

            return new JobRiskAnalysisResultDto
            {
                AnalysisId        = entity.Id,
                JobTitle          = entity.JobTitle,
                CompanyName       = entity.CompanyName,
                AnalyzedAt        = entity.CreatedAt,
                RiskScore         = entity.RiskScore,
                RiskCategory      = entity.RiskCategory,
                Explanation       = entity.Explanation,
                Indicators        = indicators,
                WarningSigns      = warningSigns,
                VerificationSteps = steps,
                ConfidenceNote    = entity.ConfidenceNote
            };
        }

        // ── Nested helper type ────────────────────────────────────────────────────

        private sealed record RiskIndicator(
            string Key,
            string Label,
            int MaxPoints,
            string Category,
            string[] Keywords);
    }
}
