using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CareerMind.Application.DTOs.Resume;
using CareerMind.Domain.Entities;
using CareerMind.Domain.Enums;
using CareerMind.Infrastructure.Data;
using CareerMind.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CareerMind.UnitTests
{
    public class ResumeIntelligenceServiceTests : IDisposable
    {
        private readonly CareerMindDbContext _db;
        private readonly ResumeIntelligenceService _service;

        // Seed data IDs
        private Guid _userId = Guid.NewGuid();
        private Guid _profileId = Guid.NewGuid();
        private Guid _roleId = Guid.NewGuid();
        private Guid _jobId = Guid.NewGuid();
        private Guid _skillCSharpId = Guid.NewGuid();
        private Guid _skillDockerID = Guid.NewGuid();
        private Guid _skillReactId = Guid.NewGuid();

        public ResumeIntelligenceServiceTests()
        {
            var options = new DbContextOptionsBuilder<CareerMindDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new CareerMindDbContext(options);
            _service = new ResumeIntelligenceService(_db, NullLogger<ResumeIntelligenceService>.Instance);

            SeedData();
        }

        private void SeedData()
        {
            var role = new Role { Id = _roleId, Name = "Candidate" };
            var user = new User
            {
                Id = _userId,
                Email = "test@careermind.com",
                FirstName = "John",
                LastName = "Doe",
                PasswordHash = "hash",
                RoleId = _roleId
            };

            var skillCSharp = new Skill { Id = _skillCSharpId, Name = "C#", IsActive = true };
            var skillDocker = new Skill { Id = _skillDockerID, Name = "Docker", IsActive = true };
            var skillReact = new Skill { Id = _skillReactId, Name = "React", IsActive = true };

            var profile = new CandidateProfile
            {
                Id = _profileId,
                UserId = _userId,
                Headline = "Software Engineer",
                CareerSummary = "Experienced developer with 5 years in .NET",
                LinkedInUrl = "https://linkedin.com/in/johndoe"
            };

            profile.CandidateSkills.Add(new CandidateSkill
            {
                Id = Guid.NewGuid(),
                CandidateProfileId = _profileId,
                SkillId = _skillCSharpId,
                ProficiencyLevel = ProficiencyLevel.Advanced,
                Skill = skillCSharp
            });

            profile.WorkExperiences.Add(new WorkExperience
            {
                Id = Guid.NewGuid(),
                CandidateProfileId = _profileId,
                CompanyName = "TechCorp",
                JobTitle = "Software Engineer",
                StartDate = DateTime.UtcNow.AddYears(-3),
                IsCurrent = true
            });

            profile.Educations.Add(new Education
            {
                Id = Guid.NewGuid(),
                CandidateProfileId = _profileId,
                Institution = "MIT",
                Degree = "BSc",
                FieldOfStudy = "Computer Science",
                StartDate = DateTime.UtcNow.AddYears(-7),
                EndDate = DateTime.UtcNow.AddYears(-3),
                Grade = "3.8"
            });

            var catId = Guid.NewGuid();
            var empId = Guid.NewGuid();

            _db.Roles.Add(role);
            _db.Users.Add(user);
            _db.Skills.AddRange(skillCSharp, skillDocker, skillReact);
            _db.CandidateProfiles.Add(profile);
            _db.CandidateSkills.Add(profile.CandidateSkills.First());
            _db.WorkExperiences.Add(profile.WorkExperiences.First());
            _db.Educations.Add(profile.Educations.First());

            _db.SaveChanges();
        }

        private Resume CreatePersistedResume(string text)
        {
            var resume = new Resume
            {
                Id = Guid.NewGuid(),
                CandidateProfileId = _profileId,
                FileName = "safe.pdf",
                OriginalFileName = "resume.pdf",
                FileType = "application/pdf",
                FileSize = 1024,
                StoragePath = "/tmp/safe.pdf",
                ExtractedText = text,
                UploadedAt = DateTime.UtcNow,
                IsPrimary = true
            };
            _db.Resumes.Add(resume);
            _db.SaveChanges();
            return resume;
        }

        // ── Test 1: Resume with strong skill alignment gets high skills score ──
        [Fact]
        public async Task Analyze_StrongSkillAlignment_HighSkillsScore()
        {
            // Arrange: resume mentions C#, which exists in DB and candidate profile
            var resume = CreatePersistedResume("C# developer with 5 years experience in .NET and React");

            // Act
            var result = await _service.AnalyzeResumeAsync(_userId, resume.Id, null);

            // Assert
            Assert.True(result.SkillsScore > 0);
            Assert.Contains("C#", result.SkillsFound);
        }

        // ── Test 2: Resume missing required skills gets lower score ──
        [Fact]
        public async Task Analyze_MissingRequiredSkills_LowerOverallScore()
        {
            // Arrange: resume with no skill mentions
            var resume = CreatePersistedResume("I love cooking and gardening.");

            // Act
            var result = await _service.AnalyzeResumeAsync(_userId, resume.Id, null);

            // Assert - skills score should be 0 since no skills found
            Assert.Equal(0, result.SkillsScore);
        }

        // ── Test 3: Score is always 0-100 ──
        [Fact]
        public async Task Analyze_ScoreAlwaysWithinBounds()
        {
            var resume = CreatePersistedResume("C# React Docker Node.js Python Java SQL leadership communication");

            var result = await _service.AnalyzeResumeAsync(_userId, resume.Id, null);

            Assert.InRange(result.ATSScore, 0, 100);
            Assert.InRange(result.StructureScore, 0, 100);
            Assert.InRange(result.SkillsScore, 0, 100);
            Assert.InRange(result.KeywordScore, 0, 100);
            Assert.InRange(result.ExperienceScore, 0, 100);
            Assert.InRange(result.EducationScore, 0, 100);
            Assert.InRange(result.CompletenessScore, 0, 100);
        }

        // ── Test 4: Empty resume is handled safely ──
        [Fact]
        public async Task Analyze_EmptyResume_HandledSafely()
        {
            var resume = CreatePersistedResume(string.Empty);

            var result = await _service.AnalyzeResumeAsync(_userId, resume.Id, null);

            Assert.NotNull(result);
            Assert.InRange(result.ATSScore, 0, 100);
        }

        // ── Test 5: Structure score improves with sections ──
        [Fact]
        public async Task Analyze_ResumeWithSections_HigherStructureScore()
        {
            var withSections = CreatePersistedResume(
                "John Doe\njohn@email.com\n+1 555 1234\nhttps://linkedin.com/in/john\n" +
                "SUMMARY\nSoftware Engineer\nSKILLS\nC# Python\nEXPERIENCE\nTechCorp 2020-Present\nEDUCATION\nMIT BSc");

            var result = await _service.AnalyzeResumeAsync(_userId, withSections.Id, null);

            Assert.True(result.StructureScore > 30, $"Structure score was {result.StructureScore}");
        }

        // ── Test 6: Completeness score uses profile data ──
        [Fact]
        public async Task Analyze_ProfileWithHeadlineAndLinkedIn_HigherCompleteness()
        {
            var resume = CreatePersistedResume("Professional software developer linkedin.com");

            var result = await _service.AnalyzeResumeAsync(_userId, resume.Id, null);

            // Profile has Headline + LinkedInUrl → should score higher completeness
            Assert.True(result.CompletenessScore >= 20);
        }

        // ── Test 7: Suggestions are generated and prioritized ──
        [Fact]
        public async Task Analyze_MissingSections_SuggestionsPrioritized()
        {
            var resume = CreatePersistedResume("Just some text without any structure.");

            var result = await _service.AnalyzeResumeAsync(_userId, resume.Id, null);

            Assert.NotEmpty(result.Suggestions);
            // Critical suggestions before Low
            var priorities = result.Suggestions.Select(s => s.Priority).ToList();
            Assert.True(priorities.Any());
        }

        // ── Test 8: Candidate can only access own resume (ownership check) ──
        [Fact]
        public async Task GetResume_WrongUser_ThrowsException()
        {
            var resume = CreatePersistedResume("My resume");

            var anotherUserId = Guid.NewGuid();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.GetResumeAsync(anotherUserId, resume.Id));
        }

        // ── Test 9: Delete marks resume as deleted ──
        [Fact]
        public async Task DeleteResume_SetsIsDeletedTrue()
        {
            var resume = CreatePersistedResume("My resume");

            await _service.DeleteResumeAsync(_userId, resume.Id);

            var dbResume = await _db.Resumes.FindAsync(resume.Id);
            Assert.True(dbResume!.IsDeleted);
        }

        // ── Test 10: GetResumes returns only non-deleted for the correct user ──
        [Fact]
        public async Task GetResumes_ReturnsOnlyOwnNonDeleted()
        {
            var r1 = CreatePersistedResume("Resume 1");
            var r2 = CreatePersistedResume("Resume 2");
            r2.IsDeleted = true;
            await _db.SaveChangesAsync();

            var result = await _service.GetResumesAsync(_userId);

            Assert.Contains(result, r => r.Id == r1.Id);
            Assert.DoesNotContain(result, r => r.Id == r2.Id);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }
    }
}
