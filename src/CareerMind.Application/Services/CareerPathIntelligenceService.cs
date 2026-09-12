using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CareerMind.Application.DTOs.Candidate;
using CareerMind.Application.DTOs.CareerPath;
using CareerMind.Application.Interfaces;
using CareerMind.Application.Models;
using CareerMind.Domain.Enums;

namespace CareerMind.Application.Services
{
    public class CareerPathIntelligenceService : ICareerPathIntelligenceService
    {
        private readonly ICandidateProfileService _profileService;

        public CareerPathIntelligenceService(ICandidateProfileService profileService)
        {
            _profileService = profileService;
        }

        public async Task<CareerPathAnalysisDto> GetCareerPathIntelligenceAsync(Guid candidateUserId)
        {
            var profile = await _profileService.GetProfileAsync(candidateUserId);
            if (profile == null)
            {
                throw new Exception("Candidate profile not found.");
            }

            var roles = CareerRoleCatalogue.GetRoles();
            var recommendations = new List<CareerRecommendationDto>();

            foreach (var role in roles)
            {
                var recommendation = AnalyzeCareerCompatibility(profile, role);
                recommendations.Add(recommendation);
            }

            // Sort by compatibility score
            recommendations = recommendations.OrderByDescending(r => r.CompatibilityScore).ToList();
            var bestCareer = recommendations.FirstOrDefault();

            var analysis = new CareerPathAnalysisDto
            {
                RecommendedCareers = recommendations,
                BestCareer = bestCareer,
                OverallCareerReadiness = bestCareer?.ReadinessScore ?? 0
            };

            if (bestCareer != null)
            {
                var targetRoleDef = roles.First(r => r.RoleName == bestCareer.CareerRole);
                analysis.CareerPath = GenerateCareerPath(profile, targetRoleDef, roles);
                analysis.NextCareerMove = GenerateNextMove(profile, bestCareer, targetRoleDef);

                analysis.ExplainableInsights.Add($"Your most compatible career path is {bestCareer.CareerRole} with a {bestCareer.CompatibilityScore:F0}% match.");
                if (bestCareer.ReadinessScore > 80)
                    analysis.ExplainableInsights.Add("You are highly ready for this role and should consider applying immediately.");
                else if (bestCareer.ReadinessScore > 50)
                    analysis.ExplainableInsights.Add("You are moderately ready. Focusing on missing skills will significantly increase your chances.");
                else
                    analysis.ExplainableInsights.Add("This role requires skill development. Follow the recommended next actions.");
            }

            return analysis;
        }

        private CareerRecommendationDto AnalyzeCareerCompatibility(CandidateProfileDto profile, CareerRoleDefinition role)
        {
            var result = new CareerRecommendationDto
            {
                CareerRole = role.RoleName,
                Description = role.Description,
                CareerLevel = role.SeniorityLevel
            };

            decimal skillsScore = CalculateSkillsScore(profile, role, result);
            decimal experienceScore = CalculateExperienceScore(profile, role, result);
            decimal educationScore = CalculateEducationScore(profile, role, result);
            decimal preferenceScore = CalculatePreferenceScore(profile, role, result);
            decimal contextScore = CalculateContextScore(profile, role, result);

            result.CompatibilityScore = (skillsScore * 0.50m) + (experienceScore * 0.20m) + (educationScore * 0.10m) + (preferenceScore * 0.10m) + (contextScore * 0.10m);
            if (result.CompatibilityScore > 100) result.CompatibilityScore = 100;

            result.ReadinessScore = CalculateReadiness(skillsScore, experienceScore);

            int totalSkills = role.RequiredSkills.Count;
            int matchedSkills = result.RequiredSkillsAnalysis.Count(s => s.Status == "STRONG" || s.Status == "DEVELOPING");
            result.SkillCoveragePercentage = totalSkills > 0 ? ((decimal)matchedSkills / totalSkills) * 100 : 100;

            if (result.SkillCoveragePercentage >= 80 && result.ReadinessScore >= 75)
                result.TransitionDifficulty = "LOW";
            else if (result.SkillCoveragePercentage >= 50 && result.ReadinessScore >= 50)
                result.TransitionDifficulty = "MEDIUM";
            else
                result.TransitionDifficulty = "HIGH";

            GenerateRecommendationAction(result);

            return result;
        }

        private decimal CalculateSkillsScore(CandidateProfileDto profile, CareerRoleDefinition role, CareerRecommendationDto result)
        {
            if (!role.RequiredSkills.Any()) return 100;

            decimal totalRequiredScore = role.RequiredSkills.Count * 4; // Expert = 4 points
            decimal earnedScore = 0;

            foreach (var reqSkill in role.RequiredSkills)
            {
                var candidateSkill = profile.Skills.FirstOrDefault(s => s.SkillName.Contains(reqSkill, StringComparison.OrdinalIgnoreCase));
                if (candidateSkill != null)
                {
                    earnedScore += (int)candidateSkill.ProficiencyLevel;
                    if (candidateSkill.ProficiencyLevel >= ProficiencyLevel.Intermediate)
                    {
                        result.RequiredSkillsAnalysis.Add(new CareerSkillRequirementDto { SkillName = reqSkill, Status = "STRONG", Priority = "LOW" });
                        result.Strengths.Add(reqSkill);
                    }
                    else
                    {
                        result.RequiredSkillsAnalysis.Add(new CareerSkillRequirementDto { SkillName = reqSkill, Status = "DEVELOPING", Priority = "MEDIUM" });
                    }
                }
                else
                {
                    result.RequiredSkillsAnalysis.Add(new CareerSkillRequirementDto { SkillName = reqSkill, Status = "MISSING", Priority = "HIGH" });
                }
            }

            var score = (earnedScore / totalRequiredScore) * 100;
            
            if (score > 80) result.Explanation.Add($"Your proficiency in key technical skills strongly aligns with {role.RoleName} requirements.");
            else if (score > 50) result.Explanation.Add($"You have a good technical foundation but improving missing skills will boost your {role.RoleName} compatibility.");
            else result.Explanation.Add($"Consider significant technical upskilling to match the {role.RoleName} requirements.");

            return score;
        }

        private decimal CalculateExperienceScore(CandidateProfileDto profile, CareerRoleDefinition role, CareerRecommendationDto result)
        {
            int candidateExp = profile.YearsOfExperience ?? 0;
            if (candidateExp == 0 && profile.WorkExperiences.Any())
            {
               candidateExp = profile.WorkExperiences.Sum(w => w.EndDate.HasValue ? w.EndDate.Value.Year - w.StartDate.Year : DateTime.UtcNow.Year - w.StartDate.Year);
            }

            if (role.MinimumExperienceYears == 0) return 100;
            var score = ((decimal)candidateExp / role.MinimumExperienceYears) * 100;

            if (score >= 100)
            {
                result.Explanation.Add($"Your experience supports this role's seniority.");
                return 100;
            }
            if (score > 50)
            {
                result.Explanation.Add($"You are approaching the experience level needed for this role.");
                return score;
            }
            
            result.Explanation.Add($"This role typically requires {role.MinimumExperienceYears} years of experience, but you can bridge the gap with strong skills.");
            return score;
        }

        private decimal CalculateEducationScore(CandidateProfileDto profile, CareerRoleDefinition role, CareerRecommendationDto result)
        {
            if (!role.RelevantEducation.Any()) return 100;
            if (!profile.Educations.Any()) return 0;

            foreach (var edu in profile.Educations)
            {
                if (role.RelevantEducation.Any(re => edu.FieldOfStudy != null && edu.FieldOfStudy.Contains(re, StringComparison.OrdinalIgnoreCase)))
                {
                    result.Explanation.Add("Your educational background aligns closely with the role.");
                    return 100;
                }
            }
            return 50; 
        }

        private decimal CalculatePreferenceScore(CandidateProfileDto profile, CareerRoleDefinition role, CareerRecommendationDto result)
        {
            if (profile.CareerPreference == null) return 50;
            
            if (!string.IsNullOrEmpty(profile.CareerPreference.PreferredJobRoles) && profile.CareerPreference.PreferredJobRoles.Contains(role.RoleName, StringComparison.OrdinalIgnoreCase))
            {
                return 100;
            }
            return 50;
        }

        private decimal CalculateContextScore(CandidateProfileDto profile, CareerRoleDefinition role, CareerRecommendationDto result)
        {
            if (!string.IsNullOrEmpty(profile.CurrentJobTitle) && 
                (role.RelatedRoles.Any(rr => profile.CurrentJobTitle.Contains(rr, StringComparison.OrdinalIgnoreCase)) || profile.CurrentJobTitle.Contains(role.RoleName, StringComparison.OrdinalIgnoreCase)))
            {
                result.Explanation.Add("Your current or previous role acts as a strong stepping stone.");
                return 100;
            }
            return 30;
        }

        private decimal CalculateReadiness(decimal skillsScore, decimal experienceScore)
        {
            return (skillsScore * 0.7m) + (experienceScore * 0.3m);
        }

        private void GenerateRecommendationAction(CareerRecommendationDto result)
        {
            var missing = result.RequiredSkillsAnalysis.Where(s => s.Status == "MISSING").Select(s => s.SkillName).ToList();
            if (missing.Any())
            {
                result.RecommendedNextAction = $"Focus on learning {string.Join(", ", missing.Take(2))} to significantly boost your readiness.";
            }
            else
            {
                result.RecommendedNextAction = $"Apply for {result.CareerRole} roles, you are well-prepared.";
            }
        }

        private List<CareerPathStageDto> GenerateCareerPath(CandidateProfileDto profile, CareerRoleDefinition targetRole, List<CareerRoleDefinition> allRoles)
        {
            var path = new List<CareerPathStageDto>();
            
            var relatedRoles = allRoles
                .Where(r => r.CareerCategory == targetRole.CareerCategory && r.CareerProgressionLevel <= targetRole.CareerProgressionLevel)
                .OrderBy(r => r.CareerProgressionLevel)
                .ToList();

            foreach (var role in relatedRoles)
            {
                var stage = new CareerPathStageDto
                {
                    Role = role.RoleName,
                    Level = role.SeniorityLevel,
                    RequiredSkills = role.RequiredSkills
                };

                // Evaluate readiness for this stage
                var dummyRec = AnalyzeCareerCompatibility(profile, role);
                stage.ReadinessPercentage = dummyRec.ReadinessScore;
                stage.CandidateSkills = dummyRec.Strengths;
                stage.MissingSkills = dummyRec.RequiredSkillsAnalysis.Where(s => s.Status == "MISSING").Select(s => s.SkillName).ToList();

                stage.IsCurrentRole = !string.IsNullOrEmpty(profile.CurrentJobTitle) && profile.CurrentJobTitle.Contains(role.RoleName, StringComparison.OrdinalIgnoreCase);

                if (stage.ReadinessPercentage > 80)
                {
                     stage.SuggestedNextStep = "Ready for this role";
                }
                else
                {
                     stage.SuggestedNextStep = $"Acquire missing skills: {string.Join(", ", stage.MissingSkills.Take(2))}";
                }

                path.Add(stage);
            }

            return path;
        }

        private NextCareerMoveDto GenerateNextMove(CandidateProfileDto profile, CareerRecommendationDto bestCareer, CareerRoleDefinition targetRole)
        {
            var nextMove = new NextCareerMoveDto
            {
                TargetRole = bestCareer.CareerRole,
                ReadinessPercentage = bestCareer.ReadinessScore,
                Action = bestCareer.RecommendedNextAction
            };

            nextMove.WhyReasons = bestCareer.Explanation;
            nextMove.FocusNext = bestCareer.RequiredSkillsAnalysis.Where(s => s.Status == "MISSING" || s.Status == "DEVELOPING").Take(3).ToList();

            return nextMove;
        }
    }
}
