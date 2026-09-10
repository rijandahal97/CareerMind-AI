using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CareerMind.Application.DTOs.Job;

namespace CareerMind.Application.Interfaces
{
    public interface IAIJobMatchingService
    {
        Task<List<JobMatchResultDto>> GetCandidateJobMatchesAsync(Guid candidateUserId);
        Task<JobMatchResultDto> GetCandidateJobMatchAsync(Guid candidateUserId, Guid jobId);
    }
}
