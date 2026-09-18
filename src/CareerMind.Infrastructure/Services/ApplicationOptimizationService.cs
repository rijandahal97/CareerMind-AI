using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CareerMind.Application.DTOs.ApplicationOptimization;
using CareerMind.Application.Interfaces;
using CareerMind.Domain.Entities;
using CareerMind.Domain.Enums;
using CareerMind.Infrastructure.Data;

namespace CareerMind.Infrastructure.Services
{
    public class ApplicationOptimizationService : IApplicationOptimizationService
    {
        private readonly CareerMindDbContext _context;

        public ApplicationOptimizationService(CareerMindDbContext context)
        {
            _context = context;
        }

        public async Task<ApplicationOptimizationDto> OptimizeApplicationAsync(Guid candidateProfileId, Guid jobId)
        {
            var job = await _context.Jobs
                .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
                .Include(j => j.EmployerProfile)
                .FirstOrDefaultAsync(j => j.Id == jobId);
            
            if (job == null) throw new ArgumentException("Job not found.");

            var candidate = await _context.CandidateProfiles
                .Include(c => c.CandidateSkills).ThenInclude(cs => cs.Skill)
                .Include(c => c.Resumes).ThenInclude(r => r.Analyses)
                .Include(c => c.Educations)
                .Include(c => c.WorkExperiences)
                .Include(c => c.InterviewSessions)
                .FirstOrDefaultAsync(c => c.Id == candidateProfileId);

            if (candidate == null) throw new ArgumentException("Candidate not found.");

            // Check if active optimization exists
            var optimization = await _context.ApplicationOptimizations
                .Include(o => o.Suggestions)
                .FirstOrDefaultAsync(o => o.CandidateProfileId == candidateProfileId && o.JobId == jobId);

            if (optimization == null)
            {
                optimization = new ApplicationOptimization
                {
                    CandidateProfileId = candidateProfileId,
                    JobId = jobId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.ApplicationOptimizations.Add(optimization);
            }
            else
            {
                optimization.UpdatedAt = DateTime.UtcNow;
                optimization.OptimizationVersion++;
                
                // Clear existing suggestions to regenerate cleanly through EF tracking
                var oldSuggestions = optimization.Suggestions.ToList();
                _context.ApplicationOptimizationSuggestions.RemoveRange(oldSuggestions);
                optimization.Suggestions.Clear();
            }

            var latestResume = candidate.Resumes.OrderByDescending(r => r.UploadedAt).FirstOrDefault();
            optimization.ResumeId = latestResume?.Id;

            // Compute Alignments
            CalculateProfileAlignment(candidate, optimization);
            CalculateSkillAlignment(candidate, job, optimization);
            CalculateExperienceAlignment(candidate, job, optimization);
            CalculateEducationAlignment(candidate, job, optimization);
            CalculateResumeAlignment(latestResume, optimization);
            CalculateInterviewReadiness(candidate, jobId, optimization);

            // Compute Readiness Score
            CalculateReadinessScore(optimization);
            DetermineApplicationStatus(optimization);

            // Generate Content
            GenerateOptimizedProfileContent(candidate, job, optimization);
            GenerateCoverLetter(candidate, job, optimization);
            
            await _context.SaveChangesAsync();
            return MapToDto(optimization);
        }

        public async Task<IEnumerable<ApplicationOptimizationSummaryDto>> GetApplicationOptimizationsAsync(Guid candidateProfileId)
        {
            var opts = await _context.ApplicationOptimizations
                .Include(o => o.Job).ThenInclude(j => j.EmployerProfile)
                .Where(o => o.CandidateProfileId == candidateProfileId)
                .ToListAsync();

            return opts.Select(o => new ApplicationOptimizationSummaryDto
            {
                Id = o.Id,
                JobId = o.JobId,
                JobTitle = o.Job?.Title ?? "",
                EmployerName = o.Job?.EmployerProfile?.CompanyName ?? "",
                ApplicationReadinessScore = o.ApplicationReadinessScore,
                ApplicationStatus = o.ApplicationStatus,
                CreatedAt = o.CreatedAt
            });
        }

        public async Task<ApplicationOptimizationDto?> GetApplicationOptimizationAsync(Guid candidateProfileId, Guid jobId)
        {
            var opt = await _context.ApplicationOptimizations
                .Include(o => o.Suggestions)
                .FirstOrDefaultAsync(o => o.CandidateProfileId == candidateProfileId && o.JobId == jobId);
            
            if (opt == null) return null;
            return MapToDto(opt);
        }

        public async Task<ApplicationReadinessDto?> GetApplicationReadinessAsync(Guid candidateProfileId, Guid jobId)
        {
            var opt = await _context.ApplicationOptimizations
                .FirstOrDefaultAsync(o => o.CandidateProfileId == candidateProfileId && o.JobId == jobId);

            if (opt == null) return null;

            var missingBlockers = new List<string>();
            if (opt.ResumeId == null) missingBlockers.Add("Upload a resume to unlock resume alignment.");
            if (!opt.InterviewReadinessScore.HasValue) missingBlockers.Add("No completed interview session yet.");

            return new ApplicationReadinessDto
            {
                JobId = opt.JobId,
                Score = opt.ApplicationReadinessScore ?? 0,
                ReadinessLevel = GetReadinessLevel(opt.ApplicationReadinessScore ?? 0),
                ProfileAlignmentScore = opt.ProfileAlignmentScore ?? 0,
                SkillAlignmentScore = opt.SkillAlignmentScore ?? 0,
                ExperienceAlignmentScore = opt.ExperienceAlignmentScore ?? 0,
                EducationAlignmentScore = opt.EducationAlignmentScore ?? 0,
                ResumeAlignmentScore = opt.ResumeAlignmentScore ?? 0,
                InterviewReadinessScore = opt.InterviewReadinessScore ?? 0,
                MissingDataBlockers = string.Join(" ", missingBlockers)
            };
        }

        public async Task<ApplicationCoverLetterDto?> GetApplicationCoverLetterAsync(Guid candidateProfileId, Guid jobId)
        {
            var opt = await _context.ApplicationOptimizations
                .FirstOrDefaultAsync(o => o.CandidateProfileId == candidateProfileId && o.JobId == jobId);
            
            if (opt == null) return null;

            return new ApplicationCoverLetterDto
            {
                JobId = opt.JobId,
                CoverLetter = opt.CoverLetter ?? ""
            };
        }

        public async Task<bool> UpdateSuggestionAsync(Guid candidateProfileId, Guid jobId, Guid suggestionId, UpdateApplicationSuggestionRequest request)
        {
            var suggestion = await _context.ApplicationOptimizationSuggestions
                .Include(s => s.ApplicationOptimization)
                .FirstOrDefaultAsync(s => s.Id == suggestionId && s.ApplicationOptimization.CandidateProfileId == candidateProfileId && s.ApplicationOptimization.JobId == jobId);
            
            if (suggestion == null) return false;
            suggestion.IsCompleted = request.IsCompleted;
            await _context.SaveChangesAsync();
            return true;
        }

        private void CalculateProfileAlignment(CandidateProfile candidate, ApplicationOptimization opt)
        {
            double maxScore = 100;
            double score = 0;
            if (!string.IsNullOrEmpty(candidate.Headline)) score += 20;
            else opt.Suggestions.Add(new ApplicationOptimizationSuggestion { Category = SuggestionCategory.Profile, Priority = SuggestionPriority.Medium, Title = "Add a Headline", Description = "Complete your professional headline." });
            
            if (!string.IsNullOrEmpty(candidate.CareerSummary)) score += 20;
            else opt.Suggestions.Add(new ApplicationOptimizationSuggestion { Category = SuggestionCategory.Profile, Priority = SuggestionPriority.High, Title = "Add a Profile Summary", Description = "Complete your professional summary." });
            
            if (!string.IsNullOrEmpty(candidate.CurrentJobTitle)) score += 20;
            if (candidate.YearsOfExperience.HasValue) score += 20;
            if (!string.IsNullOrEmpty(candidate.LinkedInUrl) || !string.IsNullOrEmpty(candidate.GitHubUrl) || !string.IsNullOrEmpty(candidate.PortfolioUrl)) score += 20;
            else opt.Suggestions.Add(new ApplicationOptimizationSuggestion { Category = SuggestionCategory.Profile, Priority = SuggestionPriority.Low, Title = "Add Professional Links", Description = "Add portfolio/GitHub evidence relevant to the target role." });

            opt.ProfileAlignmentScore = Math.Min(score, maxScore);
        }

        private void CalculateSkillAlignment(CandidateProfile candidate, Job job, ApplicationOptimization opt)
        {
            if (!job.JobSkills.Any())
            {
                opt.SkillAlignmentScore = 100;
                return;
            }

            var candSkills = candidate.CandidateSkills.Select(cs => cs.Skill.Name.ToLower()).ToList();
            var jobSkills = job.JobSkills.Select(js => js.Skill.Name).ToList();

            int matched = 0;
            foreach (var reqSkill in jobSkills)
            {
                if (candSkills.Contains(reqSkill.ToLower()))
                {
                    matched++;
                }
                else
                {
                    opt.Suggestions.Add(new ApplicationOptimizationSuggestion 
                    { 
                        Category = SuggestionCategory.Skills, 
                        Priority = SuggestionPriority.High, 
                        Title = $"Missing Skill: {reqSkill}", 
                        Description = $"Develop practical experience with {reqSkill} because it is required by the target role."
                    });
                }
            }

            opt.SkillAlignmentScore = (double)matched / jobSkills.Count * 100;
        }

        private void CalculateExperienceAlignment(CandidateProfile candidate, Job job, ApplicationOptimization opt)
        {
            if (job.MinimumExperienceYears == null || job.MinimumExperienceYears == 0)
            {
                opt.ExperienceAlignmentScore = 100;
                return;
            }

            var candExp = candidate.YearsOfExperience ?? 0;
            if (candExp == 0 && candidate.WorkExperiences.Any())
            {
                // calculate roughly
                candExp = candidate.WorkExperiences
                    .Where(w => w.StartDate != null && (w.EndDate != null || w.IsCurrent))
                    .Sum(w => ((w.EndDate ?? DateTime.UtcNow) - w.StartDate).Days / 365);
            }

            if (candExp >= job.MinimumExperienceYears)
            {
                opt.ExperienceAlignmentScore = 100;
            }
            else
            {
                var ratio = (double)candExp / job.MinimumExperienceYears.Value;
                opt.ExperienceAlignmentScore = ratio * 100;
                opt.Suggestions.Add(new ApplicationOptimizationSuggestion 
                { 
                    Category = SuggestionCategory.Experience, 
                    Priority = SuggestionPriority.Medium, 
                    Title = "Experience Gap", 
                    Description = $"Role requires {job.MinimumExperienceYears} years, but you have {candExp}. Highlight relevant projects to bridge the gap."
                });
            }
            
            if (candidate.WorkExperiences.Count == 0)
            {
                opt.Suggestions.Add(new ApplicationOptimizationSuggestion 
                { 
                    Category = SuggestionCategory.Experience, 
                    Priority = SuggestionPriority.High, 
                    Title = "Add Work Experience", 
                    Description = "Add measurable project outcomes to your experience section."
                });
            }
        }

        private void CalculateEducationAlignment(CandidateProfile candidate, Job job, ApplicationOptimization opt)
        {
            // Simplified education alignment
            if (!candidate.Educations.Any())
            {
                opt.EducationAlignmentScore = 0;
                opt.Suggestions.Add(new ApplicationOptimizationSuggestion 
                { 
                    Category = SuggestionCategory.Education, 
                    Priority = SuggestionPriority.Medium, 
                    Title = "Add Education", 
                    Description = "Add your educational background to strengthen your profile."
                });
            }
            else
            {
                opt.EducationAlignmentScore = 100;
            }
        }

        private void CalculateResumeAlignment(Resume? latestResume, ApplicationOptimization opt)
        {
            if (latestResume == null)
            {
                opt.ResumeAlignmentScore = null;
                opt.Suggestions.Add(new ApplicationOptimizationSuggestion 
                { 
                    Category = SuggestionCategory.Resume, 
                    Priority = SuggestionPriority.Critical, 
                    Title = "Upload Resume", 
                    Description = "Upload a resume to unlock resume alignment and improve your application readiness."
                });
                return;
            }

            var latestAnalysis = latestResume.Analyses.OrderByDescending(a => a.AnalyzedAt).FirstOrDefault();
            if (latestAnalysis == null)
            {
                opt.ResumeAlignmentScore = 50; // default if not analyzed
                opt.Suggestions.Add(new ApplicationOptimizationSuggestion 
                { 
                    Category = SuggestionCategory.Resume, 
                    Priority = SuggestionPriority.High, 
                    Title = "Analyze Resume", 
                    Description = "Your resume has not been analyzed yet. Run an analysis to get a realistic ATS score."
                });
            }
            else
            {
                opt.ResumeAlignmentScore = latestAnalysis.ATSScore;
                if (latestAnalysis.ATSScore < 70)
                {
                    opt.Suggestions.Add(new ApplicationOptimizationSuggestion 
                    { 
                        Category = SuggestionCategory.Resume, 
                        Priority = SuggestionPriority.High, 
                        Title = "Improve ATS Score", 
                        Description = "Update your resume to include more relevant keywords and improve structure."
                    });
                }
            }
        }

        private void CalculateInterviewReadiness(CandidateProfile candidate, Guid jobId, ApplicationOptimization opt)
        {
            var jobInterviews = candidate.InterviewSessions.Where(i => i.JobId == jobId && i.OverallReadinessScore.HasValue).ToList();
            var generalInterviews = candidate.InterviewSessions.Where(i => i.OverallReadinessScore.HasValue).ToList();

            if (jobInterviews.Any())
            {
                opt.InterviewReadinessScore = jobInterviews.Average(i => i.OverallReadinessScore.Value);
            }
            else if (generalInterviews.Any())
            {
                opt.InterviewReadinessScore = generalInterviews.Average(i => i.OverallReadinessScore.Value);
            }
            else
            {
                opt.InterviewReadinessScore = null; // Missing data
                opt.Suggestions.Add(new ApplicationOptimizationSuggestion 
                { 
                    Category = SuggestionCategory.Interview, 
                    Priority = SuggestionPriority.High, 
                    Title = "Mock Interview Required", 
                    Description = "Complete a technical mock interview for this role to unlock interview readiness score."
                });
            }
        }

        private void CalculateReadinessScore(ApplicationOptimization opt)
        {
            // Weighting: Profile 10%, Skill 30%, Experience 20%, Education 10%, Resume 15%, Interview 15%
            double totalScore = 0;
            double totalWeight = 0;

            if (opt.ProfileAlignmentScore.HasValue) { totalScore += opt.ProfileAlignmentScore.Value * 0.10; totalWeight += 0.10; }
            if (opt.SkillAlignmentScore.HasValue) { totalScore += opt.SkillAlignmentScore.Value * 0.30; totalWeight += 0.30; }
            if (opt.ExperienceAlignmentScore.HasValue) { totalScore += opt.ExperienceAlignmentScore.Value * 0.20; totalWeight += 0.20; }
            if (opt.EducationAlignmentScore.HasValue) { totalScore += opt.EducationAlignmentScore.Value * 0.10; totalWeight += 0.10; }
            
            if (opt.ResumeAlignmentScore.HasValue) { totalScore += opt.ResumeAlignmentScore.Value * 0.15; totalWeight += 0.15; }
            if (opt.InterviewReadinessScore.HasValue) { totalScore += opt.InterviewReadinessScore.Value * 0.15; totalWeight += 0.15; }

            if (totalWeight > 0)
            {
                opt.ApplicationReadinessScore = Math.Round(totalScore / totalWeight, 2);
            }
            else
            {
                opt.ApplicationReadinessScore = 0;
            }
        }

        private void DetermineApplicationStatus(ApplicationOptimization opt)
        {
            if (opt.ApplicationStatus != ApplicationStatus.Draft && opt.ApplicationStatus != ApplicationStatus.Ready) 
                return; // already applied, don't revert

            var score = opt.ApplicationReadinessScore ?? 0;
            opt.IsRecommendedToApply = score >= 75;
            
            if (opt.IsRecommendedToApply) opt.ApplicationStatus = ApplicationStatus.Ready;
            else opt.ApplicationStatus = ApplicationStatus.Draft;
        }

        private void GenerateOptimizedProfileContent(CandidateProfile cand, Job job, ApplicationOptimization opt)
        {
            // Optimized Headline using real skills
            var candSkillsWords = cand.CandidateSkills.Select(cs => cs.Skill.Name).ToList();
            var matchedSkills = job.JobSkills.Select(js => js.Skill.Name).Where(reqSkill => candSkillsWords.Any(cs => cs.Equals(reqSkill, StringComparison.OrdinalIgnoreCase))).ToList();
            
            string headlineBase = cand.CurrentJobTitle ?? "Professional";
            if (matchedSkills.Any())
            {
                opt.OptimizedHeadline = $"{headlineBase} with expertise in {string.Join(", ", matchedSkills.Take(3))}";
            }
            else
            {
                opt.OptimizedHeadline = cand.Headline ?? cand.CurrentJobTitle ?? "Professional Profile";
            }

            // Summary
            opt.OptimizedSummary = $"Experienced {headlineBase} seeking to leverage {cand.YearsOfExperience ?? 0} years of experience for the {job.Title} role." +
                (matchedSkills.Any() ? $" Possess strong core competencies in {string.Join(", ", matchedSkills.Take(5))} aligning with job requirements." : "");
        }

        private void GenerateCoverLetter(CandidateProfile cand, Job job, ApplicationOptimization opt)
        {
            var matchedSkills = job.JobSkills
                .Select(js => js.Skill.Name)
                .Where(js => cand.CandidateSkills.Any(cs => cs.Skill.Name.Equals(js, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            var candName = cand.User != null ? $"{cand.User.FirstName} {cand.User.LastName}".Trim() : "Candidate";
            
            var text = $"Dear Hiring Manager,\n\n";
            text += $"I am writing to express my strong interest in the {job.Title} position at {job.EmployerProfile?.CompanyName ?? "your company"}. ";
            text += $"With {cand.YearsOfExperience ?? 0} years of professional experience, particularly as a {cand.CurrentJobTitle ?? "Professional"}, I am confident in my ability to contribute effectively to your team.\n\n";
            
            if (matchedSkills.Any())
            {
                text += $"My background aligns well with your requirements. Specifically, my concrete experience using {string.Join(", ", matchedSkills.Take(4))} closely matches the needs outlined in the job description. ";
                if (cand.WorkExperiences.Any())
                {
                    var lastExp = cand.WorkExperiences.OrderByDescending(w => w.StartDate).First();
                    text += $"During my time at {lastExp.CompanyName}, I developed a proven track record of delivering results while honing these competencies.\n\n";
                }
                else
                {
                    text += "\n\n";
                }
            }
            
            text += $"I am drawn to this opportunity because of the innovative work being done by your team, and I am excited about the prospect of bringing my skills to your organization.\n\n";
            text += $"Thank you for considering my application. I have attached my resume for your review and would welcome the opportunity to discuss my qualifications further.\n\n";
            text += $"Sincerely,\n{candName}";

            opt.CoverLetter = text;
        }

        private string GetReadinessLevel(double score)
        {
            if (score < 40) return "Early Preparation";
            if (score < 60) return "Needs Improvement";
            if (score < 75) return "Application Building";
            if (score < 90) return "Application Ready";
            return "Highly Prepared";
        }

        private ApplicationOptimizationDto MapToDto(ApplicationOptimization o)
        {
            return new ApplicationOptimizationDto
            {
                Id = o.Id,
                CandidateProfileId = o.CandidateProfileId,
                JobId = o.JobId,
                ResumeId = o.ResumeId,
                ApplicationReadinessScore = o.ApplicationReadinessScore,
                ProfileAlignmentScore = o.ProfileAlignmentScore,
                ResumeAlignmentScore = o.ResumeAlignmentScore,
                SkillAlignmentScore = o.SkillAlignmentScore,
                ExperienceAlignmentScore = o.ExperienceAlignmentScore,
                EducationAlignmentScore = o.EducationAlignmentScore,
                InterviewReadinessScore = o.InterviewReadinessScore,
                ApplicationStatus = o.ApplicationStatus,
                IsRecommendedToApply = o.IsRecommendedToApply,
                OptimizationVersion = o.OptimizationVersion,
                OptimizedHeadline = o.OptimizedHeadline,
                OptimizedSummary = o.OptimizedSummary,
                CoverLetter = o.CoverLetter,
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt,
                Suggestions = o.Suggestions.Select(s => new ApplicationOptimizationSuggestionDto
                {
                    Id = s.Id,
                    Category = s.Category,
                    Priority = s.Priority,
                    Title = s.Title,
                    Description = s.Description,
                    SuggestedAction = s.SuggestedAction,
                    IsCompleted = s.IsCompleted,
                    CreatedAt = s.CreatedAt
                }).ToList()
            };
        }
    }
}
