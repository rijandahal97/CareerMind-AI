# Career Analytics & Progress Intelligence Architecture

## Overview

The **Career Analytics & Progress Intelligence** module provides an aggregated, deterministic, and explainable dashboard for evaluating a candidate's overall readiness across the CareerMind platform. It consolidates intelligence from Profile, Skills, Job Matches, Resume ATS performance, Interview Readiness, Application Optimization, and Action Plans to give users a clear picture of their career footprint without using "fake" or "demo" statistics.

## Core Aggregation Principles

1.  **Strict Determinism**: Zero hardcoded or random metrics. All statistics are derived strictly from database records (`Resumes`, `InterviewSessions`, `ApplicationOptimizations`, etc.).
2.  **Graceful Degradation**: If a candidate lacks data in a specific module (e.g., no uploaded resume, zero mock interviews), the system safely defaults those fields to `0` or empty sets and prompts actionable insights to fill those gaps.
3.  **Security boundaries**: Data is aggregated exclusively for the authenticated user based on the JWT `candidateUserId`, ensuring isolation and multi-tenancy support.

## Data Sources & Calculations

### 1. **Overall Progress Score**
The holistic score represents the candidate's career readiness. It is capped at `100` and balances different aspects of the pipeline:
- `30%` -> Profile and Skill Completeness (Profile completeness logic and skill distributions).
- `30%` -> Application Readiness (Average score across all configured AI application optimizations).
- `20%` -> Resume ATS Strength (Average or latest analyzed resume score).
- `20%` -> Mock Interview Performance (Average score of completed AI mock sessions).

### 2. **Profile & Skill Intelligence**
- **Profile Completeness**: Looks at the presence of `Bio`, `CurrentJobTitle`, `Educations`, `WorkExperiences`, `Skills`, and `CareerPreferences`. Missing fields decrement the total score.
- **Skills Distribution**: Calculates "Strong" and "Developing" skills based on native `ProficiencyLevel`.

### 3. **Application & Job Intelligence**
- **ATS Analytics**: Queries the latest `ResumeAnalysis` and presents performance metrics. 
- **Application Optimization**: Calculates the count of applications marked as "Strong", "Moderate", or "Weak" based on the `ApplicationReadinessScore`.
- **Suggestions**: Analyzes completion states built directly from `ApplicationOptimizationSuggestion` entities.

### 4. **Mock Interview Analytics**
- Extracts `InterviewSession` records where `Status = Completed`. Calculates technical performance bounds from `TechnicalScore` and the overall confidence average from `OverallReadinessScore`.

## Security Model
- **Endpoint**: `GET /api/candidates/me/career-analytics`
- **Controller logic**: Relies entirely on `GetUserId()` inheriting from `ControllerBase`, preventing indirect object reference (IDOR) traversal.

## Frontend Integration
- Built using React (`CareerAnalytics.jsx`) leveraging a rich dashboard topology with radial charts, data cards, and actionable insights.
- **Empty States**: Specifically designed to incentivize candidates to complete modules logically—if ATS scores are missing, it directs them to "Resume Builder"; if interview scores are missing, it sends them to the "Mock Interview" center.
