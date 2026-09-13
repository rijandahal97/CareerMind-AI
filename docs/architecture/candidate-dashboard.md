# Candidate Dashboard Architecture — Day 8

## Overview

The CareerMind Candidate Dashboard is a premium SaaS-level career workspace built in React + Vite. It was upgraded on Day 8 from a basic prototype into a full-featured, responsive application shell with AI-powered career intelligence integration.

---

## Component Structure

```
client/careermind-web/src/
├── index.css                          # Global CSS reset + utility tokens
├── main.jsx                           # React entry point with global CSS import
├── App.jsx                            # Root router (Auth → Candidate / Admin / Employer)
│
├── components/
│   └── StateComponents.jsx            # Reusable Loading / Empty / Error / NoResults states
│
├── pages/candidate/
│   ├── CandidateDashboard.jsx         # ★ Application Shell — sidebar + header + routing
│   ├── Candidate.css                  # Shell-level design system (light theme SaaS)
│   │
│   ├── DashboardOverview.jsx          # ★ Overview Dashboard page (post-login home)
│   ├── DashboardOverview.css          # Dashboard-specific card/grid styles
│   │
│   ├── CandidateJobs.jsx              # Premium job search with tabs, filters, AI matches
│   ├── CandidateJobs.css              # Job listing card + search UI styles
│   │
│   ├── CandidateApplications.jsx      # Application tracker with status badges
│   ├── CandidateSavedJobs.jsx         # Saved/bookmarked jobs list
│   │
│   └── CareerIntelligence.jsx         # Full Career Path Intelligence page
│       CareerIntelligence.css         # Dark-themed intelligence UI
│
└── services/
    ├── api.js                         # Axios client with JWT + refresh interceptors
    └── candidate.js                   # All candidate API service calls
```

---

## Application Shell Layout

### Left Sidebar (`CandidateDashboard.jsx`)
- **Brand:** CareerMind logo + wordmark
- **Nav Groups:**
  - MAIN: Overview, My Profile, Skills
  - CAREER: Jobs, Applications, Saved Jobs
  - INTELLIGENCE: Career Intelligence, Skill Roadmap, Career Path
  - TOOLS: Resume Builder, AI Assistant, Settings
- **Bottom:** User avatar + name + profile completion bar + Logout button
- **Responsive:** Fixed on desktop → slide-in drawer on mobile (≤1024px)

### Top Header
- Page title + subtitle based on active route
- Search bar (hidden on mobile)
- Notification bell with badge indicator
- User profile avatar + dropdown indicator

### Main Content
- Scrollable content area with `max-width: 1200px` centered container
- All routes rendered inside via nested `<Routes>` from React Router

---

## Dashboard Overview Page (`DashboardOverview.jsx`)

### Sections

| Section | Data Source |
|---|---|
| Welcome hero + greet | `useAuth()` + system time |
| Next Best Action card | Derived from profile + career intelligence data |
| Stats (Profile %, Readiness, Applications, Saved) | `getProfile()`, applications API, saved-jobs API |
| Recommended Jobs (top 4) | `getJobMatches()` |
| Recent Applications (top 3) | `/Candidates/me/applications` |
| Career Intelligence Preview | `getCareerPathIntelligence()` |
| Skill Roadmap Preview | `nextCareerMove.focusNext` from career path data |

### Data Strategy
- All API calls are made in **parallel** using `Promise.allSettled()` — a partial failure in one section does not crash the entire dashboard.
- Individual sections gracefully show empty states when their API data is unavailable.

---

## API Dependencies

All data is sourced from **existing Day 1–7 APIs**. No new backend endpoints were created on Day 8.

| Dashboard Section | Endpoint |
|---|---|
| Profile + completion | `GET /candidates/me/profile` |
| Applications | `GET /Candidates/me/applications` |
| Saved jobs | `GET /Candidates/me/saved-jobs` |
| AI job matches | `GET /candidates/me/job-matches` |
| Career intelligence | `GET /candidates/me/career-path` |
| Career gap | `GET /candidates/me/career-gap/{id}` |

---

## Data Flow

```
Browser Login
  → JWT stored in localStorage
  → AuthContext hydrated from token claims
  → ProtectedRoute enforces Candidate role
  → CandidateDashboard renders (shell layout)
    → DashboardOverview mounts
      → Promise.allSettled([5 parallel API calls])
      → Sections render with real data or graceful fallbacks
```

---

## Responsive Strategy

| Breakpoint | Behavior |
|---|---|
| ≥ 1280px (desktop) | Full sidebar + 2-col dashboard grid |
| 1024–1280px (laptop) | Full sidebar; stats 4-col |
| 768–1024px (tablet) | Slide-in sidebar drawer; dashboard col stacks to 1 |
| < 768px (mobile) | Stats grid 2-col; job cards stack vertically |
| < 480px (small mobile) | Stats grid 1-col; full width everything |

On mobile:
- Sidebar becomes a slide-in **drawer** triggered by hamburger icon
- Semi-transparent overlay closes the drawer on tap
- Search bar hidden in header; accessible via Jobs page

---

## Loading / Empty / Error States

### Per-Section State Handling

Every major dashboard section uses `Promise.allSettled` so other sections do not fail if one API is down.

### Reusable `StateComponents.jsx`

| Component | Appearance |
|---|---|
| `<LoadingState />` | Animated 3-dot loader with message |
| `<EmptyState />` | Dashed border, icon, descriptive message |
| `<ErrorState />` | Red accent, retry button callback |
| `<NoResultsState />` | Search-X icon with suggestions |

---

## Security Considerations

- **JWT identity only from backend claims** — all `/candidates/me/...` endpoints enforce candidate ownership server-side.
- The frontend never passes a candidateId from state/query params; all ownership decisions are made by the backend using claims from the JWT.
- Token refresh is handled transparently via Axios interceptors in `api.js`.
- Protected routes enforce `Candidate` role via `<ProtectedRoute allowedRoles={['Candidate']} />`.
- Unauthenticated requests are redirected to `/login`.

---

## Reusable UI Components

### In Shell (`Candidate.css`)
- `.btn-primary` — blue CTA button with hover lift
- `.btn-secondary` — outline ghost button
- `.btn-logout` — red-on-hover logout action
- `.input-premium` / `.select-premium` — form inputs with focus rings
- `.skill-card-premium` — tag chip for skills
- `.education-card-premium` — icon + detail education row
- `.placeholder-feature` — future-module placeholder with tag

### In Dashboard (`DashboardOverview.css`)
- `.stat-card` — KPI card with icon badge + progress bar
- `.job-card-compact` — job title + meta + score ring + action button
- `.app-card-compact` — application row with status pill
- `.intelligence-preview` — dark gradient card for career intelligence
- `ScoreRing` — SVG-based circular progress indicator (reusable component)
- `.roadmap-step` — priority dot + skill name row

### In Jobs (`CandidateJobs.css`)
- `.job-listing-card` — full-detail job card with match circle + save button
- `.jobs-tab-bar` — tab navigation with active underline indicator
- `.jlc-match` — circular match percentage indicator

---

## Database Changes

**None.** Day 8 is a frontend-only transformation. No migrations were created.

---

## Future Module Placeholders

The following sidebar items lead to clearly-marked placeholder pages (not broken routes):

- Skill Roadmap (`/candidate/roadmap`)
- Career Path Simulator (`/candidate/path`)
- Resume Builder (`/candidate/resume`)
- AI Interview Prep (`/candidate/assistant`)
- Account Settings (`/candidate/settings`)

Each placeholder includes:
- Descriptive icon
- Module title
- "In Development" tag badge

---

## Day 8 Git Commit

```
feat(ui): transform candidate dashboard into premium career workspace
```
