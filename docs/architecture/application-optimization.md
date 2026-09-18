# Application Optimization Architecture

## 1. Purpose
The Application Optimizer is an AI-ready deterministic career intelligence module designed to maximize a candidate's readiness before applying for a specific job. It aggregates multiple career insights to compute an overall Readiness Score, generates a custom cover letter, optimizes the profile headline/summary, and suggests prioritized actions if necessary.

## 2. Architecture & Domain Model
The module follows the existing Clean Architecture principles spanning across:
- **Domain**: `ApplicationOptimization` (Score, Profile/Resume alignments, Optimized content) and `ApplicationOptimizationSuggestion` (Actionable steps).
- **Application**: DTOs and `IApplicationOptimizationService`.
- **Infrastructure**: `ApplicationOptimizationService` logic and `CareerMindDbContext` (EF Core) mappings.
- **Presentation**: `CandidatesController` endpoints and a premium React Dashboard page (`ApplicationOptimizer.jsx`).

## 3. Scoring Model & Readiness Calculation
The overall `ApplicationReadinessScore` (0-100) is aggregated using weighted deterministic sub-components:
- **Skill Alignment (30%)**: Matches Candidate skills with Job requirements.
- **Experience Alignment (20%)**: Candidate's years of experience proportionally measured against required min years.
- **Resume Alignment (15%)**: Existing ATS Score from the latest `ResumeAnalysis`.
- **Interview Readiness (15%)**: Averages scores from existing `InterviewSession` instances.
- **Profile Alignment (10%)**: Checks for basic completeness, headline, bio, and external links. 
- **Education Alignment (10%)**: Validates education presence.

## 4. Missing Data & Deterministic Rules
Missing data is not treated as perfect. Critical gaps directly block readiness scaling.
- If a Resume is missing, a Critical Blocking suggestion reminds the user to upload one.
- If Interview history is missing, Interview Readiness scores null and is explicitly communicated through actionable suggestions without inventing arbitrary metrics.

## 5. Security Enforcements
Users authenticate implicitly via existing JWT (`TokenService`). Candidate profiles act as the data pivot, guaranteeing horizontal security isolated per `CandidateProfileId` mapped against `JobId`. External ID trust falls entirely on the `ClaimTypes.NameIdentifier`.

## 6. Frontend Integration Details
- Integrated seamlessly within the React Client (`CandidateDashboard.jsx` & `CandidateJobDetails.jsx`).
- Uses existing design system tokens and `lucide-react` icons.
- Exposes visually distinct ring-charts and card-based suggestions, delivering a Premium-Level AI aesthetic experience without new structural dependencies.

## 7. AI Roadmap Integration
Currently, the system is 100% deterministic (C# logic) but built with an AI-ready foundation. Future integration can inject Large Language Models recursively to enrich `OptimizedHeadline`, `OptimizedSummary`, or `CoverLetter` by overriding standard string-builders natively within `ApplicationOptimizationService`.

## 8. Testing Strategy
A fully mapped test suite in `ApplicationOptimizationServiceTests.cs` using the project's typical strictly-asserted xUnit & In-Memory Entity Framework architecture. Tests rigorously cover correct alignment algorithms, gracefully handled missing edge cases (Null Resumes, Null Interviews), content fabrication safeguards (preventing AI hallucination-style false skill claims), and persistence idempotents (updating records repeatedly vs. duplicate sets).
