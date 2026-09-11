# Career Gap Analysis & Personalized Skill Roadmap Engine (Day 6)

## Purpose
The Career Gap Analysis engine empowers candidates by comparing their current professional capabilities against the exact requirements of a target job. Instead of a simple "match score" (handled by Day 5), this module acts as a career intelligence coach, revealing exact actionable steps for improvement.

## Inputs
- **Candidate Profile & Skills**: Current proficiency level across various skills, derived from the database.
- **Target Job & Requirements**: Required skills (and optional ones), minimum proficiency levels, and importance weights.
- **Existing Day 5 Match Result**: Analyzes existing scores (overall match score) to establish baseline compatibility.

## Skill Classification
For each job requirement, the engine evaluates the candidate's existing skill stack against the required mastery:
- **STRONG**: Candidate has the skill and their proficiency is greater than or equal to the requirement.
- **DEVELOPING**: Candidate has the skill, but current proficiency is lower than required.
- **MISSING**: Candidate does not possess the required skill at all.

## Gap Severity Calculation
Gap severity determines how far away the candidate is from expectations. It is based on:
1. `rawImportance`: Baseline importance (1-5), doubled if the skill `IsRequired`.
2. `currentProficiency` vs `requiredProficiency`: Difference determines the physical gap.
- **HIGH**: Skill completely missing and has a high importance (>= 3) or is required. Discrepancy >= 2 proficiency levels.
- **MEDIUM**: Skill completely missing but lower importance. Discrepancy of 1 level but high importance (>= 4).
- **LOW**: Minor deficiency in a lower importance skill.

## Priority Calculation
Priority score determines the order of the roadmap. The scoring logic adds weight:
- High Severity (+100), Medium (+50), Low (+10)
- Missing Skill (+20)
- High Importance (+30), Medium Importance (+15)
The engine inverts this score numerically, sorting the final outputs from Highest Priority to Lowest.

## Career Readiness Score
The `currentReadinessScore` borrows from the robust, multi-faceted matching engine created on Day 5 (`OverallMatchScore`). It holistically encapsulates how "ready" the individual is for the job, serving as an anchor metric.

## Roadmap Generation
The engine aggregates the prioritized gaps and structures them sequentially (from high priority to low priority). For the Day 6 deterministic model, it takes the top 5 most impactful gaps and translates them into actionable roadmap items showing current proficiency, target proficiency, severity, and personalized explanation.

## Explainability
Deterministic rules heavily comment on *why* something is recommended. Instead of AI black boxes (yet), strings are formulated based on exact business conditions e.g., "Your proficiency in React is below the required level" or "Azure is required for this role but is not currently listed in your skills."

## API Endpoints
- `GET /api/candidates/me/career-gap/{jobId}` Restricts execution exclusively to matching candidate IDs via the securely stored token (JWT).

## Security
Enforced by:
1. Ensure the user's role/claim maps appropriately (using standard `[Authorize]` and `ClaimTypes.NameIdentifier`).
2. Only extracting candidate context from HTTP context, meaning users explicitly cannot generate gap analysis for another candidate.

## Frontend Flow
Within `CandidateJobDetails`, next to the matching information, a rich "Personalized Career Gap Analysis" panel attempts to fetch the career gap metrics asynchronously. It renders Strong, Developing, and Missing skills, displays the overall readiness score, and builds a UI card sequence depicting the step-by-step roadmap to mastery.

## Relationship with Day 5 Matching Engine
The Career Gap Engine builds ON TOP OF the Day 5 Matching System. Rather than completely duplicating candidate/job capability score calculators, Day 6 fetches the `JobMatchResultDto` using the `IAIJobMatchingService`. The Day 5 matching engine defines *suitability* for an employer, whereas Day 6 defines *learning trajectory* for a candidate.

## Future AI/ML Integration
- Semantic matching using embeddings could replace rigid string matching.
- ML could predict which roadmap steps have historic success in upskilling paths.
- LLMs could formulate dynamic rich-text insights, recommending exact courses dynamically based on user context.

## Current Deterministic Limitations
- Relies on strict categorization and rigid mappings (enum numerical integers).
- Recommends general steps rather than distinct course platforms (e.g. lacks "Take this Udemy course").
- Doesn't estimate hours/days precisely needed for completion.
