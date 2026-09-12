using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using CareerMind.Application.DTOs.Candidate;
using CareerMind.Application.DTOs.CareerPath;
using CareerMind.Application.Interfaces;
using CareerMind.Application.Services;
using CareerMind.Domain.Enums;

namespace CareerMind.UnitTests
{
    public class CareerPathIntelligenceServiceTests
    {
        private readonly Mock<ICandidateProfileService> _profileServiceMock;
        private readonly CareerPathIntelligenceService _service;

        public CareerPathIntelligenceServiceTests()
        {
            _profileServiceMock = new Mock<ICandidateProfileService>();
            _service = new CareerPathIntelligenceService(_profileServiceMock.Object);
        }

        private CandidateProfileDto CreateStrongBackendCandidate()
        {
            return new CandidateProfileDto
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                CurrentJobTitle = "Junior .NET Developer",
                YearsOfExperience = 3,
                Skills = new List<CandidateSkillDto>
                {
                    new() { SkillName = "C#", ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 3 },
                    new() { SkillName = "ASP.NET Core", ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 2 },
                    new() { SkillName = "SQL Server", ProficiencyLevel = ProficiencyLevel.Intermediate, YearsOfExperience = 2 },
                    new() { SkillName = "Entity Framework", ProficiencyLevel = ProficiencyLevel.Intermediate, YearsOfExperience = 2 },
                    new() { SkillName = "REST API", ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 2 },
                },
                Educations = new List<EducationDto>
                {
                    new() { Institution = "University", Degree = "BSc", FieldOfStudy = "Computer Science", StartDate = DateTime.Now.AddYears(-4) }
                },
                WorkExperiences = new List<WorkExperienceDto>
                {
                    new() { JobTitle = "Junior .NET Developer", CompanyName = "TechCorp", StartDate = DateTime.Now.AddYears(-3), IsCurrent = true }
                },
                Certifications = new List<CertificationDto>(),
                Languages = new List<CandidateLanguageDto>(),
                CareerPreference = new CareerPreferenceDto { PreferredJobRoles = "Backend Developer" }
            };
        }

        private CandidateProfileDto CreateWeakCandidate()
        {
            return new CandidateProfileDto
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                CurrentJobTitle = "Student",
                YearsOfExperience = 0,
                Skills = new List<CandidateSkillDto>
                {
                    new() { SkillName = "HTML", ProficiencyLevel = ProficiencyLevel.Beginner, YearsOfExperience = 1 },
                },
                Educations = new List<EducationDto>(),
                WorkExperiences = new List<WorkExperienceDto>(),
                Certifications = new List<CertificationDto>(),
                Languages = new List<CandidateLanguageDto>(),
                CareerPreference = null
            };
        }

        private CandidateProfileDto CreateEmptyCandidate()
        {
            return new CandidateProfileDto
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Skills = new List<CandidateSkillDto>(),
                Educations = new List<EducationDto>(),
                WorkExperiences = new List<WorkExperienceDto>(),
                Certifications = new List<CertificationDto>(),
                Languages = new List<CandidateLanguageDto>(),
                CareerPreference = null
            };
        }

        // Test 1
        [Fact]
        public async Task StrongCandidate_ReturnsBackendDeveloperAsStrongRecommendation()
        {
            var profile = CreateStrongBackendCandidate();
            var userId = Guid.NewGuid();
            _profileServiceMock.Setup(x => x.GetProfileAsync(userId)).ReturnsAsync(profile);

            var result = await _service.GetCareerPathIntelligenceAsync(userId);

            Assert.NotNull(result);
            Assert.NotNull(result.BestCareer);
            // The strong backend candidate should have Backend Developer as a top recommendation
            var backendRec = result.RecommendedCareers.FirstOrDefault(r => r.CareerRole == "Backend Developer");
            Assert.NotNull(backendRec);
            Assert.True(backendRec.CompatibilityScore > 60, $"Backend Developer compatibility should be above 60, was {backendRec.CompatibilityScore}");
        }

        // Test 2
        [Fact]
        public async Task CandidateWithMissingSkills_ReturnsSkillGaps()
        {
            var profile = CreateStrongBackendCandidate();
            var userId = Guid.NewGuid();
            _profileServiceMock.Setup(x => x.GetProfileAsync(userId)).ReturnsAsync(profile);

            var result = await _service.GetCareerPathIntelligenceAsync(userId);

            // Senior Backend Developer requires Docker, System Design, Microservices which candidate doesn't have
            var seniorRec = result.RecommendedCareers.FirstOrDefault(r => r.CareerRole == "Senior Backend Developer");
            Assert.NotNull(seniorRec);
            var missingSkills = seniorRec.RequiredSkillsAnalysis.Where(s => s.Status == "MISSING").ToList();
            Assert.NotEmpty(missingSkills);
            Assert.Contains(missingSkills, s => s.SkillName == "Docker" || s.SkillName == "System Design" || s.SkillName == "Microservices");
        }

        // Test 3
        [Fact]
        public async Task CareerRecommendations_AreOrderedByScore()
        {
            var profile = CreateStrongBackendCandidate();
            var userId = Guid.NewGuid();
            _profileServiceMock.Setup(x => x.GetProfileAsync(userId)).ReturnsAsync(profile);

            var result = await _service.GetCareerPathIntelligenceAsync(userId);

            Assert.True(result.RecommendedCareers.Count > 1);
            for (int i = 0; i < result.RecommendedCareers.Count - 1; i++)
            {
                Assert.True(result.RecommendedCareers[i].CompatibilityScore >= result.RecommendedCareers[i + 1].CompatibilityScore,
                    $"Recommendations should be ordered descending. {result.RecommendedCareers[i].CareerRole}={result.RecommendedCareers[i].CompatibilityScore} should be >= {result.RecommendedCareers[i+1].CareerRole}={result.RecommendedCareers[i+1].CompatibilityScore}");
            }
        }

        // Test 4
        [Fact]
        public async Task CareerPath_ContainsProgressionStages()
        {
            var profile = CreateStrongBackendCandidate();
            var userId = Guid.NewGuid();
            _profileServiceMock.Setup(x => x.GetProfileAsync(userId)).ReturnsAsync(profile);

            var result = await _service.GetCareerPathIntelligenceAsync(userId);

            Assert.NotNull(result.CareerPath);
            Assert.True(result.CareerPath.Count >= 2, "Career path should contain at least 2 stages.");
            // Each stage should have a non-empty role
            foreach (var stage in result.CareerPath)
            {
                Assert.False(string.IsNullOrWhiteSpace(stage.Role));
                Assert.False(string.IsNullOrWhiteSpace(stage.Level));
            }
        }

        // Test 5
        [Fact]
        public async Task NextCareerMove_IsDerivedFromCandidateProfile()
        {
            var profile = CreateStrongBackendCandidate();
            var userId = Guid.NewGuid();
            _profileServiceMock.Setup(x => x.GetProfileAsync(userId)).ReturnsAsync(profile);

            var result = await _service.GetCareerPathIntelligenceAsync(userId);

            Assert.NotNull(result.NextCareerMove);
            Assert.False(string.IsNullOrWhiteSpace(result.NextCareerMove.TargetRole));
            Assert.True(result.NextCareerMove.ReadinessPercentage > 0);
            Assert.False(string.IsNullOrWhiteSpace(result.NextCareerMove.Action));
        }

        // Test 6
        [Fact]
        public async Task HighSkillAlignment_ProducesHighCompatibilityScore()
        {
            var profile = CreateStrongBackendCandidate();
            var userId = Guid.NewGuid();
            _profileServiceMock.Setup(x => x.GetProfileAsync(userId)).ReturnsAsync(profile);

            var result = await _service.GetCareerPathIntelligenceAsync(userId);

            // Junior .NET Developer requires C#, .NET, SQL, Git. Candidate has C#, SQL Server (matches SQL)
            var juniorRec = result.RecommendedCareers.FirstOrDefault(r => r.CareerRole == "Junior .NET Developer");
            Assert.NotNull(juniorRec);
            Assert.True(juniorRec.CompatibilityScore > 50, $"Junior .NET Dev compatibility should be above 50 for strong candidate, was {juniorRec.CompatibilityScore}");
        }

        // Test 7
        [Fact]
        public async Task LowSkillAlignment_ProducesLowerCompatibilityScore()
        {
            var profile = CreateWeakCandidate();
            var userId = Guid.NewGuid();
            _profileServiceMock.Setup(x => x.GetProfileAsync(userId)).ReturnsAsync(profile);

            var result = await _service.GetCareerPathIntelligenceAsync(userId);

            // Weak candidate has only HTML/Beginner - Backend roles should score lower
            var backendRec = result.RecommendedCareers.FirstOrDefault(r => r.CareerRole == "Backend Developer");
            Assert.NotNull(backendRec);
            
            // Strong candidate's Backend Developer score
            var strongProfile = CreateStrongBackendCandidate();
            var strongUserId = Guid.NewGuid();
            _profileServiceMock.Setup(x => x.GetProfileAsync(strongUserId)).ReturnsAsync(strongProfile);
            var strongResult = await _service.GetCareerPathIntelligenceAsync(strongUserId);
            var strongBackendRec = strongResult.RecommendedCareers.First(r => r.CareerRole == "Backend Developer");
            
            Assert.True(backendRec.CompatibilityScore < strongBackendRec.CompatibilityScore,
                $"Weak candidate ({backendRec.CompatibilityScore}) should score lower than strong candidate ({strongBackendRec.CompatibilityScore}).");
        }

        // Test 8
        [Fact]
        public async Task CareerRecommendation_ContainsExplainableReasons()
        {
            var profile = CreateStrongBackendCandidate();
            var userId = Guid.NewGuid();
            _profileServiceMock.Setup(x => x.GetProfileAsync(userId)).ReturnsAsync(profile);

            var result = await _service.GetCareerPathIntelligenceAsync(userId);

            Assert.NotNull(result.BestCareer);
            Assert.NotEmpty(result.BestCareer.Explanation);
            Assert.NotEmpty(result.ExplainableInsights);
            // Explanations should reference the career role name
            Assert.Contains(result.BestCareer.Explanation, e => e.Contains(result.BestCareer.CareerRole));
        }

        // Test 9
        [Fact]
        public async Task UnauthorizedCandidateData_IsNotAccessible()
        {
            var userId = Guid.NewGuid();
            _profileServiceMock.Setup(x => x.GetProfileAsync(userId)).ReturnsAsync((CandidateProfileDto?)null);

            await Assert.ThrowsAsync<Exception>(() => _service.GetCareerPathIntelligenceAsync(userId));
        }

        // Test 10
        [Fact]
        public async Task EmptyCandidateProfile_IsHandledSafely()
        {
            var profile = CreateEmptyCandidate();
            var userId = Guid.NewGuid();
            _profileServiceMock.Setup(x => x.GetProfileAsync(userId)).ReturnsAsync(profile);

            var result = await _service.GetCareerPathIntelligenceAsync(userId);

            Assert.NotNull(result);
            Assert.NotEmpty(result.RecommendedCareers);
            // All roles should still be returned, but with low scores
            foreach (var rec in result.RecommendedCareers)
            {
                Assert.True(rec.CompatibilityScore >= 0);
                Assert.True(rec.ReadinessScore >= 0);
            }
        }
    }
}
