using System;
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
    public class CareerGapAnalysisServiceTests : IDisposable
    {
        private readonly CareerMindDbContext _context;
        private readonly AIJobMatchingService _matchingService;
        private readonly CareerGapAnalysisService _gapService;

        public CareerGapAnalysisServiceTests()
        {
            var options = new DbContextOptionsBuilder<CareerMindDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CareerMindDbContext(options);
            _matchingService = new AIJobMatchingService(_context);
            _gapService = new CareerGapAnalysisService(_context, _matchingService);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private async Task<(CandidateProfile candidate, Job job, Skill skill1, Skill skill2)> SetupBaseData()
        {
            var user = new User { Id = Guid.NewGuid(), FirstName = "Gap", LastName = "User" };
            var candidate = new CandidateProfile { UserId = user.Id, User = user, YearsOfExperience = 5 };

            var employer = new EmployerProfile { UserId = Guid.NewGuid(), CompanyName = "Gap Corp" };
            var category = new JobCategory { Id = Guid.NewGuid(), Name = "Engineering" };
            var job = new Job { EmployerProfile = employer, JobCategory = category, Title = "Software Engineer", Status = "Active", Location = "New York", IsRemote = true, MinimumExperienceYears = 3 };

            var skill1 = new Skill { Name = "C#", Category = "Backend" };
            var skill2 = new Skill { Name = "Azure", Category = "Cloud" };

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
        public async Task GetCareerGapAnalysis_PerfectSkillAlignment_ReturnsNoGaps()
        {
            var data = await SetupBaseData();

            _context.CandidateSkills.Add(new CandidateSkill { CandidateProfileId = data.candidate.Id, SkillId = data.skill1.Id, ProficiencyLevel = ProficiencyLevel.Advanced });
            _context.JobSkills.Add(new JobSkill { JobId = data.job.Id, SkillId = data.skill1.Id, MinimumProficiencyLevel = "Intermediate", IsRequired = true, ImportanceWeight = 4 });
            
            await _context.SaveChangesAsync();

            var result = await _gapService.GetCareerGapAnalysisAsync(data.candidate.UserId, data.job.Id);

            Assert.Equal(1, result.TotalRequiredSkills);
            Assert.Equal(1, result.MatchedSkillsCount);
            Assert.Equal(0, result.MissingSkillsCount);
            Assert.Equal(0, result.DevelopingSkillsCount);
            Assert.Single(result.StrongSkills);
            Assert.Empty(result.MissingSkills);
            Assert.Empty(result.DevelopingSkills);
            Assert.Empty(result.Roadmap);
            Assert.Contains(result.CareerInsights, i => i.Contains(data.skill1.Name));
        }

        [Fact]
        public async Task GetCareerGapAnalysis_MissingRequiredSkill_ReturnsHighPriorityGap()
        {
            var data = await SetupBaseData();

            _context.JobSkills.Add(new JobSkill { JobId = data.job.Id, SkillId = data.skill2.Id, MinimumProficiencyLevel = "Intermediate", IsRequired = true, ImportanceWeight = 4 });
            
            await _context.SaveChangesAsync();

            var result = await _gapService.GetCareerGapAnalysisAsync(data.candidate.UserId, data.job.Id);

            Assert.Equal(1, result.TotalRequiredSkills);
            Assert.Equal(1, result.MissingSkillsCount);
            Assert.Single(result.MissingSkills);
            Assert.Single(result.HighPriorityGaps);
            var highGap = result.HighPriorityGaps.First();
            Assert.Equal(data.skill2.Name, highGap.SkillName);
            Assert.Equal("High", highGap.Importance);
            Assert.Equal("Missing", highGap.Status);
            
            Assert.NotEmpty(result.Roadmap);
            Assert.Equal(data.skill2.Name, result.Roadmap.First().SkillName);
        }

        [Fact]
        public async Task GetCareerGapAnalysis_DevelopingSkill_ReturnsMediumOrHighPriorityGap()
        {
            var data = await SetupBaseData();

            _context.CandidateSkills.Add(new CandidateSkill { CandidateProfileId = data.candidate.Id, SkillId = data.skill1.Id, ProficiencyLevel = ProficiencyLevel.Beginner });
            _context.JobSkills.Add(new JobSkill { JobId = data.job.Id, SkillId = data.skill1.Id, MinimumProficiencyLevel = "Advanced", IsRequired = true, ImportanceWeight = 3 });
            
            await _context.SaveChangesAsync();

            var result = await _gapService.GetCareerGapAnalysisAsync(data.candidate.UserId, data.job.Id);

            Assert.Equal(1, result.DevelopingSkillsCount);
            Assert.Single(result.DevelopingSkills);
            // Gap is Advanced (3) - Beginner (1) = 2 -> High severity
            Assert.Single(result.HighPriorityGaps);
            Assert.Equal("Developing", result.HighPriorityGaps.First().Status);
        }

        [Fact]
        public async Task GetCareerGapAnalysis_MultipleGaps_Orderedcorrectly()
        {
            var data = await SetupBaseData();
            var skill3 = new Skill { Name = "SQL", Category = "DB" };
            _context.Skills.Add(skill3);

            // Candidate has skill3 Beginner
            _context.CandidateSkills.Add(new CandidateSkill { CandidateProfileId = data.candidate.Id, SkillId = skill3.Id, ProficiencyLevel = ProficiencyLevel.Beginner });

            // Job requires skill1(Advanced, Required), skill2(Intermediate, Optional), skill3(Intermediate, Required)
            _context.JobSkills.Add(new JobSkill { JobId = data.job.Id, SkillId = data.skill1.Id, MinimumProficiencyLevel = "Advanced", IsRequired = true, ImportanceWeight = 5 });
            _context.JobSkills.Add(new JobSkill { JobId = data.job.Id, SkillId = data.skill2.Id, MinimumProficiencyLevel = "Intermediate", IsRequired = false, ImportanceWeight = 2 });
            _context.JobSkills.Add(new JobSkill { JobId = data.job.Id, SkillId = skill3.Id, MinimumProficiencyLevel = "Intermediate", IsRequired = true, ImportanceWeight = 3 });
            
            await _context.SaveChangesAsync();

            var result = await _gapService.GetCareerGapAnalysisAsync(data.candidate.UserId, data.job.Id);

            Assert.Equal(3, result.TotalRequiredSkills);
            Assert.Equal(2, result.MissingSkillsCount);
            Assert.Equal(1, result.DevelopingSkillsCount);
            
            // Skill1 (Missing, Required, W=5) -> High priority
            // Skill2 (Missing, Not Required, W=2) -> Medium priority
            // Skill3 (Developing gap=1, Required, W=3, RawW=6) -> Medium priority
            
            Assert.Equal(1, result.HighPriorityGaps.Count);
            Assert.Equal(2, result.MediumPriorityGaps.Count);
            Assert.Equal(0, result.LowPriorityGaps.Count);

            Assert.Equal(data.skill1.Name, result.HighPriorityGaps.First().SkillName);
            // Medium priority contains both skill2 and skill3
            Assert.Contains(result.MediumPriorityGaps, g => g.SkillName == data.skill2.Name);
            Assert.Contains(result.MediumPriorityGaps, g => g.SkillName == skill3.Name);

            // Roadmap generated from High -> Medium -> Low
            Assert.Equal(3, result.Roadmap.Count);
            Assert.Equal(data.skill1.Name, result.Roadmap[0].SkillName);
        }
    }
}
