using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CareerMind.Application.DTOs.Job;
using CareerMind.Application.Interfaces;
using CareerMind.Domain.Entities;
using CareerMind.Domain.Enums;
using CareerMind.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareerMind.Infrastructure.Services
{
    public class AIJobMatchingService : IAIJobMatchingService
    {
        private readonly CareerMindDbContext _context;

        // Centralized configuration for weights
        private const decimal SkillWeight = 0.50m;
        private const decimal ExperienceWeight = 0.20m;
        private const decimal EducationWeight = 0.15m;
        private const decimal PreferenceWeight = 0.15m;

        public AIJobMatchingService(CareerMindDbContext context)
        {
            _context = context;
        }

        public async Task<List<JobMatchResultDto>> GetCandidateJobMatchesAsync(Guid candidateUserId)
        {
            var candidate = await GetCandidateProfileAsync(candidateUserId);
            if (candidate == null) throw new Exception("Candidate profile not found.");

            // Fetch active jobs
            var jobs = await _context.Jobs
                .Include(j => j.EmployerProfile)
                .Include(j => j.JobCategory)
                .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
                .Where(j => j.Status == "Active")
                .ToListAsync();

            var results = new List<JobMatchResultDto>();

            foreach (var job in jobs)
            {
                var result = CalculateMatch(candidate, job);
                results.Add(result);
            }

            return results.OrderByDescending(r => r.OverallMatchScore).ToList();
        }

        public async Task<JobMatchResultDto> GetCandidateJobMatchAsync(Guid candidateUserId, Guid jobId)
        {
            var candidate = await GetCandidateProfileAsync(candidateUserId);
            if (candidate == null) throw new Exception("Candidate profile not found.");

            var job = await _context.Jobs
                .Include(j => j.EmployerProfile)
                .Include(j => j.JobCategory)
                .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
                .FirstOrDefaultAsync(j => j.Id == jobId && j.Status == "Active");

            if (job == null) throw new Exception("Job not found.");

            return CalculateMatch(candidate, job);
        }

        private async Task<CandidateProfile?> GetCandidateProfileAsync(Guid userId)
        {
            return await _context.CandidateProfiles
                .Include(c => c.CandidateSkills).ThenInclude(cs => cs.Skill)
                .Include(c => c.WorkExperiences)
                .Include(c => c.Educations)
                .Include(c => c.CareerPreference)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        private JobMatchResultDto CalculateMatch(CandidateProfile candidate, Job job)
        {
            var result = new JobMatchResultDto
            {
                JobId = job.Id,
                JobTitle = job.Title,
                CompanyName = job.EmployerProfile?.CompanyName ?? "Unknown Company"
            };

            CalculateSkillMatch(candidate, job, result);
            CalculateExperienceMatch(candidate, job, result);
            CalculateEducationMatch(candidate, job, result);
            CalculatePreferenceMatch(candidate, job, result);

            result.OverallMatchScore = Math.Round(
                (result.SkillMatchScore * SkillWeight) +
                (result.ExperienceMatchScore * ExperienceWeight) +
                (result.EducationMatchScore * EducationWeight) +
                (result.PreferenceMatchScore * PreferenceWeight), 2);

            return result;
        }

        private void CalculateSkillMatch(CandidateProfile candidate, Job job, JobMatchResultDto result)
        {
            if (!job.JobSkills.Any())
            {
                result.SkillMatchScore = 100m;
                result.Strengths.Add("Job does not have specific required skills.");
                return;
            }

            decimal totalRequiredWeight = 0;
            decimal candidateScore = 0;

            foreach (var reqSkill in job.JobSkills)
            {
                // Determine importance (1 to 5)
                decimal importance = reqSkill.ImportanceWeight > 0 ? reqSkill.ImportanceWeight : 1;
                if (reqSkill.IsRequired) importance *= 2; // Doubled weight for required skills

                totalRequiredWeight += importance;

                var cSkill = candidate.CandidateSkills.FirstOrDefault(cs => cs.SkillId == reqSkill.SkillId);
                
                if (cSkill != null)
                {
                    int reqProficiency = ParseProficiency(reqSkill.MinimumProficiencyLevel);
                    int cProficiency = (int)cSkill.ProficiencyLevel;
                    
                    if (cProficiency >= reqProficiency)
                    {
                        candidateScore += importance;
                        result.MatchedSkills.Add(cSkill.Skill.Name);
                    }
                    else
                    {
                        // Partial match for lower proficiency
                        candidateScore += importance * 0.5m;
                        result.MissingSkills.Add(new MissingSkillDto
                        {
                            SkillName = cSkill.Skill.Name,
                            RequiredProficiency = reqSkill.MinimumProficiencyLevel ?? "Beginner",
                            Importance = reqSkill.ImportanceWeight,
                            GapStatus = "Proficiency too low"
                        });
                        result.Gaps.Add($"Proficiency level for {cSkill.Skill.Name} is lower than required.");
                    }
                }
                else
                {
                    result.MissingSkills.Add(new MissingSkillDto
                    {
                        SkillName = reqSkill.Skill.Name,
                        RequiredProficiency = reqSkill.MinimumProficiencyLevel ?? "Beginner",
                        Importance = reqSkill.ImportanceWeight,
                        GapStatus = "Missing skill"
                    });
                    
                    if (reqSkill.IsRequired)
                    {
                        result.Gaps.Add($"Missing required skill: {reqSkill.Skill.Name}");
                    }
                }
            }

            result.SkillMatchScore = totalRequiredWeight > 0 ? (candidateScore / totalRequiredWeight) * 100m : 100m;
            result.SkillMatchScore = Math.Min(result.SkillMatchScore, 100m);

            if (result.SkillMatchScore > 80)
            {
                result.Explanations.Add("Strong skill alignment with the job requirements.");
            }
            else if (result.SkillMatchScore > 50)
            {
                result.Explanations.Add("Moderate skill alignment. You have some of the required skills.");
            }
            else
            {
                result.Explanations.Add("Skill profile does not strongly match the job requirements.");
            }
        }

        private void CalculateExperienceMatch(CandidateProfile candidate, Job job, JobMatchResultDto result)
        {
            if (!job.MinimumExperienceYears.HasValue)
            {
                result.ExperienceMatchScore = 100m;
                result.ExperienceStatus = "MeetsRequirement";
                result.Strengths.Add("Job does not have a minimum experience requirement.");
                return;
            }

            int reqExp = job.MinimumExperienceYears.Value;
            
            // Calculate candidate's total years of experience manually if not set
            int candidateExp = candidate.YearsOfExperience ?? 0;
            if (candidateExp == 0 && candidate.WorkExperiences.Any())
            {
                double totalDays = 0;
                foreach (var exp in candidate.WorkExperiences)
                {
                    var start = exp.StartDate;
                    var end = exp.EndDate ?? DateTime.UtcNow;
                    totalDays += (end - start).TotalDays;
                }
                candidateExp = (int)(totalDays / 365.25);
            }

            if (candidateExp >= reqExp + 2)
            {
                result.ExperienceMatchScore = 100m;
                result.ExperienceStatus = "ExceedsRequirement";
                result.Explanations.Add("Your experience exceeds the minimum requirement.");
            }
            else if (candidateExp >= reqExp)
            {
                result.ExperienceMatchScore = 100m;
                result.ExperienceStatus = "MeetsRequirement";
                result.Explanations.Add("Your experience meets the minimum requirement.");
            }
            else if (candidateExp > 0 && candidateExp >= reqExp / 2)
            {
                result.ExperienceMatchScore = 50m;
                result.ExperienceStatus = "PartiallyMeetsRequirement";
                result.Explanations.Add($"You have {candidateExp} years of experience, but the job requires {reqExp} years.");
            }
            else
            {
                result.ExperienceMatchScore = 0m;
                result.ExperienceStatus = "DoesNotMeetRequirement";
                result.Gaps.Add($"Requires {reqExp} years of experience (You have {candidateExp}).");
            }
        }

        private void CalculateEducationMatch(CandidateProfile candidate, Job job, JobMatchResultDto result)
        {
            // For Day 5, assuming job requirements string defines degree briefly.
            // If the job domain model lacks explicit education requirement fields, we gracefully skip or attempt string match.
            bool reqSpecified = !string.IsNullOrEmpty(job.Requirements) && job.Requirements.ToLower().Contains("degree");
            
            if (!reqSpecified)
            {
                result.EducationMatchScore = 100m;
                result.EducationStatus = "RequirementNotSpecified";
                result.Explanations.Add("Education requirement not specified by employer.");
                return;
            }

            bool hasDegree = candidate.Educations.Any(e => !string.IsNullOrEmpty(e.Degree) && (e.Degree.ToLower().Contains("bachelor") || e.Degree.ToLower().Contains("master") || e.Degree.ToLower().Contains("bs") || e.Degree.ToLower().Contains("ba")));
            
            if (hasDegree)
            {
                result.EducationMatchScore = 100m;
                result.EducationStatus = "MeetsRequirement";
                result.Explanations.Add("Your education profile matches standard degree requirements.");
            }
            else
            {
                result.EducationMatchScore = 50m;
                result.EducationStatus = "PartiallyMeetsRequirement";
                result.Explanations.Add("Job mentions degree but no formal degree found in your profile.");
            }
        }

        private void CalculatePreferenceMatch(CandidateProfile candidate, Job job, JobMatchResultDto result)
        {
            var pref = candidate.CareerPreference;
            if (pref == null)
            {
                result.PreferenceMatchScore = 50m; // Neutral
                result.PreferenceStatus = "NoPreferencesSet";
                result.Explanations.Add("Candidate career preferences are not set.");
                return;
            }

            decimal score = 0;
            decimal maxScore = 0;

            // Remote match
            maxScore += 1;
            if (pref.OpenToRemote && job.IsRemote)
            {
                score += 1;
                result.Strengths.Add("Your preferred remote work arrangement matches this job.");
            }
            else if (!job.IsRemote)
            {
                // Not remote job, so neutral match
                score += 0.5m;
            }

            // Location
            if (!string.IsNullOrEmpty(pref.PreferredLocations) && !string.IsNullOrEmpty(job.Location))
            {
                maxScore += 1;
                if (pref.PreferredLocations.Contains(job.Location, StringComparison.OrdinalIgnoreCase) || job.Location.Contains(pref.PreferredLocations, StringComparison.OrdinalIgnoreCase))
                {
                    score += 1;
                }
            }

            // Salary
            if (pref.PreferredSalaryMin.HasValue && job.MaximumSalary.HasValue)
            {
                maxScore += 1;
                if (job.MaximumSalary >= pref.PreferredSalaryMin)
                {
                    score += 1;
                }
                else
                {
                    result.Gaps.Add("Salary expectation is higher than the maximum offered.");
                }
            }

            result.PreferenceMatchScore = maxScore > 0 ? (score / maxScore) * 100m : 100m;
            result.PreferenceStatus = result.PreferenceMatchScore > 80 ? "StrongMatch" : (result.PreferenceMatchScore > 50 ? "ModerateMatch" : "WeakMatch");
            
            if (result.PreferenceMatchScore > 80)
            {
                result.Explanations.Add("Career preferences align very well with the job.");
            }
        }

        private int ParseProficiency(string? label)
        {
            if (string.IsNullOrEmpty(label)) return 1;
            var lower = label.ToLower();
            if (lower.Contains("expert")) return 4;
            if (lower.Contains("advanced")) return 3;
            if (lower.Contains("intermediate")) return 2;
            return 1; // Beginner
        }
    }
}
