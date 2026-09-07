# CareerMind: AI Career Intelligence Platform

## Overview
CareerMind is an AI-powered Career Intelligence Platform built for final year projects. It connects candidates with employers using AI to analyze resumes, calculate job match scores, provide skill gap analyses, generate personalized career roadmaps, and conduct mock interviews.

## Technology Stack
- **Backend**: .NET 10 Web API, C#, Entity Framework Core
- **Database**: Microsoft SQL Server (Local: `RIJAN\SQLEXPRESS`)
- **Architecture**: Modular Monolith, Clean Architecture principles
- **Frontend**: React + Vite, Tailwind CSS, React Router, Axios

## Architecture Overview
The project is split into the following layered architecture:
- `CareerMind.API`: Entry point, Controllers, Dependency Injection, Swagger.
- `CareerMind.Application`: Business logic, Interfaces, DTOs, Validators, Services.
- `CareerMind.Domain`: Enterprise business entities, Enums, Exceptions.
- `CareerMind.Infrastructure`: Data access, Entity configurations, Migrations, External abstractions for AI integrations.

## Local Development Setup

### Prequisites
- .NET 10 SDK
- Node.js (v24+)
- SQL Server (SQLEXPRESS instance `RIJAN\SQLEXPRESS`)
- Git

### Backend Setup
1. Open the solution in your IDE or use CLI.
2. Ensure you have user secrets or `appsettings.Development.json` configured properly. Database is configured to point to `CareerMindDB` on local SQLEXPRESS using Windows Authentication.
3. Apply the initial migration:
   ```bash
   dotnet ef database update -p src/CareerMind.Infrastructure -s src/CareerMind.API
   ```
4. Run the API:
   ```bash
   dotnet run --project src/CareerMind.API
   ```

### Frontend Setup
1. Open terminal at `client/careermind-web`.
2. Install packages:
   ```bash
   npm install
   ```
3. Run dev server:
   ```bash
   npm run dev
   ```

### Database
- Uses Code-First Entity Framework Core.
- The connection uses a trusted connection to `RIJAN\SQLEXPRESS`. Ensure your Active Directory / Windows user has permissions to create tables and databases there.
