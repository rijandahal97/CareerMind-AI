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
using CareerMind.Application.DTOs.Candidate;

namespace CareerMind.UnitTests
{
    public class CandidateProfileServiceTests : IDisposable
    {
        private readonly CareerMindDbContext _context;
        private readonly CandidateProfileService _service;
        
        public CandidateProfileServiceTests()
        {
            var options = new DbContextOptionsBuilder<CareerMindDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _context = new CareerMindDbContext(options);
            _service = new CandidateProfileService(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetProfileAsync_ReturnsProfile_WhenExists()
        {
            var userId = Guid.NewGuid();
            var profile = new CandidateProfile { UserId = userId, Headline = "Test Headline" };
            _context.CandidateProfiles.Add(profile);
            await _context.SaveChangesAsync();

            var result = await _service.GetProfileAsync(userId);
            
            Assert.NotNull(result);
            Assert.Equal("Test Headline", result.Headline);
            Assert.Equal(profile.Id, result.Id);
        }

        [Fact]
        public async Task UpdateProfileAsync_CreatesProfile_WhenNotExists()
        {
            var userId = Guid.NewGuid();
            var dto = new UpdateCandidateProfileDto { Headline = "New Profile Headline" };
            
            var result = await _service.UpdateProfileAsync(userId, dto);
            
            Assert.NotNull(result);
            Assert.Equal("New Profile Headline", result.Headline);
            
            var savedProfile = await _context.CandidateProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            Assert.NotNull(savedProfile);
            Assert.Equal("New Profile Headline", savedProfile.Headline);
        }

        [Fact]
        public async Task AddSkillAsync_CalculatesPercentageCorrectly()
        {
            var userId = Guid.NewGuid();
            var profile = new CandidateProfile { UserId = userId, Headline = "Test" };
            var skill = new Skill { Name = "C#" };
            _context.CandidateProfiles.Add(profile);
            _context.Skills.Add(skill);
            await _context.SaveChangesAsync();

            await _service.AddSkillAsync(userId, new AddCandidateSkillDto { SkillId = skill.Id, ProficiencyLevel = ProficiencyLevel.Advanced });
            
            var profileResult = await _service.GetProfileAsync(userId);
            Assert.NotNull(profileResult);
            Assert.True(profileResult.CompletionPercentage > 0);
            Assert.Single(profileResult.Skills);
        }
        
        [Fact]
        public async Task AddEducation_BelongsToCorrectCandidate()
        {
            var userId = Guid.NewGuid();
            var profile = new CandidateProfile { UserId = userId };
            _context.CandidateProfiles.Add(profile);
            await _context.SaveChangesAsync();
            
            var eduDto = new AddEducationDto
            {
                Institution = "Test University",
                Degree = "Bachelor",
                FieldOfStudy = "Computer Science",
                StartDate = DateTime.UtcNow.AddYears(-4),
                EndDate = DateTime.UtcNow
            };
            
            await _service.AddEducationAsync(userId, eduDto);
            
            var savedEdu = await _context.Educations.FirstOrDefaultAsync();
            Assert.NotNull(savedEdu);
            Assert.Equal(profile.Id, savedEdu.CandidateProfileId);
            Assert.Equal("Test University", savedEdu.Institution);
        }
        
        [Fact]
        public async Task User_CannotModify_OtherCandidateProfile()
        {
            // Note: Since service gets profile by passed userId (which comes from Auth context in controller),
            // this effectively verifies ownership. If we pass wrong userId, we modify wrong profile.
            // A more strict test would check controller. 
            // We simulate: user A tries to fetch User B's profile.
            var userIdA = Guid.NewGuid();
            var userIdB = Guid.NewGuid();
            
            var profileB = new CandidateProfile { UserId = userIdB, Headline = "Profile B" };
            _context.CandidateProfiles.Add(profileB);
            await _context.SaveChangesAsync();
            
            var result = await _service.GetProfileAsync(userIdA);
            Assert.Null(result); // Service treats it as non-existent profile for user A.
        }
        
        [Fact]
        public async Task AddExperience_Validates_CurrentJob()
        {
            // Validation is currently done in controller, but let's test if service accepts null end date
            var userId = Guid.NewGuid();
            var profile = new CandidateProfile { UserId = userId };
            _context.CandidateProfiles.Add(profile);
            await _context.SaveChangesAsync();
            
            var expDto = new AddWorkExperienceDto
            {
                CompanyName = "Test Company",
                IsCurrent = true,
                EndDate = null // Valid for current
            };
            
            var res = await _service.AddExperienceAsync(userId, expDto);
            Assert.NotNull(res);
            Assert.Null(res.EndDate);
            Assert.True(res.IsCurrent);
        }
    }
}
