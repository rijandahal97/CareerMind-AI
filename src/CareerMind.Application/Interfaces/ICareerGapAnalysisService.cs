using System;
using System.Threading.Tasks;
using CareerMind.Application.DTOs.CareerGap;

namespace CareerMind.Application.Interfaces
{
    public interface ICareerGapAnalysisService
    {
        Task<CareerGapAnalysisDto> GetCareerGapAnalysisAsync(Guid candidateUserId, Guid jobId);
    }
}
