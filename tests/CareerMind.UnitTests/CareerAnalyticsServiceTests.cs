using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using CareerMind.Infrastructure.Data;
using CareerMind.Infrastructure.Services;
using CareerMind.Domain.Entities;
using CareerMind.Domain.Enums;

namespace CareerMind.UnitTests
{
    public class CareerAnalyticsServiceTests : IDisposable
    {
        private readonly CareerMindDbContext _context;
        private readonly CareerAnalyticsService _service;

        public CareerAnalyticsServiceTests()
        {
            var options = new DbContextOptionsBuilder<CareerMindDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CareerMindDbContext(options);
            _service = new CareerAnalyticsService(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetCareerAnalyticsAsync_ReturnsAnalytics_ForNewCandidateWithMinimalData()
        {
            var userId = Guid.NewGuid();
            var candidateId = Guid.NewGuid();
            var candidate = new CandidateProfile
            {
                Id = candidateId,
                UserId = userId
            };
            _context.CandidateProfiles.Add(candidate);
            await _context.SaveChangesAsync();

            var result = await _service.GetCareerAnalyticsAsync(userId);

            Assert.NotNull(result);
            Assert.Equal(0, result.OverallProgressScore);
            Assert.Equal(0, result.ProfileAnalytics.CompletionPercentage);
            Assert.Empty(result.ProfileAnalytics.CompletedSections);
            Assert.Equal(6, result.ProfileAnalytics.MissingSections.Count);

            Assert.Equal(0, result.SkillAnalytics.TotalSkills);
            Assert.Equal(0, result.JobMatchingAnalytics.TotalJobsEvaluated);
            Assert.False(result.ResumeAnalytics.HasResume);
            Assert.Equal(0, result.InterviewAnalytics.TotalInterviewSessions);
            Assert.Equal(0, result.ApplicationAnalytics.TotalOptimizedApplications);
            Assert.Equal(0, result.ActionPlanAnalytics.TotalPlannedActions);
            
            // Insight for missing resume
            Assert.Contains(result.Insights, i => i.InsightId == "res_insight_3");
            Assert.Contains(result.Insights, i => i.InsightId == "prof_insight_1");
        }

        [Fact]
        public async Task GetCareerAnalyticsAsync_ReturnsCompleteProfile_ForFullyPopulatedCandidate()
        {
            var userId = Guid.NewGuid();
            var candidateId = Guid.NewGuid();
            var candidate = new CandidateProfile
            {
                Id = candidateId,
                UserId = userId,
                Bio = "Experienced Developer",
                CurrentJobTitle = "Senior Software Engineer"
            };

            candidate.Educations.Add(new Education { Id = Guid.NewGuid(), Institution = "MIT", Degree = "BSCS" });
            candidate.WorkExperiences.Add(new WorkExperience { Id = Guid.NewGuid(), JobTitle = "Dev", CompanyName = "Tech" });
            var skill = new Skill { Id = Guid.NewGuid(), Name = "C#", Category = "Backend" };
            candidate.CandidateSkills.Add(new CandidateSkill { Skill = skill, ProficiencyLevel = ProficiencyLevel.Expert });
            candidate.CareerPreference = new CareerPreference { PreferredJobRoles = "Backend Engineer" };

            _context.CandidateProfiles.Add(candidate);
            await _context.SaveChangesAsync();

            var result = await _service.GetCareerAnalyticsAsync(userId);

            Assert.NotNull(result);
            Assert.Equal(100, result.ProfileAnalytics.CompletionPercentage);
            Assert.Equal("Complete", result.ProfileAnalytics.CompletionStatus);
            Assert.Equal(6, result.ProfileAnalytics.CompletedSections.Count);
            Assert.Empty(result.ProfileAnalytics.MissingSections);
            Assert.Contains(result.Insights, i => i.InsightId == "prof_insight_2");
            Assert.True(result.OverallProgressScore > 0);
        }

        [Fact]
        public async Task GetCareerAnalyticsAsync_CalculatesScoresCorrectly_AndDoesNotExceed100()
        {
            var userId = Guid.NewGuid();
            var candidateId = Guid.NewGuid();
            var candidate = new CandidateProfile
            {
                Id = candidateId,
                UserId = userId,
                Bio = "Test",
                CurrentJobTitle = "Test"
            };
            candidate.Educations.Add(new Education());
            candidate.WorkExperiences.Add(new WorkExperience());
            candidate.CandidateSkills.Add(new CandidateSkill { Skill = new Skill { Name = "S1" }, ProficiencyLevel = ProficiencyLevel.Expert });
            candidate.CareerPreference = new CareerPreference();

            _context.CandidateProfiles.Add(candidate);

            var resume = new Resume
            {
                Id = Guid.NewGuid(),
                CandidateProfileId = candidateId,
                Analyses = new List<ResumeAnalysis>
                {
                    new ResumeAnalysis { ATSScore = 150, StructureScore = 150, SkillsScore = 150, KeywordScore = 150, CompletenessScore = 150 }
                }
            };
            _context.Resumes.Add(resume);

            var ivSession = new InterviewSession
            {
                CandidateProfileId = candidateId,
                Status = InterviewStatus.Completed,
                OverallReadinessScore = 150
            };
            _context.InterviewSessions.Add(ivSession);

            var appOpt = new ApplicationOptimization
            {
                CandidateProfileId = candidateId,
                ApplicationReadinessScore = 150
            };
            _context.ApplicationOptimizations.Add(appOpt);

            await _context.SaveChangesAsync();

            var result = await _service.GetCareerAnalyticsAsync(userId);

            // Should be clamped to 100
            Assert.True(result.OverallProgressScore <= 100, $"Score {result.OverallProgressScore} exceeds 100");
        }

        [Fact]
        public async Task GetCareerAnalyticsAsync_UsesExistingData_WithoutFakingValues()
        {
            var userId = Guid.NewGuid();
            var candidateId = Guid.NewGuid();
            var candidate = new CandidateProfile
            {
                Id = candidateId,
                UserId = userId
            };
            _context.CandidateProfiles.Add(candidate);
            await _context.SaveChangesAsync();

            var result = await _service.GetCareerAnalyticsAsync(userId);

            // Verify missing values remain 0 / null, not assigned fake values randomly
            Assert.Equal(0, result.InterviewAnalytics.CompletedSessions);
            Assert.Equal(0, result.InterviewAnalytics.AverageReadinessScore);
            Assert.Equal(0, result.JobMatchingAnalytics.AverageMatchScore);
            Assert.Equal(0, result.ResumeAnalytics.LatestATSScore);
            Assert.Null(result.ResumeAnalytics.PreviousATSScore);
        }

        [Fact]
        public async Task GetCareerAnalyticsAsync_ThrowsKeyNotFound_ForInvalidUser()
        {
            var userId = Guid.NewGuid();
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetCareerAnalyticsAsync(userId));
        }

        [Fact]
        public async Task GetCareerAnalyticsAsync_EmptyStates_HandledCorrectly()
        {
            var userId = Guid.NewGuid();
            var candidate = new CandidateProfile { Id = Guid.NewGuid(), UserId = userId };
            _context.CandidateProfiles.Add(candidate);

            var resumeEmpty = new Resume { CandidateProfileId = candidate.Id };
            _context.Resumes.Add(resumeEmpty);

            var ivEmpty = new InterviewSession { CandidateProfileId = candidate.Id, Status = InterviewStatus.NotStarted };
            _context.InterviewSessions.Add(ivEmpty);

            await _context.SaveChangesAsync();

            var result = await _service.GetCareerAnalyticsAsync(userId);

            // Empty analytics check
            Assert.True(result.ResumeAnalytics.HasResume);
            Assert.Equal(0, result.ResumeAnalytics.LatestATSScore); // No analysis
            
            Assert.Equal(1, result.InterviewAnalytics.TotalInterviewSessions);
            Assert.Equal(0, result.InterviewAnalytics.CompletedSessions); // None completed
        }
    }
}
