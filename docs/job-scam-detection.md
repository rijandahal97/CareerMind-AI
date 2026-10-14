# 🛡️ Job Scam Risk Detection — Architecture & Feature Documentation

## Overview

**Day 14 — CareerMind Job Scam Detection Intelligence** adds a deterministic, explainable risk-scoring engine that analyses a submitted job posting for patterns commonly associated with fraudulent or misleading employment advertisements.

> **No external APIs, web scraping, or machine-learning models are used.** All scoring is performed locally against configurable text-pattern rules. The system produces risk indicators, not verified facts.

---

## Architecture

This feature follows the existing **Clean Architecture** layering:

```
CareerMind.Domain
  └── Entities/JobRiskAnalysis.cs          ← Persisted result entity

CareerMind.Application
  ├── DTOs/JobRisk/
  │   ├── AnalyzeJobRiskRequestDto.cs      ← Input DTO (validated)
  │   ├── JobRiskIndicatorDto.cs           ← One evaluated indicator
  │   ├── JobRiskAnalysisResultDto.cs      ← Full response DTO
  │   └── JobRiskAnalysisSummaryDto.cs     ← History list row
  └── Interfaces/IJobRiskAnalysisService.cs

CareerMind.Infrastructure
  ├── Services/JobRiskAnalysisService.cs   ← Scoring engine + persistence
  └── Migrations/20261014071454_AddJobRiskAnalysis.cs

CareerMind.API
  └── Controllers/JobRiskController.cs     ← REST endpoints

client (React + Vite)
  └── src/pages/candidate/
      ├── JobScamDetector.jsx
      └── JobScamDetector.css
```

---

## REST API Endpoints

All endpoints require a valid JWT (`Authorization: Bearer <token>`).

| Method | Route                     | Description                                          |
|--------|---------------------------|------------------------------------------------------|
| POST   | `/api/jobrisk/analyze`    | Submit a job posting for risk analysis (persists result) |
| GET    | `/api/jobrisk/history`    | List current user's previous analyses (newest first) |
| GET    | `/api/jobrisk/{id}`       | Retrieve a single saved analysis (403 if not owner)  |

### POST `/api/jobrisk/analyze` — Request Body

```json
{
  "jobTitle": "Remote Data Entry Specialist",
  "companyName": "Global Hire LLC",
  "jobDescription": "Earn $1000 per day from home! Act now — limited spots...",
  "salaryDetails": "Unlimited income guaranteed",
  "applicationInstructions": "Send registration fee via western union",
  "employerInformation": null
}
```

### Response

```json
{
  "analysisId": "3fa85f64-...",
  "jobTitle": "Remote Data Entry Specialist",
  "companyName": "Global Hire LLC",
  "analyzedAt": "2026-10-14T07:14:00Z",
  "riskScore": 83,
  "riskCategory": "Very High",
  "explanation": "The posting scored 83/100 (Risk: Very High). Detected indicators: 'Requests Upfront Fee', 'Unusual Payment Method'...",
  "indicators": [ ... ],
  "warningSigns": ["Requests Upfront Fee", "Unusual Payment Method"],
  "verificationSteps": ["Search for 'Global Hire LLC' on LinkedIn...", ...],
  "confidenceNote": "The following fields were not provided: employer information...",
  "disclaimer": "This analysis is based on configurable text-pattern indicators..."
}
```

---

## Scoring Algorithm

### Indicator Table

| Key                    | Label                                    | Max Points | Category     |
|------------------------|------------------------------------------|-----------|--------------|
| `upfront_fee`          | Requests Upfront Fee or Payment          | 25        | Financial    |
| `unusual_payment`      | Unusual Payment Method Requested         | 15        | Financial    |
| `overpayment_scheme`   | Potential Overpayment / Check Scam       | 20        | Financial    |
| `unrealistic_salary`   | Unrealistic Salary Promise               | 15        | Promises     |
| `guaranteed_job`       | Guaranteed Hiring Without Assessment     | 10        | Promises     |
| `urgency_pressure`     | Artificial Urgency / Pressure Tactics    | 10        | Pressure     |
| `personal_info_early`  | Requests Sensitive Personal Info Early   | 15        | Privacy      |
| `no_company_address`   | No Company Address or Contact Details    |  5        | Legitimacy   |
| `vague_description`    | Vague or Generic Job Description         |  5        | Legitimacy   |
| `suspicious_apply_method` | Suspicious Application Method         | 10        | Application  |
| `work_from_home_scam`  | Vague 'Work from Home' with No Details   |  5        | Application  |
| **Total possible**     |                                          | **135**   |              |

### Score Normalisation

```
raw_score = sum of triggered indicator MaxPoints
score     = round(raw_score × 100 ÷ total_max_possible)
score     = clamp(score, 0, 100)
```

### Risk Categories

| Score Range | Category  |
|-------------|-----------|
| 0 – 24      | Low       |
| 25 – 49     | Moderate  |
| 50 – 74     | High      |
| 75 – 100    | Very High |

### Indicator Evaluation Logic

- **Keyword-based indicators** — case-insensitive substring search across the composite text corpus (all input fields joined).
- **Custom logic indicators:**
  - `no_company_address` — triggers when `EmployerInformation` is blank AND the corpus contains no recognisable address words (street, avenue, road, etc.).
  - `vague_description` — triggers when the job description contains fewer than 60 words.

---

## Key Design Decisions

1. **No external services** — The engine is fully self-contained. Results are reproducible and auditable.
2. **Explainability first** — Every indicator includes a `Rationale` field explaining exactly why it was triggered or not.
3. **Mandatory disclaimer** — The response DTO always includes a hardcoded disclaimer that the analysis cannot confirm legitimacy or fraud.
4. **Confidence note** — When optional fields are omitted, the response explicitly notes which fields were missing and how this reduces indicator coverage.
5. **User data isolation** — The `GetRiskAnalysis` endpoint throws `UnauthorizedAccessException` (→ HTTP 403) if the requested analysis belongs to another user.
6. **Soft deletes** — `IsDeleted` flag inherited from `BaseEntity`; deleted records are filtered in all queries.

---

## Example Scenarios

### Scenario A — Legitimate posting (low risk)
- Full company address provided ✓
- Reasonable salary range ✓
- Detailed job description (>60 words) ✓
- Apply via official website ✓
- **Expected score:** < 25 (Low)

### Scenario B — Classic fee scam (very high risk)
- "Send registration fee via western union" → `upfront_fee` + `unusual_payment` fire
- "Earn $1000 per day guaranteed" → `unrealistic_salary` fires
- "Act now — limited spots" → `urgency_pressure` fires
- No employer information → `no_company_address` fires
- Very short description → `vague_description` fires
- **Expected score:** 70–85 (High / Very High)

### Scenario C — Overpayment scam
- "We will send you an overpayment cheque, return the difference" → `overpayment_scheme` fires
- **Expected score:** Moderate to High

---

## Limitations

1. **False positives** — A legitimate government agency may ask for ID. A real Amazon-affiliated job may mention "prime" which contains no triggering terms. The system cannot distinguish context.
2. **False negatives** — A sophisticated scam that avoids trigger phrases will score low. Low score ≠ legitimate.
3. **Input quality** — Scores depend entirely on what the user submits. Abbreviated or paraphrased descriptions reduce indicator coverage.
4. **No real-time data** — The engine does not query company registries, LinkedIn, or any live database.
5. **Language** — Indicators are English-only. Postings in other languages will produce lower (potentially misleading) scores.

---

## Database

**New table:** `JobRiskAnalyses`

| Column                   | Type         | Notes                               |
|--------------------------|--------------|-------------------------------------|
| `Id`                     | uniqueidentifier | PK, from BaseEntity               |
| `CandidateProfileId`     | uniqueidentifier | FK → CandidateProfiles (Cascade)  |
| `JobTitle`               | nvarchar(max) |                                    |
| `CompanyName`            | nvarchar(max) |                                    |
| `JobDescription`         | nvarchar(max) |                                    |
| `SalaryDetails`          | nvarchar(max) | nullable                           |
| `ApplicationInstructions`| nvarchar(max) | nullable                           |
| `EmployerInformation`    | nvarchar(max) | nullable                           |
| `RiskScore`              | int           | 0–100                              |
| `RiskCategory`           | nvarchar(max) | Low/Moderate/High/Very High        |
| `TriggeredIndicatorsJson`| nvarchar(max) | JSON array of triggered key strings|
| `WarningSummaryJson`     | nvarchar(max) | JSON array of warning sign labels  |
| `Explanation`            | nvarchar(max) |                                    |
| `VerificationStepsJson`  | nvarchar(max) | JSON array of step strings         |
| `ConfidenceNote`         | nvarchar(max) |                                    |
| `CreatedAt`              | datetime2     | From BaseEntity                    |
| `IsDeleted`              | bit           | Soft delete from BaseEntity        |

---

## Future Improvements

- **Configurable indicator weights** — Load indicator definitions and point values from `appsettings.json` or a database table to allow A/B testing and tuning without redeployment.
- **Severity tiers** — Allow partial scoring (e.g. 50% points for "possible" matches vs 100% for "definite" matches).
- **Phrase context** — Check keyword proximity (e.g. "western union" in the purpose field vs a generic mention) to reduce false positives.
- **Company registry lookup** — Integrate with Companies House (UK) or SEC EDGAR (US) APIs to verify employer existence.
- **Multi-language support** — Translate indicator patterns to support postings in other languages.
- **Aggregate reporting** — Allow employers/admins to see aggregate risk patterns across reported postings.
