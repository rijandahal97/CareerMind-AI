using System;
using System.Threading.Tasks;
using CareerMind.Application.DTOs.CareerAnalytics;

namespace CareerMind.Application.Interfaces
{
    public interface ICareerAnalyticsService
    {
        Task<CareerAnalyticsDto> GetCareerAnalyticsAsync(Guid candidateUserId);
    }
}
