using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CareerMind.Application.DTOs.ApplicationOptimization;

namespace CareerMind.Application.Interfaces
{
    public interface IApplicationOptimizationService
    {
        Task<ApplicationOptimizationDto> OptimizeApplicationAsync(Guid candidateProfileId, Guid jobId);
        Task<IEnumerable<ApplicationOptimizationSummaryDto>> GetApplicationOptimizationsAsync(Guid candidateProfileId);
        Task<ApplicationOptimizationDto?> GetApplicationOptimizationAsync(Guid candidateProfileId, Guid jobId);
        Task<ApplicationReadinessDto?> GetApplicationReadinessAsync(Guid candidateProfileId, Guid jobId);
        Task<ApplicationCoverLetterDto?> GetApplicationCoverLetterAsync(Guid candidateProfileId, Guid jobId);
        Task<bool> UpdateSuggestionAsync(Guid candidateProfileId, Guid jobId, Guid suggestionId, UpdateApplicationSuggestionRequest request);
    }
}
