# CareerMind AI Interview Intelligence Architecture

## Overview
The "AI Interview Intelligence & Mock Interview Coach" uses deterministically generated simulated interviews to help candidates prepare for roles efficiently.

## Domain Model
- `InterviewSession`: Encompasses the entire mock interview attempt for a candidate, targeting an optional `Job`.
- `InterviewQuestion`: Individual questions logically partitioned by type (e.g., Technical, Behavioral, Mixed).
- `InterviewAnswer`: Submitted answers tracking comprehensive sub-scores based on deterministic evaluation.

## Interview Flow
1. **Selection:** Candidate selects their target role or broad skills focus, passing constraints like preferred `Difficulty` (Beginner to Advanced) and `InterviewType`.
2. **Generation:** Deterministic mapping to appropriate query templates by analyzing actual data like `JobSkills` from `Job` matches and corresponding Candidate Profile skills.
3. **Execution:** Candidate provides input text representing their answers in the simulated interface incrementally.
4. **Scoring:** On answer submission, the evaluation engine determines the score immediately.
5. **Completion:** Engine calculates `OverallReadinessScore` drawing upon completed query analyses, highlighting metrics across Communication, Completeness, Roles, and Technical Accuracy.

## Deterministic Evaluation Engine & Scoring Formulas
We enforce transparency by replacing opaque LLM evaluations with fully deterministic evaluations:
- **Completeness:** Assures thoroughness (Answer Word Count scaled against 50 words boundary ceiling).
- **Relevance:** Percentage of matched critical keywords outlined via `ExpectedTopics`, weighted directly per question base.
- **Technical Accuracy:** Directly related to relevance scaling (relevance * 0.9 multiplier).
- **Communication:** Evaluates precision, penalizing drastically short or overwhelmingly extraneous rambling.
- **Role Alignment:** Baseline fixed score, modulated conceptually depending upon behavioural patterns mapped appropriately.

**Total Score Weights:**
- Relevance: 25%
- Technical Accuracy: 30%
- Completeness: 20%
- Communication: 15%
- Role Alignment: 10%

## Question Generation Strategy
When generating questions for a `Technical` track, if the participant hasn't specified matching competencies within an applied Job, the system maps the highest demanding requirements automatically as assessment points. Behavioral models rely on mapped generic scenarios invoking the candidate's last title dynamically.

## Security Model
Extending the standard JWT methodology, endpoints strictly evaluate session ownership:
No candidate is ever permitted cross-access to external candidate mock histories. Controller checks evaluate token-based `CandidateProfileId` natively preceding EF execution paths.

## API Endpoints
- `POST /api/candidates/me/interviews` - Initializes session, configuring questions.
- `GET /api/candidates/me/interviews` - Queries historical mock history overview.
- `GET /api/candidates/me/interviews/{sessionId}` - Resumes mock session data payload.
- `POST /api/candidates/me/interviews/{sessionId}/answers` - Submits distinct answers securely per designated question.
- `GET /api/candidates/me/interviews/{sessionId}/readiness` - Recalculates/fetches complete Readiness analytics profile.
- `POST /api/candidates/me/interviews/{sessionId}/complete` - Signals logic closure enforcing snapshot storage processing.

## Future LLM Integration Strategy
The framework structure is heavily optimized for LLM interchangeability. Currently, scoring behaves deterministicallyâ€”but `CareerInterviewService` allows hot-swappable AI injection points parsing semantic embeddings against text rather than counting keyword density stringentially, keeping existing UI/UX and database architectures fully intact upon transition.
