using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using CareerMind.Infrastructure.Data;
using CareerMind.Infrastructure.Services;
using CareerMind.Domain.Entities;
using CareerMind.Domain.Enums;
using CareerMind.Application.Interfaces;
using CareerMind.Application.DTOs.CareerPath;
using CareerMind.Application.DTOs.Job;
using CareerMind.Application.DTOs.CareerGap;

namespace CareerMind.UnitTests
{
    public class CareerCommandCenterServiceTests : IDisposable
    {
        private readonly CareerMindDbContext _context;
        private readonly Mock<ICareerPathIntelligenceService> _pathServiceMock;
        private readonly Mock<IAIJobMatchingService> _jobMatchingServiceMock;
        private readonly CareerCommandCenterService _service;

        public CareerCommandCenterServiceTests()
        {
            var options = new DbContextOptionsBuilder<CareerMindDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CareerMindDbContext(options);
            _pathServiceMock = new Mock<ICareerPathIntelligenceService>();
            _jobMatchingServiceMock = new Mock<IAIJobMatchingService>();

            _service = new CareerCommandCenterService(
                _context,
                _pathServiceMock.Object,
                _jobMatchingServiceMock.Object);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetCareerCommandCenterAsync_ReturnsValidCommandCenterDto_ForExistingCandidate()
        {
            var userId = Guid.NewGuid();
            var candidate = new CandidateProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CurrentJobTitle = "Junior Developer",
                Bio = "Passionate software engineer",
                YearsOfExperience = 2
            };
            _context.CandidateProfiles.Add(candidate);
            await _context.SaveChangesAsync();

            _pathServiceMock.Setup(s => s.GetCareerPathIntelligenceAsync(userId))
                .ReturnsAsync(new CareerPathAnalysisDto
                {
                    BestCareer = new CareerRecommendationDto
                    {
                        CareerRole = "Software Engineer",
                        ReadinessScore = 80m,
                        CompatibilityScore = 85m,
                        TransitionDifficulty = "LOW",
                        RequiredSkillsAnalysis = new List<CareerSkillRequirementDto>()
                    }
                });

            _jobMatchingServiceMock.Setup(s => s.GetCandidateJobMatchesAsync(userId))
                .ReturnsAsync(new List<JobMatchResultDto>());

            var result = await _service.GetCareerCommandCenterAsync(userId);

            Assert.NotNull(result);
            Assert.NotNull(result.Readiness);
            Assert.True(result.Readiness.OverallScore >= 0 && result.Readiness.OverallScore <= 100);
            Assert.NotNull(result.NextBestAction);
            Assert.NotNull(result.ActionPlan);
            Assert.NotNull(result.GoalSummary);
            Assert.Equal("Junior Developer", result.GoalSummary.CurrentRole);
            Assert.Equal("Software Engineer", result.GoalSummary.TargetRole);
        }

        [Fact]
        public async Task GetCareerCommandCenterAsync_ThrowsKeyNotFoundException_WhenCandidateNotFound()
        {
            var nonExistentUserId = Guid.NewGuid();

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.GetCareerCommandCenterAsync(nonExistentUserId));
        }

        [Fact]
        public async Task GetCareerCommandCenterAsync_IdentifiesTopBlockers_WhenResumeMissing()
        {
            var userId = Guid.NewGuid();
            var candidate = new CandidateProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CurrentJobTitle = ".NET Developer"
            };
            _context.CandidateProfiles.Add(candidate);
            await _context.SaveChangesAsync();

            var result = await _service.GetCareerCommandCenterAsync(userId);

            Assert.NotNull(result.TopBlockers);
            Assert.Contains(result.TopBlockers, b => b.Id == "blocker-resume-missing");
            Assert.False(result.ResumeHealth.HasResume);
        }

        [Fact]
        public async Task UpdateActionProgressAsync_CreatesAndUpdatesActionProgress()
        {
            var userId = Guid.NewGuid();
            var candidate = new CandidateProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CurrentJobTitle = "Frontend Developer"
            };
            _context.CandidateProfiles.Add(candidate);
            await _context.SaveChangesAsync();

            string actionKey = "action-profile-complete";

            // Initial insert as completed
            var progress = await _service.UpdateActionProgressAsync(userId, actionKey, true);

            Assert.NotNull(progress);
            Assert.Equal(actionKey, progress.ActionKey);
            Assert.True(progress.IsCompleted);
            Assert.NotNull(progress.CompletedAt);

            // Fetch list
            var list = await _service.GetActionProgressesAsync(userId);
            Assert.Single(list);
            Assert.Equal(actionKey, list.First().ActionKey);
            Assert.True(list.First().IsCompleted);

            // Update to uncompleted
            var updated = await _service.UpdateActionProgressAsync(userId, actionKey, false);
            Assert.False(updated.IsCompleted);
            Assert.Null(updated.CompletedAt);
        }

        [Fact]
        public async Task GetCareerCommandCenterAsync_SyncsActionPlanProgressFromDatabase()
        {
            var userId = Guid.NewGuid();
            var candidateProfileId = Guid.NewGuid();
            var candidate = new CandidateProfile
            {
                Id = candidateProfileId,
                UserId = userId,
                CurrentJobTitle = "DevOps Engineer"
            };
            _context.CandidateProfiles.Add(candidate);

            var actionProgress = new CareerActionProgress
            {
                CandidateProfileId = candidateProfileId,
                ActionKey = "action-profile-complete",
                IsCompleted = true,
                CompletedAt = DateTime.UtcNow
            };
            _context.CareerActionProgresses.Add(actionProgress);
            await _context.SaveChangesAsync();

            var result = await _service.GetCareerCommandCenterAsync(userId);

            Assert.NotNull(result.ActionPlan);
            var completedProfileAction = result.ActionPlan.ThirtyDayActions
                .FirstOrDefault(a => a.ActionKey == "action-profile-complete");

            Assert.NotNull(completedProfileAction);
            Assert.True(completedProfileAction.IsCompleted);
            Assert.True(result.ActionPlan.CompletedActions > 0);
        }
    }
}
