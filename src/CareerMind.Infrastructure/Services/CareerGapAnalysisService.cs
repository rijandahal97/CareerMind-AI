using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CareerMind.Application.DTOs.CareerGap;
using CareerMind.Application.Interfaces;
using CareerMind.Domain.Entities;
using CareerMind.Domain.Enums;
using CareerMind.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareerMind.Infrastructure.Services
{
    public class CareerGapAnalysisService : ICareerGapAnalysisService
    {
        private readonly CareerMindDbContext _context;
        private readonly IAIJobMatchingService _jobMatchingService;

        public CareerGapAnalysisService(CareerMindDbContext context, IAIJobMatchingService jobMatchingService)
        {
            _context = context;
            _jobMatchingService = jobMatchingService;
        }

        public async Task<CareerGapAnalysisDto> GetCareerGapAnalysisAsync(Guid candidateUserId, Guid jobId)
        {
            var candidate = await _context.CandidateProfiles
                .Include(c => c.CandidateSkills).ThenInclude(cs => cs.Skill)
                .Include(c => c.WorkExperiences)
                .Include(c => c.Educations)
                .Include(c => c.CareerPreference)
                .FirstOrDefaultAsync(c => c.UserId == candidateUserId);
                
            if (candidate == null) throw new Exception("Candidate profile not found.");

            var job = await _context.Jobs
                .Include(j => j.EmployerProfile)
                .Include(j => j.JobCategory)
                .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
                .FirstOrDefaultAsync(j => j.Id == jobId && j.Status == "Active");

            if (job == null) throw new Exception("Job not found.");

            var matchResult = await _jobMatchingService.GetCandidateJobMatchAsync(candidateUserId, jobId);

            var analysis = new CareerGapAnalysisDto
            {
                JobId = job.Id,
                JobTitle = job.Title,
                CompanyName = job.EmployerProfile?.CompanyName ?? "Unknown Company",
                CurrentReadinessScore = matchResult.OverallMatchScore,
                TotalRequiredSkills = job.JobSkills.Count,
            };

            AnalyzeSkills(candidate, job, analysis);
            GenerateInsights(analysis);
            GenerateRoadmap(analysis);

            return analysis;
        }

        private void AnalyzeSkills(CandidateProfile candidate, Job job, CareerGapAnalysisDto analysis)
        {
            var allGaps = new List<SkillGapDetailDto>();

            foreach (var reqSkill in job.JobSkills)
            {
                var candidateSkill = candidate.CandidateSkills.FirstOrDefault(cs => cs.SkillId == reqSkill.SkillId);
                var requiredProficiency = ParseProficiency(reqSkill.MinimumProficiencyLevel);
                var currentProficiency = candidateSkill != null ? (int)candidateSkill.ProficiencyLevel : 0;
                
                string importance = GetImportanceLabel(reqSkill.ImportanceWeight);

                if (candidateSkill != null && currentProficiency >= requiredProficiency)
                {
                    // STRONG
                    analysis.StrongSkills.Add(reqSkill.Skill.Name);
                    analysis.MatchedSkillsCount++;
                }
                else if (candidateSkill != null && currentProficiency < requiredProficiency)
                {
                    // DEVELOPING
                    analysis.DevelopingSkills.Add(reqSkill.Skill.Name);
                    analysis.DevelopingSkillsCount++;
                    
                    allGaps.Add(new SkillGapDetailDto
                    {
                        SkillId = reqSkill.SkillId,
                        SkillName = reqSkill.Skill.Name,
                        CurrentProficiency = candidateSkill.ProficiencyLevel.ToString(),
                        RequiredProficiency = reqSkill.MinimumProficiencyLevel ?? "Beginner",
                        Importance = importance,
                        GapSeverity = GetSeverity(currentProficiency, requiredProficiency, reqSkill.ImportanceWeight, reqSkill.IsRequired),
                        Status = "Developing",
                        Explanation = $"Your proficiency in {reqSkill.Skill.Name} is below the required level."
                    });
                }
                else
                {
                    // MISSING
                    analysis.MissingSkills.Add(reqSkill.Skill.Name);
                    analysis.MissingSkillsCount++;
                    
                    allGaps.Add(new SkillGapDetailDto
                    {
                        SkillId = reqSkill.SkillId,
                        SkillName = reqSkill.Skill.Name,
                        CurrentProficiency = "None",
                        RequiredProficiency = reqSkill.MinimumProficiencyLevel ?? "Beginner",
                        Importance = importance,
                        GapSeverity = GetSeverity(0, requiredProficiency, reqSkill.ImportanceWeight, reqSkill.IsRequired),
                        Status = "Missing",
                        Explanation = $"{reqSkill.Skill.Name} is required for this role but is not currently listed in your skills."
                    });
                }
            }

            // Calculate priorities
            ProcessGapsAndPriorities(allGaps, analysis);
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
        
        private string GetImportanceLabel(int weight)
        {
            if (weight >= 4) return "High";
            if (weight == 3) return "Medium";
            return "Low";
        }
        
        private string GetSeverity(int currentProficiency, int requiredProficiency, int importanceWeight, bool isRequired)
        {
            var rawImportance = importanceWeight > 0 ? importanceWeight : 1;
            if (isRequired) rawImportance *= 2;

            if (currentProficiency == 0 && (rawImportance >= 3 || isRequired)) return "High";
            if (currentProficiency == 0) return "Medium";

            int gap = requiredProficiency - currentProficiency;
            if (gap >= 2) return "High";
            if (gap == 1 && rawImportance >= 4) return "Medium";
            if (gap == 1) return "Low";

            return "Low";
        }

        private void ProcessGapsAndPriorities(List<SkillGapDetailDto> allGaps, CareerGapAnalysisDto analysis)
        {
            // Calculate a numeric priority for sorting based on severity and importance. Lower number = higher priority.
            foreach (var gap in allGaps)
            {
                gap.Priority = CalculatePriorityScore(gap);
            }

            // High Priority Gaps
            analysis.HighPriorityGaps = allGaps.Where(g => g.GapSeverity == "High").OrderBy(g => g.Priority).ToList();
            
            // Medium Priority Gaps
            analysis.MediumPriorityGaps = allGaps.Where(g => g.GapSeverity == "Medium").OrderBy(g => g.Priority).ToList();
            
            // Low Priority Gaps
            analysis.LowPriorityGaps = allGaps.Where(g => g.GapSeverity == "Low").OrderBy(g => g.Priority).ToList();
        }

        private int CalculatePriorityScore(SkillGapDetailDto gap)
        {
            int score = 0;
            if (gap.GapSeverity == "High") score += 100;
            else if (gap.GapSeverity == "Medium") score += 50;
            else score += 10;

            if (gap.Status == "Missing") score += 20;

            if (gap.Importance == "High") score += 30;
            else if (gap.Importance == "Medium") score += 15;

            // Invert because higher score = more important, but Priority is often 1 = highest.
            // So we return 1000 - score to make the smallest number the highest priority.
            return 1000 - score;
        }

        private void GenerateInsights(CareerGapAnalysisDto analysis)
        {
            if (analysis.StrongSkills.Any())
            {
                var skillsText = string.Join(", ", analysis.StrongSkills.Take(3));
                if (analysis.StrongSkills.Count > 3) skillsText += " and others";
                analysis.CareerInsights.Add($"You already have strong alignment with {skillsText}.");
            }
            else
            {
                analysis.CareerInsights.Add("Your profile is missing most of the foundational skills for this role.");
            }

            if (analysis.HighPriorityGaps.Any())
            {
                var topGap = analysis.HighPriorityGaps.First();
                analysis.CareerInsights.Add($"Your largest gap is {topGap.SkillName}, which is a {(topGap.Importance.ToLower())}-importance requirement for this role.");
            }

            if (analysis.DevelopingSkills.Any())
            {
                var devGap = analysis.DevelopingSkills.First();
                analysis.CareerInsights.Add($"Improving {devGap} proficiency could increase your readiness for this position.");
            }

            if (analysis.MissingSkills.Any())
            {
                analysis.CareerInsights.Add($"Your profile contains several relevant skills, but additional experience with missing requirements would strengthen your profile.");
            }
        }

        private void GenerateRoadmap(CareerGapAnalysisDto analysis)
        {
            var orderedGaps = new List<SkillGapDetailDto>();
            orderedGaps.AddRange(analysis.HighPriorityGaps);
            orderedGaps.AddRange(analysis.MediumPriorityGaps);
            orderedGaps.AddRange(analysis.LowPriorityGaps);

            int step = 1;
            foreach (var gap in orderedGaps.Take(5)) // Suggest up to 5 steps
            {
                analysis.Roadmap.Add(new SkillRoadmapItemDto
                {
                    StepNumber = step++,
                    SkillId = gap.SkillId,
                    SkillName = gap.SkillName,
                    CurrentLevel = gap.CurrentProficiency,
                    TargetLevel = gap.RequiredProficiency,
                    Priority = gap.GapSeverity,
                    Severity = gap.GapSeverity,
                    Reason = gap.Status == "Missing" ? "Missing required skill" : "Current proficiency below requirement",
                    Status = gap.Status
                });
            }
        }
    }
}
