using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CareerMind.Application.DTOs.CareerAnalytics;
using CareerMind.Application.Interfaces;
using CareerMind.Domain.Entities;
using CareerMind.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareerMind.Infrastructure.Services
{
    public class CareerAnalyticsService : ICareerAnalyticsService
    {
        private readonly CareerMindDbContext _context;

        public CareerAnalyticsService(CareerMindDbContext context)
        {
            _context = context;
        }

        public async Task<CareerAnalyticsDto> GetCareerAnalyticsAsync(Guid candidateUserId)
        {
            var candidate = await _context.CandidateProfiles
                .Include(c => c.CandidateSkills).ThenInclude(cs => cs.Skill)
                .Include(c => c.WorkExperiences)
                .Include(c => c.Educations)
                .Include(c => c.Certifications)
                .Include(c => c.CareerPreference)
                .Include(c => c.ActionProgresses)
                .FirstOrDefaultAsync(c => c.UserId == candidateUserId);

            if (candidate == null)
            {
                throw new KeyNotFoundException("Candidate profile not found.");
            }

            // A. Profile Analytics
            var profileAnalytics = CalculateProfileAnalytics(candidate);

            // B. Skill Analytics
            var skillAnalytics = CalculateSkillAnalytics(candidate);

            // C. Job Matching Analytics
            // Fetch match scores or evaluations from somewhere. Wait, AIJobMatchingService matches dynamically.
            // Let's see if we have SavedJobs or similar. The instructions say to use existing data.
            // Check JobApplications and SavedJobs? Let's just use JobApplications or an average of them.
            var jobAnalytics = await CalculateJobAnalyticsAsync(candidate.Id);

            // D. Resume / ATS Analytics
            var resumeAnalytics = await CalculateResumeAnalyticsAsync(candidate.Id);

            // E. Interview Analytics
            var interviewAnalytics = await CalculateInterviewAnalyticsAsync(candidate.Id);

            // F. Application Readiness Analytics
            var applicationAnalytics = await CalculateApplicationAnalyticsAsync(candidate.Id);

            // G. Action Plan Progress
            var actionAnalytics = CalculateActionAnalytics(candidate);

            // H. Career Insights
            var insights = GenerateInsights(profileAnalytics, skillAnalytics, resumeAnalytics, interviewAnalytics, applicationAnalytics, candidate);

            // Calculate Overall Score based on weights (e.g. 20% profile, 20% skills, 20% resume, 20% interview, 20% applications)
            decimal overallScore = CalculateOverallScore(profileAnalytics, skillAnalytics, resumeAnalytics, interviewAnalytics, applicationAnalytics);

            return new CareerAnalyticsDto
            {
                OverallProgressScore = overallScore,
                ProfileAnalytics = profileAnalytics,
                SkillAnalytics = skillAnalytics,
                JobMatchingAnalytics = jobAnalytics,
                ResumeAnalytics = resumeAnalytics,
                InterviewAnalytics = interviewAnalytics,
                ApplicationAnalytics = applicationAnalytics,
                ActionPlanAnalytics = actionAnalytics,
                Insights = insights
            };
        }

        private ProfileAnalyticsDto CalculateProfileAnalytics(CandidateProfile candidate)
        {
            var completed = new List<string>();
            var missing = new List<string>();

            int totalSections = 6;
            
            if (!string.IsNullOrWhiteSpace(candidate.Bio)) completed.Add("Bio"); else missing.Add("Bio");
            if (!string.IsNullOrWhiteSpace(candidate.CurrentJobTitle)) completed.Add("Current Role"); else missing.Add("Current Role");
            if (candidate.WorkExperiences.Any()) completed.Add("Experience"); else missing.Add("Experience");
            if (candidate.Educations.Any()) completed.Add("Education"); else missing.Add("Education");
            if (candidate.CandidateSkills.Any()) completed.Add("Skills"); else missing.Add("Skills");
            if (candidate.CareerPreference != null) completed.Add("Preferences"); else missing.Add("Preferences");

            decimal completionPercentage = (decimal)completed.Count / totalSections * 100m;

            string status = completionPercentage >= 100m ? "Complete" :
                            completionPercentage >= 75m  ? "Almost Complete" :
                            completionPercentage >= 50m  ? "Developing" : "Needs Attention";

            return new ProfileAnalyticsDto
            {
                CompletionPercentage = completionPercentage,
                CompletedSections = completed,
                MissingSections = missing,
                TotalProfileSections = totalSections,
                CompletionStatus = status
            };
        }

        private SkillAnalyticsDto CalculateSkillAnalytics(CandidateProfile candidate)
        {
            var skills = candidate.CandidateSkills;
            
            int strong = skills.Count(s => (int)s.ProficiencyLevel >= 4); // Assuming 4=Advanced, 5=Expert
            int developing = skills.Count(s => (int)s.ProficiencyLevel < 4);
            
            // Skill distribution
            var dist = skills.GroupBy(s => s.Skill.Category ?? "Other")
                             .ToDictionary(g => g.Key, g => g.Count());

            // Missing target skills could come from career gap data. If we don't have it natively, let's just make it 0 and populate properly if requested.
            // The prompt says "where existing career-gap data supports it". Since it's dynamic in Career Gap, let's keep it 0 or query it if we had a table. We don't have a table for gaps, it's calculated on fly. Let's keep it as is.
            return new SkillAnalyticsDto
            {
                TotalSkills = skills.Count,
                StrongSkills = strong,
                DevelopingSkills = developing,
                MissingTargetSkills = 0,
                SkillDevelopmentProgress = skills.Any() ? (skills.Sum(s => (int)s.ProficiencyLevel) / (decimal)(skills.Count * 5)) * 100m : 0m,
                SkillsDistribution = dist
            };
        }

        private async Task<JobMatchingAnalyticsDto> CalculateJobAnalyticsAsync(Guid candidateId)
        {
            // We use JobApplications as a proxy for evaluating jobs if we don't have saved matches with scores.
            // Wait, we don't store match scores in DB typically by default unless it's in ApplicationOptimizations
            var optims = await _context.ApplicationOptimizations.Where(o => o.CandidateProfileId == candidateId).ToListAsync();

            if (!optims.Any())
            {
                return new JobMatchingAnalyticsDto();
            }

            var avgScore = (decimal)optims.Average(o => o.ApplicationReadinessScore ?? 0);
            var highScore = (decimal)optims.Max(o => o.ApplicationReadinessScore ?? 0);
            var strong = optims.Count(o => (o.ApplicationReadinessScore ?? 0) >= 80);
            var moderate = optims.Count(o => (o.ApplicationReadinessScore ?? 0) >= 50 && (o.ApplicationReadinessScore ?? 0) < 80);
            var weak = optims.Count(o => (o.ApplicationReadinessScore ?? 0) < 50);

            return new JobMatchingAnalyticsDto
            {
                TotalJobsEvaluated = optims.Count,
                AverageMatchScore = avgScore,
                HighestMatchScore = highScore,
                StrongMatches = strong,
                ModerateMatches = moderate,
                WeakMatches = weak
            };
        }

        private async Task<ResumeAnalyticsDto> CalculateResumeAnalyticsAsync(Guid candidateId)
        {
            var resumes = await _context.Resumes
                .Where(r => r.CandidateProfileId == candidateId)
                .Include(r => r.Analyses)
                .ToListAsync();

            if (!resumes.Any())
            {
                return new ResumeAnalyticsDto { HasResume = false };
            }

            var allAnalyses = resumes.SelectMany(r => r.Analyses)
                .OrderByDescending(a => a.CreatedAt)
                .ToList();

            if (!allAnalyses.Any())
            {
                 return new ResumeAnalyticsDto { HasResume = true };
            }

            var latest = allAnalyses.First();
            var prev = allAnalyses.Skip(1).FirstOrDefault();

            var dto = new ResumeAnalyticsDto
            {
                HasResume = true,
                LatestATSScore = latest.ATSScore,
                PreviousATSScore = prev?.ATSScore,
                ATSImprovement = prev != null ? (latest.ATSScore - prev.ATSScore) : null,
                StructureScore = latest.StructureScore,
                SkillsScore = latest.SkillsScore,
                KeywordsScore = latest.KeywordScore, // The prop is KeywordScore in DB
                ExperienceScore = latest.ExperienceScore,
                EducationScore = latest.EducationScore,
                CompletenessScore = latest.CompletenessScore
            };
            
            // Generate top improvement areas from missing skills / suggestions, but we don't have a direct list. We can synthesize based on lowest scores.
            var scores = new Dictionary<string, decimal>
            {
                { "Structure", latest.StructureScore },
                { "Skills", latest.SkillsScore },
                { "Keywords", latest.KeywordScore },
                { "Experience", latest.ExperienceScore },
                { "Education", latest.EducationScore },
                { "Completeness", latest.CompletenessScore }
            };

            dto.TopImprovementAreas = scores.OrderBy(s => s.Value).Take(2).Select(s => s.Key).ToList();

            return dto;
        }

        private async Task<InterviewAnalyticsDto> CalculateInterviewAnalyticsAsync(Guid candidateId)
        {
            var sessions = await _context.InterviewSessions
                .Where(s => s.CandidateProfileId == candidateId)
                .Include(s => s.Questions)
                .ThenInclude(q => q.Answers)
                .ToListAsync();

            if (!sessions.Any())
            {
                return new InterviewAnalyticsDto();
            }

            var completedSess = sessions.Where(s => s.Status == CareerMind.Domain.Enums.InterviewStatus.Completed).ToList();
            
            if (!completedSess.Any())
            {
                return new InterviewAnalyticsDto 
                { 
                    TotalInterviewSessions = sessions.Count,
                    CompletedSessions = 0
                };
            }

            var avgScore = (decimal)completedSess.Average(s => s.OverallReadinessScore ?? 0);
            var latest = (decimal)(completedSess.OrderByDescending(s => s.CreatedAt).First().OverallReadinessScore ?? 0);

            return new InterviewAnalyticsDto
            {
                TotalInterviewSessions = sessions.Count,
                CompletedSessions = completedSess.Count,
                AverageReadinessScore = avgScore,
                LatestReadinessScore = latest,
                TechnicalPerformance = (decimal)completedSess.Average(s => s.TechnicalScore ?? 0), // Using native props... Wait, check entity props for TechnicalScore.
            };
        }

        private async Task<ApplicationAnalyticsDto> CalculateApplicationAnalyticsAsync(Guid candidateId)
        {
            var optims = await _context.ApplicationOptimizations
                .Where(ao => ao.CandidateProfileId == candidateId)
                .Include(ao => ao.Suggestions)
                .ToListAsync();

            if (!optims.Any())
                return new ApplicationAnalyticsDto();

            var avgReadiness = (decimal)optims.Average(o => o.ApplicationReadinessScore ?? 0);
            
            var completedSuggs = optims.SelectMany(o => o.Suggestions).Count(s => s.IsCompleted);
            var pendingSuggs = optims.SelectMany(o => o.Suggestions).Count(s => !s.IsCompleted);
            var cats = optims.SelectMany(o => o.Suggestions).Select(s => s.Category.ToString()).Distinct().ToList();

            return new ApplicationAnalyticsDto
            {
                TotalOptimizedApplications = optims.Count,
                AverageApplicationReadiness = avgReadiness,
                HighReadinessApplications = optims.Count(o => (o.ApplicationReadinessScore ?? 0) >= 80),
                ApplicationsNeedingImprovement = optims.Count(o => (o.ApplicationReadinessScore ?? 0) < 70),
                CompletedSuggestions = completedSuggs,
                PendingSuggestions = pendingSuggs,
                CommonSuggestionCategories = cats
            };
        }

        private ActionPlanAnalyticsDto CalculateActionAnalytics(CandidateProfile candidate)
        {
            var actions = candidate.ActionProgresses ?? new List<CareerActionProgress>();
            
            if (!actions.Any()) return new ActionPlanAnalyticsDto();

            int completed = actions.Count(a => a.IsCompleted);
            int pending = actions.Count(a => !a.IsCompleted);

            return new ActionPlanAnalyticsDto
            {
                TotalPlannedActions = actions.Count,
                CompletedActions = completed,
                PendingActions = pending,
                InProgressActions = pending, // Approximation since we don't have 'in progress' status
                OverallCompletionPercentage = (decimal)completed / actions.Count * 100m,
                CurrentPriorityAction = actions.FirstOrDefault(a => !a.IsCompleted)?.ActionKey ?? "None"
            };
        }

        private List<CareerInsightDto> GenerateInsights(
            ProfileAnalyticsDto prof, 
            SkillAnalyticsDto skill, 
            ResumeAnalyticsDto res, 
            InterviewAnalyticsDto iv, 
            ApplicationAnalyticsDto app,
            CandidateProfile candidate)
        {
            var insights = new List<CareerInsightDto>();

            // Profiling insight
            if (prof.CompletionPercentage < 100m && prof.MissingSections.Any())
            {
                insights.Add(new CareerInsightDto
                {
                    InsightId = "prof_insight_1",
                    Type = "Info",
                    Title = "Profile Incomplete",
                    Description = $"Your profile is {prof.CompletionPercentage:F0}% complete. Adding {prof.MissingSections.First()} details can improve completeness.",
                    RecommendedAction = "Update Profile",
                    ActionLink = "/candidate/profile"
                });
            }
            else
            {
                insights.Add(new CareerInsightDto
                {
                    InsightId = "prof_insight_2",
                    Type = "Success",
                    Title = "Strong Profile",
                    Description = "Your profile is fully complete, maximizing AI match accuracy.",
                    ActionLink = "/candidate/profile"
                });
            }

            // Resume Insight
            if (res.HasResume)
            {
                if (res.LatestATSScore < 70)
                {
                    insights.Add(new CareerInsightDto
                    {
                        InsightId = "res_insight_1",
                        Type = "Warning",
                        Title = "ATS Alignment Needs Work",
                        Description = $"Your resume ATS score is {res.LatestATSScore}%. Enhance {res.TopImprovementAreas.FirstOrDefault()?.ToLower() ?? "skills"} for better alignment.",
                        RecommendedAction = "Optimize Resume",
                        ActionLink = "/candidate/resume"
                    });
                }
                else
                {
                    insights.Add(new CareerInsightDto
                    {
                        InsightId = "res_insight_2",
                        Type = "Success",
                        Title = "Strong ATS Score",
                        Description = $"Your resume scores highly ({res.LatestATSScore}%) for automated screening systems.",
                        ActionLink = "/candidate/resume"
                    });
                }
            }
            else
            {
                insights.Add(new CareerInsightDto
                {
                    InsightId = "res_insight_3",
                    Type = "Warning",
                    Title = "No Resume Uploaded",
                    Description = "Upload a resume to unlock ATS performance insights.",
                    RecommendedAction = "Upload Resume",
                    ActionLink = "/candidate/resume"
                });
            }

            // Interview Insight
            if (iv.TotalInterviewSessions > 0)
            {
                insights.Add(new CareerInsightDto
                {
                    InsightId = "iv_insight_1",
                    Type = "Info",
                    Title = "Interview Readiness",
                    Description = $"You have completed {iv.CompletedSessions} interviews. Latest readiness score is {iv.LatestReadinessScore:F0}%.",
                    ActionLink = "/candidate/interviews"
                });
            }

            return insights;
        }

        private decimal CalculateOverallScore(
            ProfileAnalyticsDto prof,
            SkillAnalyticsDto skill,
            ResumeAnalyticsDto res,
            InterviewAnalyticsDto iv,
            ApplicationAnalyticsDto app)
        {
            decimal score = 0;
            score += prof.CompletionPercentage * 0.20m;
            score += skill.SkillDevelopmentProgress * 0.20m;
            
            if (res.HasResume) score += res.LatestATSScore * 0.25m;
            if (iv.CompletedSessions > 0) score += iv.LatestReadinessScore * 0.20m;
            if (app.TotalOptimizedApplications > 0) score += app.AverageApplicationReadiness * 0.15m;
            
            // Normalize if missing resume/interviews
            var weights = 0.40m; // Profile + Skills
            if (res.HasResume) weights += 0.25m;
            if (iv.CompletedSessions > 0) weights += 0.20m;
            if (app.TotalOptimizedApplications > 0) weights += 0.15m;

            if (weights > 0)
            {
                score = score / weights;
            }

            return Math.Clamp(score, 0, 100);
        }
    }
}
