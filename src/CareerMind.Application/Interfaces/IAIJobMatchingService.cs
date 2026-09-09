using System;
using System.Threading.Tasks;

namespace CareerMind.Application.Interfaces
{
    public interface IAIJobMatchingService
    {
        // Conceptual methods for future AI implementation
        Task<decimal> CalculateSkillMatchAsync(Guid candidateProfileId, Guid jobId);
        Task<decimal> CalculateExperienceMatchAsync(Guid candidateProfileId, Guid jobId);
        Task<decimal> CalculatePreferenceMatchAsync(Guid candidateProfileId, Guid jobId);
        Task<decimal> CalculateEducationMatchAsync(Guid candidateProfileId, Guid jobId);
        Task<decimal> CalculateOverallMatchScoreAsync(Guid candidateProfileId, Guid jobId);
    }
}
