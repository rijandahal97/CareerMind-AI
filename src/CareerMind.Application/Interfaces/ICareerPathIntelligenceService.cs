using System;
using System.Threading.Tasks;
using CareerMind.Application.DTOs.CareerPath;

namespace CareerMind.Application.Interfaces
{
    public interface ICareerPathIntelligenceService
    {
        Task<CareerPathAnalysisDto> GetCareerPathIntelligenceAsync(Guid candidateUserId);
    }
}
