using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CareerMind.Application.DTOs.Job;
using CareerMind.Application.Interfaces;
using CareerMind.Domain.Entities;
using CareerMind.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareerMind.Infrastructure.Services
{
    public class JobService : IJobService
    {
        private readonly CareerMindDbContext _context;

        public JobService(CareerMindDbContext context)
        {
            _context = context;
        }

        // Employer Job management
        public async Task<JobDto> CreateJobAsync(Guid userId, CreateJobRequest request)
        {
            if (request.MinimumSalary.HasValue && request.MaximumSalary.HasValue && request.MinimumSalary > request.MaximumSalary)
                throw new Exception("Minimum salary cannot be greater than maximum salary.");
            if (request.MinimumExperienceYears.HasValue && request.MaximumExperienceYears.HasValue && request.MinimumExperienceYears > request.MaximumExperienceYears)
                throw new Exception("Minimum experience cannot be greater than maximum experience.");
            if (request.ApplicationDeadline.HasValue && request.ApplicationDeadline.Value <= DateTime.UtcNow)
                throw new Exception("Application deadline must be in the future.");

            var profile = await _context.EmployerProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null) throw new Exception("Employer profile not found.");

            var job = new Job
            {
                EmployerProfileId = profile.Id,
                JobCategoryId = request.JobCategoryId,
                Title = request.Title,
                Description = request.Description,
                Responsibilities = request.Responsibilities,
                Requirements = request.Requirements,
                EmploymentType = request.EmploymentType,
                WorkMode = request.WorkMode,
                ExperienceLevel = request.ExperienceLevel,
                MinimumExperienceYears = request.MinimumExperienceYears,
                MaximumExperienceYears = request.MaximumExperienceYears,
                MinimumSalary = request.MinimumSalary,
                MaximumSalary = request.MaximumSalary,
                SalaryCurrency = request.SalaryCurrency,
                Location = request.Location,
                Country = request.Country,
                City = request.City,
                IsRemote = request.IsRemote,
                ApplicationDeadline = request.ApplicationDeadline,
                VacancyCount = request.VacancyCount,
                Status = "Active"
            };

            foreach (var reqSkill in request.Skills)
            {
                job.JobSkills.Add(new JobSkill
                {
                    SkillId = reqSkill.SkillId,
                    IsRequired = reqSkill.IsRequired,
                    ImportanceWeight = reqSkill.ImportanceWeight,
                    MinimumProficiencyLevel = reqSkill.MinimumProficiencyLevel
                });
            }

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            return await GetEmployerJobByIdAsync(userId, job.Id);
        }

        public async Task<JobDto> UpdateJobAsync(Guid userId, Guid jobId, UpdateJobRequest request)
        {
            var profile = await _context.EmployerProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null) throw new Exception("Employer profile not found.");

            var job = await _context.Jobs
                .Include(j => j.JobSkills)
                .FirstOrDefaultAsync(j => j.Id == jobId && j.EmployerProfileId == profile.Id);

            if (job == null) throw new Exception("Job not found or access denied.");

            if (request.MinimumSalary.HasValue && request.MaximumSalary.HasValue && request.MinimumSalary > request.MaximumSalary)
                throw new Exception("Minimum salary cannot be greater than maximum salary.");
            if (request.MinimumExperienceYears.HasValue && request.MaximumExperienceYears.HasValue && request.MinimumExperienceYears > request.MaximumExperienceYears)
                throw new Exception("Minimum experience cannot be greater than maximum experience.");

            job.JobCategoryId = request.JobCategoryId;
            job.Title = request.Title;
            job.Description = request.Description;
            job.Responsibilities = request.Responsibilities;
            job.Requirements = request.Requirements;
            job.EmploymentType = request.EmploymentType;
            job.WorkMode = request.WorkMode;
            job.ExperienceLevel = request.ExperienceLevel;
            job.MinimumExperienceYears = request.MinimumExperienceYears;
            job.MaximumExperienceYears = request.MaximumExperienceYears;
            job.MinimumSalary = request.MinimumSalary;
            job.MaximumSalary = request.MaximumSalary;
            job.SalaryCurrency = request.SalaryCurrency;
            job.Location = request.Location;
            job.Country = request.Country;
            job.City = request.City;
            job.IsRemote = request.IsRemote;
            job.ApplicationDeadline = request.ApplicationDeadline;
            job.VacancyCount = request.VacancyCount;
            job.Status = request.Status;

            _context.JobSkills.RemoveRange(job.JobSkills);
            foreach (var reqSkill in request.Skills)
            {
                job.JobSkills.Add(new JobSkill
                {
                    SkillId = reqSkill.SkillId,
                    IsRequired = reqSkill.IsRequired,
                    ImportanceWeight = reqSkill.ImportanceWeight,
                    MinimumProficiencyLevel = reqSkill.MinimumProficiencyLevel
                });
            }

            await _context.SaveChangesAsync();
            return await GetEmployerJobByIdAsync(userId, job.Id);
        }

        public async Task DeleteJobAsync(Guid userId, Guid jobId)
        {
            var profile = await _context.EmployerProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null) throw new Exception("Employer profile not found.");

            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == jobId && j.EmployerProfileId == profile.Id);
            if (job == null) throw new Exception("Job not found or access denied.");

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();
        }

        public async Task<List<JobDto>> GetEmployerJobsAsync(Guid userId)
        {
            var profile = await _context.EmployerProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null) return new List<JobDto>();

            var jobs = await _context.Jobs
                .Include(j => j.JobCategory)
                .Where(j => j.EmployerProfileId == profile.Id)
                .OrderByDescending(j => j.PostedAt)
                .ToListAsync();

            return jobs.Select(MapToDto).ToList();
        }

        public async Task<JobDto> GetEmployerJobByIdAsync(Guid userId, Guid jobId)
        {
            var profile = await _context.EmployerProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null) throw new Exception("Employer profile not found.");

            var job = await _context.Jobs
                .Include(j => j.JobCategory)
                .Include(j => j.EmployerProfile)
                .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
                .FirstOrDefaultAsync(j => j.Id == jobId && j.EmployerProfileId == profile.Id);

            if (job == null) throw new Exception("Job not found or access denied.");

            return MapToDtoWithSkills(job);
        }

        public async Task<List<JobApplicationDto>> GetJobApplicationsAsync(Guid userId, Guid jobId)
        {
            var profile = await _context.EmployerProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null) throw new Exception("Employer profile not found.");

            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == jobId && j.EmployerProfileId == profile.Id);
            if (job == null) throw new Exception("Job not found or access denied.");

            var applications = await _context.JobApplications
                .Include(ja => ja.CandidateProfile).ThenInclude(cp => cp.User)
                .Include(ja => ja.Job)
                .Where(ja => ja.JobId == jobId)
                .OrderByDescending(ja => ja.AppliedAt)
                .ToListAsync();

            return applications.Select(MapAppToDto).ToList();
        }

        public async Task UpdateApplicationStatusAsync(Guid userId, Guid applicationId, UpdateApplicationStatusRequest request)
        {
            var profile = await _context.EmployerProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null) throw new Exception("Employer profile not found.");

            var application = await _context.JobApplications
                .Include(ja => ja.Job)
                .FirstOrDefaultAsync(ja => ja.Id == applicationId);

            if (application == null) throw new Exception("Application not found.");
            if (application.Job.EmployerProfileId != profile.Id) throw new Exception("Access denied.");

            application.Status = request.Status;
            await _context.SaveChangesAsync();
        }

        // Candidate functionality
        public async Task<List<JobDto>> SearchJobsAsync(string? keyword, Guid? categoryId, string? location, bool? isRemote, string? employmentType, string? experienceLevel)
        {
            var query = _context.Jobs
                .Include(j => j.JobCategory)
                .Include(j => j.EmployerProfile)
                .Where(j => j.Status == "Active")
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(j => j.Title.Contains(keyword) || j.Description.Contains(keyword) || j.EmployerProfile.CompanyName.Contains(keyword));
            }
            if (categoryId.HasValue)
            {
                query = query.Where(j => j.JobCategoryId == categoryId.Value);
            }
            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(j => j.Location!.Contains(location) || j.City!.Contains(location) || j.Country!.Contains(location));
            }
            if (isRemote.HasValue)
            {
                query = query.Where(j => j.IsRemote == isRemote.Value);
            }
            if (!string.IsNullOrEmpty(employmentType))
            {
                query = query.Where(j => j.EmploymentType == employmentType);
            }
            if (!string.IsNullOrEmpty(experienceLevel))
            {
                query = query.Where(j => j.ExperienceLevel == experienceLevel);
            }

            var jobs = await query
                .OrderByDescending(j => j.PostedAt)
                .ToListAsync();

            return jobs.Select(MapToDto).ToList();
        }

        public async Task<JobDto> GetJobDetailsAsync(Guid jobId)
        {
            var job = await _context.Jobs
                .Include(j => j.JobCategory)
                .Include(j => j.EmployerProfile)
                .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
                .FirstOrDefaultAsync(j => j.Id == jobId && j.Status == "Active");

            if (job == null) throw new Exception("Job not found.");

            return MapToDtoWithSkills(job);
        }

        public async Task<JobApplicationDto> ApplyForJobAsync(Guid userId, Guid jobId, ApplyJobRequest request)
        {
            var candidate = await _context.CandidateProfiles.FirstOrDefaultAsync(c => c.UserId == userId);
            if (candidate == null) throw new Exception("Candidate profile not found.");

            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == jobId && j.Status == "Active");
            if (job == null) throw new Exception("Job not found or not active.");
            if (job.ApplicationDeadline.HasValue && job.ApplicationDeadline.Value < DateTime.UtcNow)
                throw new Exception("Application deadline has passed.");

            var existingApp = await _context.JobApplications
                .FirstOrDefaultAsync(a => a.CandidateProfileId == candidate.Id && a.JobId == jobId);
            
            if (existingApp != null) throw new Exception("You have already applied for this job.");

            var application = new JobApplication
            {
                JobId = jobId,
                CandidateProfileId = candidate.Id,
                CoverLetter = request.CoverLetter,
                ResumeUrl = request.ResumeUrl ?? candidate.ResumeUrl,
                Status = "Applied"
            };

            _context.JobApplications.Add(application);
            await _context.SaveChangesAsync();

            application = await _context.JobApplications
                .Include(a => a.Job).ThenInclude(j => j.EmployerProfile)
                .Include(a => a.CandidateProfile).ThenInclude(c => c.User)
                .FirstOrDefaultAsync(a => a.Id == application.Id);

            return MapAppToDto(application!);
        }

        public async Task<List<JobApplicationDto>> GetCandidateApplicationsAsync(Guid userId)
        {
            var candidate = await _context.CandidateProfiles.FirstOrDefaultAsync(c => c.UserId == userId);
            if (candidate == null) throw new Exception("Candidate profile not found.");

            var applications = await _context.JobApplications
                .Include(a => a.Job).ThenInclude(j => j.EmployerProfile)
                .Include(a => a.CandidateProfile).ThenInclude(c => c.User)
                .Where(a => a.CandidateProfileId == candidate.Id)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();

            return applications.Select(MapAppToDto).ToList();
        }

        public async Task SaveJobAsync(Guid userId, Guid jobId)
        {
            var candidate = await _context.CandidateProfiles.FirstOrDefaultAsync(c => c.UserId == userId);
            if (candidate == null) throw new Exception("Candidate profile not found.");

            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == jobId);
            if (job == null) throw new Exception("Job not found.");

            var existingSaved = await _context.SavedJobs.FirstOrDefaultAsync(s => s.CandidateProfileId == candidate.Id && s.JobId == jobId);
            if (existingSaved != null) throw new Exception("Job is already saved.");

            _context.SavedJobs.Add(new SavedJob { CandidateProfileId = candidate.Id, JobId = jobId });
            await _context.SaveChangesAsync();
        }

        public async Task RemoveSavedJobAsync(Guid userId, Guid jobId)
        {
            var candidate = await _context.CandidateProfiles.FirstOrDefaultAsync(c => c.UserId == userId);
            if (candidate == null) throw new Exception("Candidate profile not found.");

            var saved = await _context.SavedJobs.FirstOrDefaultAsync(s => s.CandidateProfileId == candidate.Id && s.JobId == jobId);
            if (saved == null) throw new Exception("Saved job not found.");

            _context.SavedJobs.Remove(saved);
            await _context.SaveChangesAsync();
        }

        public async Task<List<SavedJobDto>> GetSavedJobsAsync(Guid userId)
        {
            var candidate = await _context.CandidateProfiles.FirstOrDefaultAsync(c => c.UserId == userId);
            if (candidate == null) throw new Exception("Candidate profile not found.");

            var savedJobs = await _context.SavedJobs
                .Include(s => s.Job)
                .ThenInclude(j => j.EmployerProfile)
                .Where(s => s.CandidateProfileId == candidate.Id)
                .OrderByDescending(s => s.SavedAt)
                .ToListAsync();

            return savedJobs.Select(s => new SavedJobDto
            {
                JobId = s.JobId,
                JobTitle = s.Job.Title,
                CompanyName = s.Job.EmployerProfile?.CompanyName ?? "",
                Location = s.Job.Location,
                EmploymentType = s.Job.EmploymentType,
                SavedAt = s.SavedAt
            }).ToList();
        }
        
        private JobDto MapToDto(Job job)
        {
            return new JobDto
            {
                Id = job.Id,
                EmployerProfileId = job.EmployerProfileId,
                CompanyName = job.EmployerProfile?.CompanyName ?? "",
                CompanyLogoUrl = job.EmployerProfile?.LogoUrl,
                JobCategoryId = job.JobCategoryId,
                CategoryName = job.JobCategory?.Name ?? "",
                Title = job.Title,
                Description = job.Description,
                Responsibilities = job.Responsibilities,
                Requirements = job.Requirements,
                EmploymentType = job.EmploymentType,
                WorkMode = job.WorkMode,
                ExperienceLevel = job.ExperienceLevel,
                MinimumExperienceYears = job.MinimumExperienceYears,
                MaximumExperienceYears = job.MaximumExperienceYears,
                MinimumSalary = job.MinimumSalary,
                MaximumSalary = job.MaximumSalary,
                SalaryCurrency = job.SalaryCurrency,
                Location = job.Location,
                Country = job.Country,
                City = job.City,
                IsRemote = job.IsRemote,
                ApplicationDeadline = job.ApplicationDeadline,
                PostedAt = job.PostedAt,
                Status = job.Status,
                VacancyCount = job.VacancyCount
            };
        }

        private JobDto MapToDtoWithSkills(Job job)
        {
            var dto = MapToDto(job);
            if (job.JobSkills != null)
            {
                dto.Skills = job.JobSkills.Select(js => new JobSkillDto
                {
                    SkillId = js.SkillId,
                    SkillName = js.Skill?.Name ?? "",
                    IsRequired = js.IsRequired,
                    ImportanceWeight = js.ImportanceWeight,
                    MinimumProficiencyLevel = js.MinimumProficiencyLevel
                }).ToList();
            }
            return dto;
        }

        private JobApplicationDto MapAppToDto(JobApplication app)
        {
            return new JobApplicationDto
            {
                Id = app.Id,
                JobId = app.JobId,
                JobTitle = app.Job?.Title ?? "",
                CompanyName = app.Job?.EmployerProfile?.CompanyName ?? "",
                CandidateProfileId = app.CandidateProfileId,
                CandidateName = app.CandidateProfile?.User?.FirstName + " " + app.CandidateProfile?.User?.LastName,
                Status = app.Status,
                AiMatchScore = app.AiMatchScore,
                CoverLetter = app.CoverLetter,
                ResumeUrl = app.ResumeUrl,
                AppliedAt = app.AppliedAt
            };
        }
    }
}
