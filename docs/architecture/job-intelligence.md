# Job Intelligence Architecture

## Overview
The Job Intelligence module is the core domain foundation that enables CareerMind-AI's future smart job matching algorithms. It includes the structured representation of jobs, employer profiles, categorized job fields, applicant metadata, and precise skill mappings to support multi-dimensional candidate evaluations.

## Domain Model
- **EmployerProfile**: Extends the `User` identity specifically for employer use cases, capturing company size, verification status, and contact endpoints.
- **Job**: The robust entity specifying the open role, encompassing fields for location, employment type, precise salary boundaries, and status.
- **JobSkill**: The intelligence pivot linking an open role to our normalized `Skill` entity. Tracks importance weight (1-5) and minimum proficiency.
- **JobCategory**: Normalization of job clusters (e.g. Software Development).
- **JobApplication**: Captures the candidate's bid for a job, encapsulating cover letters, resumes, timeline metrics, and lifecycle status.
- **SavedJob**: Bookmark entity allowing candidates to persist job interests.

## Job Lifecycle
1. **Draft/Posted**: Employer initiates a job listing. Status can be "Draft" or "Active".
2. **Receiving Applications**: Candidates apply via matching routes. System logs constraints (salary, experience bounds).
3. **Closing**: Employer or pipeline deadline shifts state to "Closed".

## Employer Workflow
1. **Profile Setup**: Employer updates `EmployerProfile`.
2. **Job Posting**: Submits new `Job` with exact metadata payload.
3. **Candidate Review**: Uses `/Employers/me/jobs/{id}/applications` to digest candidate flow.
4. **Status Triggers**: Progresses applications via status updates (Applied &rarr; UnderReview &rarr; Interview &rarr; Hired/Rejected).

## Candidate Application Workflow
1. **Discovery**: Queries `Jobs` via advanced filters (`/Jobs?keyword=X&location=Y`).
2. **Saved State**: Books jobs of interest (`/Jobs/{id}/save`).
3. **Profile Extraction**: System packages their `CandidateProfile`.
4. **App Creation**: A singular `JobApplication` generated with unique `(JobId, CandidateProfileId)` indexing to prevent duplication.
5. **Timeline Viewing**: Dashboard displays the historical trajectory in "My Applications".

## Database Relationships
- `User` 1:1 `EmployerProfile`
- `EmployerProfile` 1:N `Job`
- `JobCategory` 1:N `Job`
- `Job` 1:N `JobSkill` N:1 `Skill`
- `Job` 1:N `JobApplication` N:1 `CandidateProfile`
- `Job` 1:N `SavedJob` N:1 `CandidateProfile`

All relational intersections maintain appropriate constraints, indexes, or CASCADE mechanisms to ensure data integrity without cascading cycles.

## API Overview
### Employers
- `GET/PUT /api/employers/me` - Profile operations
- `GET/POST /api/employers/me/jobs` - Job fetching/creation
- `GET/PUT/DELETE /api/employers/me/jobs/{id}` - Granular job manipulation
- `GET /api/employers/me/jobs/{id}/applications` - Applicant aggregation
- `PUT /api/employers/me/applications/{id}/status` - Applicant progression

### Candidates
- `GET /api/jobs` - Job Search
- `GET /api/jobs/{id}` - Details viewing
- `POST /api/jobs/{id}/apply` - Initiate application
- `POST/DELETE /api/jobs/{id}/save` - Bookmark toggle
- `GET /api/candidates/me/applications` - Historic apps
- `GET /api/candidates/me/saved-jobs` - Saved persistence

## Future AI Integration Boundary
An explicit boundary is established for upcoming AI calculations via `IAIJobMatchingService`. This abstract service definition allows the future integration of models that:
- Calculate Skill Match via `JobSkill.ImportanceWeight` and `CandidateSkill.ProficiencyLevel`.
- Calculate Experience Match via overlapping bounds.
- Inject `AiMatchScore` back into `JobApplication`.
