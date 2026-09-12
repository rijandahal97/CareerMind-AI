# Career Path Intelligence Engine — Day 7 Architecture

## Purpose

The Career Path Intelligence Engine evolves CareerMind from job matching and gap analysis into full career trajectory intelligence. It answers the question:

> "What career path should I follow next, and what should I do to reach it?"

This is a deterministic, explainable, AI-ready engine built on top of Day 3–6 foundations (profile, skills, experience, job matching, gap analysis).

---

## Architecture

```
CareerMind.Application/
├── Models/
│   ├── CareerRoleDefinition.cs       # Career role model
│   └── CareerRoleCatalogue.cs        # Static extensible catalogue
├── DTOs/CareerPath/
│   ├── CareerPathAnalysisDto.cs      # Root response DTO
│   ├── CareerRecommendationDto.cs    # Per-role recommendation
│   ├── CareerPathStageDto.cs         # Career path timeline stage
│   ├── CareerSkillRequirementDto.cs  # Skill classification
│   └── NextCareerMoveDto.cs          # Immediate next step
├── Interfaces/
│   └── ICareerPathIntelligenceService.cs
└── Services/
    └── CareerPathIntelligenceService.cs
```

No database schema changes. Career roles are defined as application-level configuration, not persisted entities.

---

## Career Role Model

Roles are defined in `CareerRoleCatalogue.cs` as a static, strongly-typed list covering:
- Junior .NET Developer (Level 1)
- Backend Developer (Level 2)
- Full Stack Developer (Level 2)
- Frontend Developer (Level 2)
- Senior Backend Developer (Level 3)
- Software Architect (Level 4)

Each role defines:
- Required Skills (must-have)
- Preferred Skills (nice-to-have)
- Minimum Experience Years
- Relevant Education
- Related Roles (for career pathing)
- Seniority Level and Career Progression Level

The catalogue is extensible — new roles can be added without any code changes to the engine.

---

## Scoring Strategy

### Career Compatibility Score (0–100)

| Factor | Weight | Rationale |
|--------|--------|-----------|
| Skills | 50% | Primary career differentiator |
| Experience | 20% | Seniority/readiness indicator |
| Education | 10% | Supporting qualification |
| Career Preference Alignment | 10% | Intent signal |
| Current Career Context | 10% | Transition proximity |

### Skill Scoring
Each required skill is scored based on candidate's ProficiencyLevel (1–4 = Beginner–Expert).
- Score = (sum of candidate proficiency) / (max possible proficiency × skill count) × 100

### Experience Scoring
- Score = (candidate years) / (role minimum years) × 100, capped at 100

### Education Scoring
- 100 if field of study matches relevant education
- 50 for unrelated education
- 0 if no education records

### Preference Scoring
- 100 if PreferredJobRoles contains the role name
- 50 otherwise

### Context Scoring
- 100 if current job title matches a related role
- 30 otherwise

### Readiness Score
- `(skills_score × 0.7) + (experience_score × 0.3)`

---

## Career Recommendation Algorithm

1. Load candidate profile via `ICandidateProfileService`
2. Iterate all roles in `CareerRoleCatalogue`
3. For each role, compute:
   - Skills Score (with skill classification: STRONG/DEVELOPING/MISSING)
   - Experience Score
   - Education Score
   - Preference Score
   - Context Score
4. Compute weighted Compatibility Score
5. Compute Readiness Score
6. Compute Skill Coverage %
7. Determine Transition Difficulty (LOW/MEDIUM/HIGH)
8. Generate explainable reasons
9. Sort by Compatibility Score descending
10. Select best career

---

## Career Path Generation

For the best career recommendation:
1. Find all roles in the same `CareerCategory`
2. Filter to roles at or below the target's progression level
3. Order by `CareerProgressionLevel`
4. For each stage, compute readiness using the same scoring engine
5. Mark the candidate's current role
6. Identify missing skills and suggest next steps

---

## Skill Transition Analysis

Skills are classified per-role:
- **STRONG**: Candidate has skill at Intermediate+ proficiency
- **DEVELOPING**: Candidate has skill at Beginner proficiency
- **MISSING**: Candidate does not have the skill

Priority is derived from status:
- MISSING → HIGH priority
- DEVELOPING → MEDIUM priority
- STRONG → LOW priority

---

## Readiness Calculation

Career Readiness = `(Skills Score × 0.7) + (Experience Score × 0.3)`

This is computed both at:
- Overall level (best career's readiness)
- Per career path stage level

---

## Explainability

Every recommendation includes:
- Per-factor explanations (e.g., "Your C# and ASP.NET Core proficiency strongly aligns with Backend Developer requirements")
- Overall insight sentences
- Explanations reference specific skills, roles, and scores — never generic boilerplate

---

## API

### Endpoint
```
GET /api/candidates/me/career-path
```

### Security
- `[Authorize]` attribute
- Uses `ClaimTypes.NameIdentifier` from JWT
- Candidate can only access their own career intelligence

### Response: `CareerPathAnalysisDto`
- `overallCareerReadiness`: decimal (0-100)
- `recommendedCareers`: List of CareerRecommendationDto (sorted by score)
- `bestCareer`: Top recommendation
- `careerPath`: Timeline progression stages
- `nextCareerMove`: Immediate actionable next step
- `explainableInsights`: Human-readable insight sentences

---

## Frontend Integration

### Page: `CareerIntelligence.jsx`
Located at `/candidate/career-intelligence`

### Components:
- Score circles with SVG ring indicators
- Skill analysis table with status/priority badges
- Career path vertical timeline with readiness bars
- Next career move card with action box
- All recommendations grid
- Loading, error, and empty states

### Design:
- Consistent with existing Candidate.css dark theme
- Responsive grid layout
- Hover animations and glassmorphism

---

## Testing

10 unit tests in `CareerPathIntelligenceServiceTests.cs`:

1. Strong candidate gets Backend Developer as strong recommendation
2. Candidate with missing skills returns skill gaps
3. Recommendations are ordered by score
4. Career path contains progression stages
5. Next career move is derived from candidate profile
6. High skill alignment produces high compatibility
7. Low skill alignment produces lower compatibility
8. Recommendations contain explainable reasons
9. Unauthorized/missing candidate throws exception
10. Empty candidate profile is handled safely

---

## Future ML/LLM Integration

This engine is explicitly designed as a **deterministic AI-ready foundation**:

1. **Role catalogue** can be replaced with ML-predicted roles from job market data
2. **Scoring weights** can be tuned via ML regression on successful career transitions
3. **Skill matching** can use NLP embeddings for semantic skill similarity
4. **Explanation generation** can be enhanced with LLM-generated natural language
5. **Career path prediction** can use graph neural networks on career transition datasets

The current deterministic implementation provides:
- Baseline correctness validation for future ML models
- Explainable scoring that ML models must match or exceed
- Structured output format that remains stable across engine upgrades
