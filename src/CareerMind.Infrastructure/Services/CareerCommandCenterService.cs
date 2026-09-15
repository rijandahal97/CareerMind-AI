using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CareerMind.Application.DTOs.CareerCommandCenter;
using CareerMind.Application.DTOs.CareerGap;
using CareerMind.Application.DTOs.CareerPath;
using CareerMind.Application.DTOs.Job;
using CareerMind.Application.Interfaces;
using CareerMind.Domain.Entities;
using CareerMind.Domain.Enums;
using CareerMind.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareerMind.Infrastructure.Services
{
    public class CareerCommandCenterService : ICareerCommandCenterService
    {
        private readonly CareerMindDbContext _context;
        private readonly ICareerPathIntelligenceService _careerPathIntelligenceService;
        private readonly IAIJobMatchingService _aiJobMatchingService;

        public CareerCommandCenterService(
            CareerMindDbContext context,
            ICareerPathIntelligenceService careerPathIntelligenceService,
            IAIJobMatchingService aiJobMatchingService)
        {
            _context = context;
            _careerPathIntelligenceService = careerPathIntelligenceService;
            _aiJobMatchingService = aiJobMatchingService;
        }

        public async Task<CareerCommandCenterDto> GetCareerCommandCenterAsync(Guid candidateUserId)
        {
            var candidate = await _context.CandidateProfiles
                .Include(c => c.CandidateSkills).ThenInclude(cs => cs.Skill)
                .Include(c => c.WorkExperiences)
                .Include(c => c.Educations)
                .Include(c => c.Certifications)
                .Include(c => c.CandidateLanguages).ThenInclude(cl => cl.Language)
                .Include(c => c.CareerPreference)
                .Include(c => c.ActionProgresses)
                .FirstOrDefaultAsync(c => c.UserId == candidateUserId);

            if (candidate == null)
            {
                throw new KeyNotFoundException("Candidate profile not found.");
            }

            // 1. Calculate Profile Completeness (0-100)
            decimal profileScore = CalculateProfileCompletenessScore(candidate);

            // 2. Fetch Career Path Intelligence
            CareerPathAnalysisDto? pathAnalysis = null;
            try
            {
                pathAnalysis = await _careerPathIntelligenceService.GetCareerPathIntelligenceAsync(candidateUserId);
            }
            catch
            {
                // Graceful handling if career path engine fails or returns null
            }

            var bestCareer = pathAnalysis?.BestCareer;
            decimal careerPathScore = bestCareer?.ReadinessScore ?? 50m;
            string targetRole = bestCareer?.CareerRole ?? (!string.IsNullOrEmpty(candidate.CurrentJobTitle) ? candidate.CurrentJobTitle : "Software Engineer");
            string currentRole = candidate.CurrentJobTitle ?? "Aspiring Professional";

            // 3. Fetch Job Market Match Intelligence
            List<JobMatchResultDto> matches = new List<JobMatchResultDto>();
            try
            {
                matches = await _aiJobMatchingService.GetCandidateJobMatchesAsync(candidateUserId) ?? new List<JobMatchResultDto>();
            }
            catch
            {
                // Graceful handling if job matching encounters an issue
            }

            decimal jobMatchScore = matches.Any() 
                ? Math.Round(matches.Take(5).Average(m => m.OverallMatchScore), 2) 
                : 50m;

            // 4. Calculate Skill Score
            decimal skillScore = CalculateSkillScore(candidate, bestCareer);

            // 5. Fetch Resume Intelligence
            ResumeHealthSummaryDto resumeHealth = await GetResumeHealthAsync(candidate.Id);
            decimal resumeScore = resumeHealth.HasResume ? resumeHealth.OverallScore : 0m;

            // 6. Overall Career Readiness Score (Deterministic Weighted Formula)
            // Profile 15%, Skill 25%, Career Path 20%, Job Match 20%, Resume 20%
            decimal overallScore = Math.Round(
                (profileScore * 0.15m) +
                (skillScore * 0.25m) +
                (careerPathScore * 0.20m) +
                (jobMatchScore * 0.20m) +
                (resumeScore * 0.20m), 2);

            if (overallScore > 100m) overallScore = 100m;
            if (overallScore < 0m) overallScore = 0m;

            string readinessLevel = DetermineReadinessLevel(overallScore);

            var readiness = new CareerReadinessDto
            {
                OverallScore = overallScore,
                ProfileScore = profileScore,
                SkillScore = skillScore,
                CareerPathScore = careerPathScore,
                JobMatchScore = jobMatchScore,
                ResumeScore = resumeScore,
                ReadinessLevel = readinessLevel
            };

            readiness.ScoreBreakdownExplanations.Add($"Overall Career Readiness: {overallScore:F0}% ({readinessLevel}).");
            readiness.ScoreBreakdownExplanations.Add($"Profile Completeness: {profileScore:F0}%.");
            readiness.ScoreBreakdownExplanations.Add($"Skill Readiness: {skillScore:F0}%.");
            readiness.ScoreBreakdownExplanations.Add($"Career Path Alignment: {careerPathScore:F0}%.");
            readiness.ScoreBreakdownExplanations.Add($"Job Market Match: {jobMatchScore:F0}%.");
            readiness.ScoreBreakdownExplanations.Add(resumeHealth.HasResume ? $"Resume ATS Readiness: {resumeScore:F0}%." : "No primary resume uploaded yet.");

            // 7. Extract Missing and Top Skills
            var missingSkills = bestCareer?.RequiredSkillsAnalysis
                .Where(s => s.Status == "MISSING")
                .Select(s => s.SkillName)
                .ToList() ?? new List<string>();

            if (!missingSkills.Any() && matches.Any())
            {
                missingSkills = matches.SelectMany(m => m.MissingSkills).Select(ms => ms.SkillName).Distinct().Take(4).ToList();
            }

            var topSkills = candidate.CandidateSkills.Select(cs => cs.Skill.Name).ToList();

            // 8. Top Career Blockers
            var blockers = BuildCareerBlockers(candidate, profileScore, skillScore, resumeHealth, missingSkills, targetRole);

            // 9. Next Best Action
            var nextBestAction = DetermineNextBestAction(blockers, missingSkills, targetRole, readinessLevel);

            // 10. 30 / 60 / 90 Day Action Plan
            var progressDict = candidate.ActionProgresses.ToDictionary(p => p.ActionKey, p => p);
            var actionPlan = BuildActionPlan(targetRole, missingSkills, resumeHealth, profileScore, progressDict);

            // Sync NextBestAction completion state if matching key in progressDict
            if (!string.IsNullOrEmpty(nextBestAction.ActionKey) && progressDict.TryGetValue(nextBestAction.ActionKey, out var nbaProgress))
            {
                nextBestAction.IsCompleted = nbaProgress.IsCompleted;
                nextBestAction.CompletedAt = nbaProgress.CompletedAt;
            }

            // 11. Goal Summary
            var goalSummary = new CareerGoalSummaryDto
            {
                CurrentRole = currentRole,
                TargetRole = targetRole,
                CareerCategory = bestCareer?.CareerLevel ?? "Engineering",
                ReadinessScore = careerPathScore,
                CompatibilityScore = bestCareer?.CompatibilityScore ?? 50m,
                TransitionDifficulty = bestCareer?.TransitionDifficulty ?? "MEDIUM",
                TopSkills = topSkills,
                MissingSkills = missingSkills,
                RecommendedTargetJobs = matches.Take(3).ToList()
            };

            // 12. Top Skills to Develop
            var topSkillsToDevelop = BuildTopSkillsToDevelop(missingSkills, candidate);

            return new CareerCommandCenterDto
            {
                Readiness = readiness,
                NextBestAction = nextBestAction,
                TopBlockers = blockers,
                ActionPlan = actionPlan,
                GoalSummary = goalSummary,
                TopSkillsToDevelop = topSkillsToDevelop,
                RecommendedJobs = matches.Take(4).ToList(),
                ResumeHealth = resumeHealth
            };
        }

        public async Task<List<CareerActionProgressDto>> GetActionProgressesAsync(Guid candidateUserId)
        {
            var candidate = await _context.CandidateProfiles
                .FirstOrDefaultAsync(c => c.UserId == candidateUserId);

            if (candidate == null)
            {
                throw new KeyNotFoundException("Candidate profile not found.");
            }

            var items = await _context.CareerActionProgresses
                .Where(cap => cap.CandidateProfileId == candidate.Id)
                .ToListAsync();

            return items.Select(i => new CareerActionProgressDto
            {
                Id = i.Id,
                CandidateProfileId = i.CandidateProfileId,
                ActionKey = i.ActionKey,
                IsCompleted = i.IsCompleted,
                CompletedAt = i.CompletedAt,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt
            }).ToList();
        }

        public async Task<CareerActionProgressDto> UpdateActionProgressAsync(Guid candidateUserId, string actionKey, bool isCompleted)
        {
            if (string.IsNullOrWhiteSpace(actionKey))
            {
                throw new ArgumentException("ActionKey cannot be empty.", nameof(actionKey));
            }

            var candidate = await _context.CandidateProfiles
                .FirstOrDefaultAsync(c => c.UserId == candidateUserId);

            if (candidate == null)
            {
                throw new KeyNotFoundException("Candidate profile not found.");
            }

            var existing = await _context.CareerActionProgresses
                .FirstOrDefaultAsync(cap => cap.CandidateProfileId == candidate.Id && cap.ActionKey == actionKey);

            if (existing == null)
            {
                existing = new CareerActionProgress
                {
                    CandidateProfileId = candidate.Id,
                    ActionKey = actionKey,
                    IsCompleted = isCompleted,
                    CompletedAt = isCompleted ? DateTime.UtcNow : null
                };
                _context.CareerActionProgresses.Add(existing);
            }
            else
            {
                existing.IsCompleted = isCompleted;
                existing.CompletedAt = isCompleted ? DateTime.UtcNow : null;
                existing.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return new CareerActionProgressDto
            {
                Id = existing.Id,
                CandidateProfileId = existing.CandidateProfileId,
                ActionKey = existing.ActionKey,
                IsCompleted = existing.IsCompleted,
                CompletedAt = existing.CompletedAt,
                CreatedAt = existing.CreatedAt,
                UpdatedAt = existing.UpdatedAt
            };
        }

        private decimal CalculateProfileCompletenessScore(CandidateProfile candidate)
        {
            decimal score = 0;
            if (!string.IsNullOrWhiteSpace(candidate.Bio) || !string.IsNullOrWhiteSpace(candidate.CareerSummary)) score += 15m;
            if (!string.IsNullOrWhiteSpace(candidate.CurrentJobTitle) || !string.IsNullOrWhiteSpace(candidate.Headline)) score += 15m;

            int skillCount = candidate.CandidateSkills.Count;
            if (skillCount >= 3) score += 20m;
            else if (skillCount >= 1) score += 10m;

            if (candidate.Educations.Any()) score += 15m;
            if (candidate.WorkExperiences.Any()) score += 15m;
            if (candidate.Certifications.Any() || candidate.CandidateLanguages.Any()) score += 10m;
            if (candidate.CareerPreference != null) score += 10m;

            return Math.Min(score, 100m);
        }

        private decimal CalculateSkillScore(CandidateProfile candidate, CareerRecommendationDto? bestCareer)
        {
            if (!candidate.CandidateSkills.Any()) return 0m;

            decimal totalPossible = candidate.CandidateSkills.Count * 4m;
            decimal totalEarned = candidate.CandidateSkills.Sum(cs => (int)cs.ProficiencyLevel);
            decimal proficiencyScore = (totalEarned / totalPossible) * 100m;

            if (bestCareer != null && bestCareer.SkillCoveragePercentage > 0)
            {
                return Math.Round((proficiencyScore * 0.5m) + (bestCareer.SkillCoveragePercentage * 0.5m), 2);
            }

            return Math.Round(proficiencyScore, 2);
        }

        private async Task<ResumeHealthSummaryDto> GetResumeHealthAsync(Guid candidateProfileId)
        {
            var dto = new ResumeHealthSummaryDto { HasResume = false };

            var primaryResume = await _context.Resumes
                .FirstOrDefaultAsync(r => r.CandidateProfileId == candidateProfileId && r.IsPrimary);

            if (primaryResume == null)
            {
                primaryResume = await _context.Resumes
                    .OrderByDescending(r => r.CreatedAt)
                    .FirstOrDefaultAsync(r => r.CandidateProfileId == candidateProfileId);
            }

            if (primaryResume == null) return dto;

            dto.HasResume = true;
            dto.PrimaryResumeId = primaryResume.Id;
            dto.FileName = primaryResume.OriginalFileName;

            var latestAnalysis = await _context.ResumeAnalyses
                .Where(ra => ra.ResumeId == primaryResume.Id)
                .OrderByDescending(ra => ra.CreatedAt)
                .FirstOrDefaultAsync();

            if (latestAnalysis != null)
            {
                dto.OverallScore = latestAnalysis.ATSScore;
                dto.StructureScore = latestAnalysis.StructureScore;
                dto.SkillsScore = latestAnalysis.SkillsScore;
                dto.KeywordsScore = latestAnalysis.KeywordScore;
                dto.ExperienceScore = latestAnalysis.ExperienceScore;
                dto.EducationScore = latestAnalysis.EducationScore;
                dto.CompletenessScore = latestAnalysis.CompletenessScore;
                dto.LastAnalyzedAt = latestAnalysis.CreatedAt;
            }
            else
            {
                // Default moderate score for uploaded but unanalyzed resume
                dto.OverallScore = 65m;
                dto.StructureScore = 65m;
                dto.SkillsScore = 60m;
                dto.KeywordsScore = 60m;
                dto.ExperienceScore = 70m;
                dto.EducationScore = 70m;
                dto.CompletenessScore = 60m;
            }

            return dto;
        }

        private string DetermineReadinessLevel(decimal overallScore)
        {
            if (overallScore >= 85m) return "Highly Competitive";
            if (overallScore >= 70m) return "Job Ready";
            if (overallScore >= 50m) return "Developing";
            return "Beginner";
        }

        private List<CareerBlockerDto> BuildCareerBlockers(
            CandidateProfile candidate,
            decimal profileScore,
            decimal skillScore,
            ResumeHealthSummaryDto resumeHealth,
            List<string> missingSkills,
            string targetRole)
        {
            var list = new List<CareerBlockerDto>();

            // 1. Resume Blocker
            if (!resumeHealth.HasResume)
            {
                list.Add(new CareerBlockerDto
                {
                    Id = "blocker-resume-missing",
                    Title = "No Primary Resume Uploaded",
                    Description = "You have not uploaded a resume to analyze ATS keyword match and formatting.",
                    Severity = "Critical",
                    ImpactScore = 90m,
                    RelatedArea = "Resume",
                    RecommendedAction = "Upload your resume in PDF/DOCX format for instant ATS feedback.",
                    RelatedModule = "/candidate/resume"
                });
            }
            else if (resumeHealth.OverallScore < 70m)
            {
                list.Add(new CareerBlockerDto
                {
                    Id = "blocker-resume-ats-low",
                    Title = "Weak Resume ATS Alignment",
                    Description = $"Your resume ATS score is {resumeHealth.OverallScore:F0}%. Automated scanners may filter out your profile.",
                    Severity = "High",
                    ImpactScore = 80m,
                    RelatedArea = "Resume",
                    RecommendedAction = "Optimize resume keywords, layout, and targeted skills.",
                    RelatedModule = "/candidate/resume"
                });
            }

            // 2. Missing Skills Blocker
            if (missingSkills.Any())
            {
                var topMissing = missingSkills.First();
                list.Add(new CareerBlockerDto
                {
                    Id = $"blocker-skill-{topMissing.ToLower().Replace(" ", "-")}",
                    Title = $"Missing Key Skill: {topMissing}",
                    Description = $"Target roles for {targetRole} frequently require proficiency in {topMissing}.",
                    Severity = "High",
                    ImpactScore = 85m,
                    RelatedArea = "Skills",
                    RecommendedAction = $"Add {topMissing} to your skills and complete a quick project module.",
                    RelatedModule = "/candidate/skills"
                });
            }

            // 3. Incomplete Profile Blocker
            if (profileScore < 70m)
            {
                list.Add(new CareerBlockerDto
                {
                    Id = "blocker-profile-incomplete",
                    Title = "Incomplete Professional Profile",
                    Description = $"Your profile completeness is currently at {profileScore:F0}%. Key details are missing.",
                    Severity = "High",
                    ImpactScore = 75m,
                    RelatedArea = "Profile",
                    RecommendedAction = "Add work experiences, education history, and career summary.",
                    RelatedModule = "/candidate/profile"
                });
            }

            // 4. Experience Gap Blocker
            int expYears = candidate.YearsOfExperience ?? 0;
            if (expYears < 2)
            {
                list.Add(new CareerBlockerDto
                {
                    Id = "blocker-experience-gap",
                    Title = "Early-Career Experience Gap",
                    Description = $"Your profile lists {expYears} year(s) of experience. Demonstrating practical project work is crucial.",
                    Severity = "Medium",
                    ImpactScore = 65m,
                    RelatedArea = "Experience",
                    RecommendedAction = "Highlight open-source or portfolio projects in your profile.",
                    RelatedModule = "/candidate/profile"
                });
            }

            // 5. Career Preferences Blocker
            if (candidate.CareerPreference == null)
            {
                list.Add(new CareerBlockerDto
                {
                    Id = "blocker-preferences-missing",
                    Title = "Career Preferences Not Set",
                    Description = "Without preferred locations, job roles, or salary expectations, AI matches may be inaccurate.",
                    Severity = "Low",
                    ImpactScore = 40m,
                    RelatedArea = "Career Direction",
                    RecommendedAction = "Set your career preferences in your profile.",
                    RelatedModule = "/candidate/profile"
                });
            }

            return list.OrderByDescending(b => b.ImpactScore).ToList();
        }

        private CareerActionDto DetermineNextBestAction(
            List<CareerBlockerDto> blockers,
            List<string> missingSkills,
            string targetRole,
            string readinessLevel)
        {
            var topBlocker = blockers.FirstOrDefault();

            if (topBlocker != null)
            {
                return new CareerActionDto
                {
                    Id = "next-best-action-1",
                    ActionKey = topBlocker.Id.Replace("blocker-", "action-"),
                    Title = topBlocker.RecommendedAction,
                    Description = topBlocker.Description,
                    Category = topBlocker.RelatedArea switch
                    {
                        "Resume" => "Resume",
                        "Skills" => "Skill Development",
                        "Profile" => "Profile",
                        "Experience" => "Experience",
                        _ => "Career Planning"
                    },
                    Priority = topBlocker.Severity,
                    EstimatedImpact = "Critical Impact",
                    TargetDays = 30,
                    EstimatedEffort = "1-2 hours",
                    RelatedModule = topBlocker.RelatedModule,
                    Reason = $"Addressing '{topBlocker.Title}' will yield the highest immediate increase in your career readiness."
                };
            }

            // Fallback default next best action
            if (missingSkills.Any())
            {
                var skill = missingSkills.First();
                return new CareerActionDto
                {
                    Id = "next-best-action-skill",
                    ActionKey = $"action-learn-{skill.ToLower().Replace(" ", "-")}",
                    Title = $"Master {skill} fundamentals",
                    Description = $"Adding {skill} proficiency will unlock high-compatibility {targetRole} job matches.",
                    Category = "Skill Development",
                    Priority = "High",
                    EstimatedImpact = "+15% Match Score",
                    TargetDays = 30,
                    EstimatedEffort = "1 week",
                    RelatedSkill = skill,
                    RelatedModule = "/candidate/skills",
                    Reason = $"Target roles frequently list {skill} as a core requirement."
                };
            }

            return new CareerActionDto
            {
                Id = "next-best-action-apply",
                ActionKey = "action-apply-target-jobs",
                Title = $"Apply to top {targetRole} openings",
                Description = "Your profile and resume scores show strong readiness. Start submitting applications.",
                Category = "Job Search",
                Priority = "Medium",
                EstimatedImpact = "Job Applications",
                TargetDays = 30,
                EstimatedEffort = "Ongoing",
                RelatedModule = "/candidate/jobs",
                Reason = "Your profile is job ready. Active job applications will accelerate your career move."
            };
        }

        private CareerActionPlanDto BuildActionPlan(
            string targetRole,
            List<string> missingSkills,
            ResumeHealthSummaryDto resumeHealth,
            decimal profileScore,
            Dictionary<string, CareerActionProgress> progressDict)
        {
            var plan = new CareerActionPlanDto();

            string primaryMissingSkill = missingSkills.FirstOrDefault() ?? ".NET";
            string secondaryMissingSkill = missingSkills.Skip(1).FirstOrDefault() ?? "Cloud Deployment";

            // 30 DAYS (Foundation)
            var day30 = new List<CareerActionDto>
            {
                new CareerActionDto
                {
                    Id = "plan-action-1",
                    ActionKey = "action-profile-complete",
                    Title = "Complete Professional Profile",
                    Description = "Fill in bio, current role, education, and career preferences.",
                    Category = "Profile",
                    Priority = "High",
                    TargetDays = 30,
                    EstimatedEffort = "1 hour",
                    EstimatedImpact = "High",
                    RelatedModule = "/candidate/profile"
                },
                new CareerActionDto
                {
                    Id = "plan-action-2",
                    ActionKey = "action-resume-ats-optimize",
                    Title = "Optimize Resume for ATS Scanners",
                    Description = "Upload latest resume and refine skills and keywords.",
                    Category = "Resume",
                    Priority = "High",
                    TargetDays = 30,
                    EstimatedEffort = "2 hours",
                    EstimatedImpact = "High",
                    RelatedModule = "/candidate/resume"
                },
                new CareerActionDto
                {
                    Id = "plan-action-3",
                    ActionKey = $"action-skill-{primaryMissingSkill.ToLower().Replace(" ", "-")}",
                    Title = $"Acquire Foundational {primaryMissingSkill}",
                    Description = $"Learn core concepts of {primaryMissingSkill} to bridge initial target role gap.",
                    Category = "Skill Development",
                    Priority = "High",
                    TargetDays = 30,
                    EstimatedEffort = "2 weeks",
                    EstimatedImpact = "High",
                    RelatedSkill = primaryMissingSkill,
                    RelatedModule = "/candidate/skills"
                }
            };

            // 60 DAYS (Growth & Portfolio)
            var day60 = new List<CareerActionDto>
            {
                new CareerActionDto
                {
                    Id = "plan-action-4",
                    ActionKey = $"action-skill-{secondaryMissingSkill.ToLower().Replace(" ", "-")}",
                    Title = $"Master {secondaryMissingSkill}",
                    Description = $"Gain proficiency in {secondaryMissingSkill} to strengthen technical stack.",
                    Category = "Skill Development",
                    Priority = "Medium",
                    TargetDays = 60,
                    EstimatedEffort = "3 weeks",
                    EstimatedImpact = "Medium",
                    RelatedSkill = secondaryMissingSkill,
                    RelatedModule = "/candidate/skills"
                },
                new CareerActionDto
                {
                    Id = "plan-action-5",
                    ActionKey = "action-build-portfolio-project",
                    Title = "Build Hands-on Portfolio Project",
                    Description = $"Create a full project demonstrating {primaryMissingSkill} and {secondaryMissingSkill}.",
                    Category = "Portfolio",
                    Priority = "Medium",
                    TargetDays = 60,
                    EstimatedEffort = "3-4 weeks",
                    EstimatedImpact = "High",
                    RelatedModule = "/candidate/profile"
                },
                new CareerActionDto
                {
                    Id = "plan-action-6",
                    ActionKey = "action-tailor-target-resumes",
                    Title = "Tailor Resume for Specific Roles",
                    Description = "Use job-specific resume analysis for target applications.",
                    Category = "Resume",
                    Priority = "Medium",
                    TargetDays = 60,
                    EstimatedEffort = "1 week",
                    EstimatedImpact = "Medium",
                    RelatedModule = "/candidate/resume"
                }
            };

            // 90 DAYS (Job Readiness & Application Strategy)
            var day90 = new List<CareerActionDto>
            {
                new CareerActionDto
                {
                    Id = "plan-action-7",
                    ActionKey = "action-apply-top-matches",
                    Title = $"Submit Applications for {targetRole} Roles",
                    Description = "Target jobs with >75% compatibility score.",
                    Category = "Job Search",
                    Priority = "High",
                    TargetDays = 90,
                    EstimatedEffort = "Ongoing",
                    EstimatedImpact = "High",
                    RelatedModule = "/candidate/jobs"
                },
                new CareerActionDto
                {
                    Id = "plan-action-8",
                    ActionKey = "action-interview-prep-technical",
                    Title = "Prepare Technical Interview Q&A",
                    Description = "Practice system design and technical domain questions.",
                    Category = "Interview Preparation",
                    Priority = "Medium",
                    TargetDays = 90,
                    EstimatedEffort = "1 week",
                    EstimatedImpact = "Medium",
                    RelatedModule = "/candidate/intelligence"
                }
            };

            // Sync completion status from DB
            ApplyProgress(day30, progressDict);
            ApplyProgress(day60, progressDict);
            ApplyProgress(day90, progressDict);

            plan.ThirtyDayActions = day30;
            plan.SixtyDayActions = day60;
            plan.NinetyDayActions = day90;

            int allActionsCount = day30.Count + day60.Count + day90.Count;
            int completedCount = day30.Count(a => a.IsCompleted) + day60.Count(a => a.IsCompleted) + day90.Count(a => a.IsCompleted);

            plan.TotalActions = allActionsCount;
            plan.CompletedActions = completedCount;
            plan.ProgressPercentage = allActionsCount > 0 
                ? Math.Round(((decimal)completedCount / allActionsCount) * 100m, 1) 
                : 0m;

            return plan;
        }

        private void ApplyProgress(List<CareerActionDto> actions, Dictionary<string, CareerActionProgress> progressDict)
        {
            foreach (var action in actions)
            {
                if (progressDict.TryGetValue(action.ActionKey, out var progress))
                {
                    action.IsCompleted = progress.IsCompleted;
                    action.CompletedAt = progress.CompletedAt;
                }
            }
        }

        private List<SkillRoadmapItemDto> BuildTopSkillsToDevelop(List<string> missingSkills, CandidateProfile candidate)
        {
            var list = new List<SkillRoadmapItemDto>();
            int step = 1;

            foreach (var skill in missingSkills.Take(5))
            {
                list.Add(new SkillRoadmapItemDto
                {
                    StepNumber = step++,
                    SkillName = skill,
                    CurrentLevel = "None",
                    TargetLevel = "Intermediate",
                    Priority = step <= 2 ? "High" : "Medium",
                    Severity = step <= 2 ? "High" : "Medium",
                    Reason = $"Required skill gap for targeted career path.",
                    Status = "Missing"
                });
            }

            // Fill up to 5 if needed
            if (list.Count < 5)
            {
                var developing = candidate.CandidateSkills
                    .Where(cs => cs.ProficiencyLevel < ProficiencyLevel.Advanced)
                    .Take(5 - list.Count);

                foreach (var dev in developing)
                {
                    list.Add(new SkillRoadmapItemDto
                    {
                        StepNumber = step++,
                        SkillId = dev.SkillId,
                        SkillName = dev.Skill.Name,
                        CurrentLevel = dev.ProficiencyLevel.ToString(),
                        TargetLevel = "Advanced",
                        Priority = "Medium",
                        Severity = "Low",
                        Reason = "Level up existing skill to expert proficiency.",
                        Status = "Developing"
                    });
                }
            }

            return list;
        }
    }
}
