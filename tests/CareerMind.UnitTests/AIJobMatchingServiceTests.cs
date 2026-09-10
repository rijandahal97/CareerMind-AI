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
using CareerMind.Application.DTOs.Job;

namespace CareerMind.UnitTests
{
    public class AIJobMatchingServiceTests : IDisposable
    {
        private readonly CareerMindDbContext _context;
        private readonly AIJobMatchingService _matchingService;

        public AIJobMatchingServiceTests()
        {
            var options = new DbContextOptionsBuilder<CareerMindDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CareerMindDbContext(options);
            _matchingService = new AIJobMatchingService(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private async Task<(CandidateProfile candidate, Job job, Skill skill1, Skill skill2)> SetupBaseData()
        {
            var user = new User { Id = Guid.NewGuid(), FirstName = "Test", LastName = "User" };
            var candidate = new CandidateProfile { UserId = user.Id, User = user, YearsOfExperience = 5 };
            
            var employer = new EmployerProfile { UserId = Guid.NewGuid(), CompanyName = "Tech Corp" };
            var category = new JobCategory { Id = Guid.NewGuid(), Name = "Software" };
            var job = new Job { EmployerProfile = employer, JobCategory = category, Title = "Software Engineer", Status = "Active", Location = "New York", IsRemote = true, MinimumExperienceYears = 3, MaximumSalary = 120000 };
            
            var skill1 = new Skill { Name = "C#", Category = "Backend" };
            var skill2 = new Skill { Name = "SQL", Category = "Database" };

            _context.Users.Add(user);
            _context.CandidateProfiles.Add(candidate);
            _context.EmployerProfiles.Add(employer);
            _context.JobCategories.Add(category);
            _context.Jobs.Add(job);
            _context.Skills.AddRange(skill1, skill2);

            await _context.SaveChangesAsync();

            return (candidate, job, skill1, skill2);
        }

        [Fact]
        public async Task CalculateSkillMatch_PerfectMatch_Returns100()
        {
            var data = await SetupBaseData();

            // Candidate has Expert C#
            _context.CandidateSkills.Add(new CandidateSkill { CandidateProfileId = data.candidate.Id, SkillId = data.skill1.Id, ProficiencyLevel = ProficiencyLevel.Expert });
            // Job requires Intermediate C#
            _context.JobSkills.Add(new JobSkill { JobId = data.job.Id, SkillId = data.skill1.Id, MinimumProficiencyLevel = "Intermediate", IsRequired = true });
            
            await _context.SaveChangesAsync();

            var result = await _matchingService.GetCandidateJobMatchAsync(data.candidate.UserId, data.job.Id);

            Assert.Equal(100m, result.SkillMatchScore);
        }

        [Fact]
        public async Task CalculateSkillMatch_MissingRequiredSkill_ReturnsLowerScore()
        {
            var data = await SetupBaseData();

            // Candidate has SQL
            _context.CandidateSkills.Add(new CandidateSkill { CandidateProfileId = data.candidate.Id, SkillId = data.skill2.Id, ProficiencyLevel = ProficiencyLevel.Intermediate });
            
            // Job requires C# (Required) and SQL (Optional)
            _context.JobSkills.Add(new JobSkill { JobId = data.job.Id, SkillId = data.skill1.Id, MinimumProficiencyLevel = "Intermediate", IsRequired = true, ImportanceWeight = 2 });
            _context.JobSkills.Add(new JobSkill { JobId = data.job.Id, SkillId = data.skill2.Id, MinimumProficiencyLevel = "Beginner", IsRequired = false, ImportanceWeight = 1 });
            
            await _context.SaveChangesAsync();

            var result = await _matchingService.GetCandidateJobMatchAsync(data.candidate.UserId, data.job.Id);

            // Total weight = (2*2) + 1 = 5
            // Match score = 1 / 5 = 20%
            Assert.Equal(20m, result.SkillMatchScore);
            Assert.Contains(result.Gaps, g => g.Contains("Missing required skill"));
        }

        [Fact]
        public async Task CalculateSkillMatch_ProficiencyMismatch_ReturnsPartialScore()
        {
            var data = await SetupBaseData();

            // Candidate has Beginner C#
            _context.CandidateSkills.Add(new CandidateSkill { CandidateProfileId = data.candidate.Id, SkillId = data.skill1.Id, ProficiencyLevel = ProficiencyLevel.Beginner });
            
            // Job requires Expert C#
            _context.JobSkills.Add(new JobSkill { JobId = data.job.Id, SkillId = data.skill1.Id, MinimumProficiencyLevel = "Expert", IsRequired = true, ImportanceWeight = 2 });
            
            await _context.SaveChangesAsync();

            var result = await _matchingService.GetCandidateJobMatchAsync(data.candidate.UserId, data.job.Id);

            // Proficiency too low, gives 0.5 * importance
            // importance = 4 (because required * 2)
            // candidatescore = 4 * 0.5 = 2. weight = 4. score = 50%
            Assert.Equal(50m, result.SkillMatchScore);
            Assert.Contains(result.Gaps, g => g.Contains("lower than required"));
        }

        [Fact]
        public async Task CalculateExperienceMatch_ExperienceMeetsRequirement_Returns100()
        {
            var data = await SetupBaseData();
            
            // Candidate has 3, Job requires 3
            data.candidate.YearsOfExperience = 3;
            await _context.SaveChangesAsync();

            var result = await _matchingService.GetCandidateJobMatchAsync(data.candidate.UserId, data.job.Id);

            Assert.Equal(100m, result.ExperienceMatchScore);
            Assert.Equal("MeetsRequirement", result.ExperienceStatus);
        }

        [Fact]
        public async Task CalculateExperienceMatch_ExperienceExceedsRequirement_Returns100()
        {
            var data = await SetupBaseData();
            
            // Candidate has 6, Job requires 3
            data.candidate.YearsOfExperience = 6;
            await _context.SaveChangesAsync();

            var result = await _matchingService.GetCandidateJobMatchAsync(data.candidate.UserId, data.job.Id);

            Assert.Equal(100m, result.ExperienceMatchScore);
            Assert.Equal("ExceedsRequirement", result.ExperienceStatus);
        }

        [Fact]
        public async Task CalculateExperienceMatch_ExperienceBelowRequirement_ReturnsLowerScore()
        {
            var data = await SetupBaseData();
            
            // Candidate has 1, Job requires 3
            data.candidate.YearsOfExperience = 1;
            await _context.SaveChangesAsync();

            var result = await _matchingService.GetCandidateJobMatchAsync(data.candidate.UserId, data.job.Id);

            Assert.Equal(50m, result.ExperienceMatchScore); // greater than or equal to half
            Assert.Equal("PartiallyMeetsRequirement", result.ExperienceStatus);
        }

        [Fact]
        public async Task CalculateEducationMatch_MeetsRequirement_Returns100()
        {
            var data = await SetupBaseData();
            data.job.Requirements = "Must have a Bachelor's degree";
            
            _context.Educations.Add(new Education { CandidateProfileId = data.candidate.Id, Degree = "Bachelor of Science", StartDate = DateTime.Now });
            await _context.SaveChangesAsync();

            var result = await _matchingService.GetCandidateJobMatchAsync(data.candidate.UserId, data.job.Id);

            Assert.Equal(100m, result.EducationMatchScore);
            Assert.Equal("MeetsRequirement", result.EducationStatus);
        }

        [Fact]
        public async Task CalculateEducationMatch_MissingRequirement_Returns50()
        {
            var data = await SetupBaseData();
            data.job.Requirements = "Must have a Bachelor degree";
            // No education added for candidate
            await _context.SaveChangesAsync();

            var result = await _matchingService.GetCandidateJobMatchAsync(data.candidate.UserId, data.job.Id);

            Assert.Equal(50m, result.EducationMatchScore);
            Assert.Equal("PartiallyMeetsRequirement", result.EducationStatus);
        }

        [Fact]
        public async Task CalculatePreferenceMatch_MeetsCriteria_ReturnsHighScore()
        {
            var data = await SetupBaseData();
            _context.CareerPreferences.Add(new CareerPreference 
            { 
                CandidateProfileId = data.candidate.Id, 
                OpenToRemote = true,
                PreferredLocations = "New York, California",
                PreferredSalaryMin = 100000
            });
            await _context.SaveChangesAsync();

            var result = await _matchingService.GetCandidateJobMatchAsync(data.candidate.UserId, data.job.Id);

            // Job is remote, location is NY, salary max is 120k > min 100k
            // Should be 100%
            Assert.Equal(100m, result.PreferenceMatchScore);
        }

        [Fact]
        public async Task CalculatePreferenceMatch_MismatchedCriteria_ReturnsLowerScore()
        {
            var data = await SetupBaseData();
            _context.CareerPreferences.Add(new CareerPreference 
            { 
                CandidateProfileId = data.candidate.Id, 
                OpenToRemote = false, // Doesn't match remote
                PreferredLocations = "California", // Doesn't match NY
                PreferredSalaryMin = 150000 // Higher than job max
            });
            await _context.SaveChangesAsync();

            var result = await _matchingService.GetCandidateJobMatchAsync(data.candidate.UserId, data.job.Id);

            Assert.True(result.PreferenceMatchScore < 100m);
        }

        [Fact]
        public async Task GetCandidateJobMatches_ReturnsSortedJobs()
        {
            var data = await SetupBaseData(); // sets up one job

            // Set candidate properties
            data.candidate.YearsOfExperience = 5;
            
            var category = new JobCategory { Id = Guid.NewGuid(), Name = "Other" };
            var job2 = new Job { EmployerProfileId = data.job.EmployerProfileId, JobCategory = category, Title = "Junior Engineer", Status = "Active", Location = "Texas", IsRemote = false, MinimumExperienceYears = 1 };
            _context.JobCategories.Add(category);
            _context.Jobs.Add(job2);
            await _context.SaveChangesAsync();

            var results = await _matchingService.GetCandidateJobMatchesAsync(data.candidate.UserId);

            Assert.Equal(2, results.Count);
            Assert.True(results[0].OverallMatchScore >= results[1].OverallMatchScore);
        }
    }
}
