using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CareerMind.Application.DTOs.Job;

namespace CareerMind.Application.Interfaces
{
    public interface IJobService
    {
        // Employer Job management
        Task<JobDto> CreateJobAsync(Guid userId, CreateJobRequest request);
        Task<JobDto> UpdateJobAsync(Guid userId, Guid jobId, UpdateJobRequest request);
        Task DeleteJobAsync(Guid userId, Guid jobId);
        Task<List<JobDto>> GetEmployerJobsAsync(Guid userId);
        Task<JobDto> GetEmployerJobByIdAsync(Guid userId, Guid jobId);
        Task<List<JobApplicationDto>> GetJobApplicationsAsync(Guid userId, Guid jobId);
        Task UpdateApplicationStatusAsync(Guid userId, Guid applicationId, UpdateApplicationStatusRequest request);

        // Candidate functionality
        Task<List<JobDto>> SearchJobsAsync(string? keyword, Guid? categoryId, string? location, bool? isRemote, string? employmentType, string? experienceLevel);
        Task<JobDto> GetJobDetailsAsync(Guid jobId);
        Task<JobApplicationDto> ApplyForJobAsync(Guid userId, Guid jobId, ApplyJobRequest request);
        Task<List<JobApplicationDto>> GetCandidateApplicationsAsync(Guid userId);
        
        Task SaveJobAsync(Guid userId, Guid jobId);
        Task RemoveSavedJobAsync(Guid userId, Guid jobId);
        Task<List<SavedJobDto>> GetSavedJobsAsync(Guid userId);
    }
}
