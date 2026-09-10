# AI Job Matching Architecture

## Purpose
This document provides the standard architecture and formulas for calculating AI-driven Job Matching intelligence within the CareerMind platform. It lays the groundwork to allow candidates to discover roles suited to their skills and preferences, offering deterministic and explainable results.

## AI Future-Readiness & Disclaimer
**IMPORTANT NOTE:** As of Day 5, this architecture provides an **AI-ready deterministic intelligence foundation**. It calculates matches based on strict deterministic scoring and rules applied to structured data. It *does not* claim to contain a currently trained machine-learning or LLM model.
The architecture is specifically designed with the *Strategy Pattern/Services Layer* approach so that the deterministic calculations can seamlessly be replaced or augmented later using:
- ML Ranking models.
- Embedding-based semantic skill matchers.
- LLM-powered explanations and career advice.

## Architecture & Responsibilities
- **Matching Inputs**: Accepts `CandidateProfile` (with nested Skills, Experience, Education, Preferences) and `Job` (with nested JobSkills, Requirements, etc.).
- **AIJobMatchingService**: Consumes EF Core context to evaluate alignment across four dimensions (Skill, Experience, Education, Preference). It outputs a strongly typed `JobMatchResultDto`.
- **API Endpoints**: 
  - `GET /api/candidates/me/job-matches` - Retrieves sorted match list for the authenticated user.
  - `GET /api/candidates/me/job-matches/{jobId}` - Retrieves highly detailed analysis.
- **Security**: The candidate endpoints demand standard JWT token `[Authorize]`. The identity `candidateUserId` is strictly pulled via the authenticated claim contexts. No endpoints permit arbitrary `candidateId` lookups. Cross-tenant candidate exposure is explicitly mitigated.

## Matching Formula & Weighting
The matching calculation currently generates a 0-100 overall score by combining the weighted sub-scores:

- **Skill Match**: 50%
- **Experience Match**: 20%
- **Education Match**: 15%
- **Preference Match**: 15%

`Overall Score = (Skill * 0.50) + (Experience * 0.20) + (Education * 0.15) + (Preference * 0.15)`

## Dimension Scoring

### 1. Skill Matching Logic
Iterates through required and optional `JobSkill` components.
- **Required Skills**: Double weight, missing these significantly impacts the match and flags a skill-gap.
- **Proficiency matching**: 100% matched if candidate `ProficiencyLevel >= MinimumProficiencyLevel`. Submits 50% partial credit if proficiency falls short, issuing a gap warning.
- Yields: `MatchedSkills`, `MissingSkills`.

### 2. Experience Matching
Compare candidate `YearsOfExperience` against job `MinimumExperienceYears`.
- Experience Exceeds/Meets (100% credit)
- Experience Partially Meets (50% credit, >= half the requirement)
- Does Not Meet (0% credit)

### 3. Education Matching
Simple semantic fallback due to data constraints; determines if role explicitly dictates a degree requirement and evaluates if candidate possesses standard mapped degrees (Bachelor, Master, etc.)

### 4. Preference Matching
Checks alignment for: Remote work affinity, desired location, salary minimums vs. maximum provisions.
Yields varying levels of match depending on fulfilled overlaps.

## Explainability
Generates user-friendly strings injected into `JobMatchResultDto.Explanations`, `Strengths`, and `Gaps` to maintain transparency. 
- "Your continuous experience meets the minimum requirement."
- "You are missing Azure, which is an important requirement for this role."

## Skill-Gap Generation
Highlights crucial missing skills or proficiency shortfalls, designed intentionally to pipe later into AI Career Coaching roadmap tools and personalized course recommendations.

## Limitations of Deterministic Scoring
- Vocabulary mismatch: C# vs. C-Sharp would fail strict matching (until semantic clustering is integrated).
- Fixed weighting: Hardcoded config might not fit varying industry contexts equally.
