using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using CareerMind.Infrastructure.Data;
using CareerMind.Infrastructure.Services;
using CareerMind.Domain.Entities;
using CareerMind.Application.DTOs.Job;

namespace CareerMind.UnitTests
{
    public class JobServiceTests : IDisposable
    {
        private readonly CareerMindDbContext _context;
        private readonly JobService _jobService;
        
        public JobServiceTests()
        {
            var options = new DbContextOptionsBuilder<CareerMindDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _context = new CareerMindDbContext(options);
            _jobService = new JobService(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task CreateJob_WithSalaryValidation_ThrowsExceptionWhenMinGreaterThanMax()
        {
            var userId = Guid.NewGuid();
            _context.EmployerProfiles.Add(new EmployerProfile { UserId = userId, CompanyName = "Test" });
            await _context.SaveChangesAsync();

            var request = new CreateJobRequest
            {
                Title = "Software Engineer",
                MinimumSalary = 100000,
                MaximumSalary = 80000
            };

            await Assert.ThrowsAsync<Exception>(() => _jobService.CreateJobAsync(userId, request));
        }

        [Fact]
        public async Task CreateJob_WithDeadlineValidation_ThrowsExceptionWhenDeadlineInPast()
        {
            var userId = Guid.NewGuid();
            _context.EmployerProfiles.Add(new EmployerProfile { UserId = userId, CompanyName = "Test" });
            await _context.SaveChangesAsync();

            var request = new CreateJobRequest
            {
                Title = "Software Engineer",
                ApplicationDeadline = DateTime.UtcNow.AddDays(-1)
            };

            await Assert.ThrowsAsync<Exception>(() => _jobService.CreateJobAsync(userId, request));
        }

        [Fact]
        public async Task DeleteJob_UnauthorizedAccess_ThrowsExceptionWhenNotOwner()
        {
            var ownerId = Guid.NewGuid();
            var notOwnerId = Guid.NewGuid();

            var ownerProfile = new EmployerProfile { UserId = ownerId, CompanyName = "Owner" };
            var notOwnerProfile = new EmployerProfile { UserId = notOwnerId, CompanyName = "Not Owner" };
            
            _context.EmployerProfiles.Add(ownerProfile);
            _context.EmployerProfiles.Add(notOwnerProfile);

            var job = new Job { EmployerProfile = ownerProfile, Title = "Test Job" };
            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            await Assert.ThrowsAsync<Exception>(() => _jobService.DeleteJobAsync(notOwnerId, job.Id));
        }

        [Fact]
        public async Task CandidateApplication_DuplicateApplicationPrevention_ThrowsException()
        {
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, FirstName = "Test", LastName = "User" };
            var candidate = new CandidateProfile { UserId = userId, User = user };
            _context.Users.Add(user);
            _context.CandidateProfiles.Add(candidate);
            
            var category = new JobCategory { Id = Guid.NewGuid(), Name = "Tech" };
            var job = new Job { Title = "Test Job", Status = "Active", JobCategory = category, EmployerProfile = new EmployerProfile { CompanyName = "Test" } };
            _context.JobCategories.Add(category);
            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            // First application should succeed
            await _jobService.ApplyForJobAsync(userId, job.Id, new ApplyJobRequest());

            // Second application should fail
            await Assert.ThrowsAsync<Exception>(() => _jobService.ApplyForJobAsync(userId, job.Id, new ApplyJobRequest()));
        }

        [Fact]
        public async Task SavedJob_DuplicateSavedJobPrevention_ThrowsException()
        {
            var userId = Guid.NewGuid();
            var candidate = new CandidateProfile { UserId = userId };
            _context.CandidateProfiles.Add(candidate);
            
            var job = new Job { Title = "Test Job", Status = "Active" };
            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            // First save should succeed
            await _jobService.SaveJobAsync(userId, job.Id);

            // Second save should fail
            await Assert.ThrowsAsync<Exception>(() => _jobService.SaveJobAsync(userId, job.Id));
        }

        [Fact]
        public async Task SearchJobs_JobFiltering_ReturnsCorrectJobs()
        {
            var employer = new EmployerProfile { UserId = Guid.NewGuid(), CompanyName = "Tech Corp" };
            var categoryId = Guid.NewGuid();
            var category = new JobCategory { Id = categoryId, Name = "Tech" };
            
            _context.Jobs.Add(new Job { EmployerProfile = employer, JobCategory = category, Title = "C# Dev", IsRemote = true, Status = "Active" });
            _context.Jobs.Add(new Job { EmployerProfile = employer, JobCategory = category, Title = "Java Dev", IsRemote = false, Status = "Active" });
            await _context.SaveChangesAsync();

            var remoteJobs = await _jobService.SearchJobsAsync(null, null, null, true, null, null);
            Assert.Single(remoteJobs);
            Assert.Equal("C# Dev", remoteJobs[0].Title);

            var keywordJobs = await _jobService.SearchJobsAsync("Java", null, null, null, null, null);
            Assert.Single(keywordJobs);
            Assert.Equal("Java Dev", keywordJobs[0].Title);
        }
    }
}
