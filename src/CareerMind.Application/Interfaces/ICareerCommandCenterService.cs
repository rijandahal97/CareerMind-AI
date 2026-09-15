using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CareerMind.Application.DTOs.CareerCommandCenter;

namespace CareerMind.Application.Interfaces
{
    public interface ICareerCommandCenterService
    {
        Task<CareerCommandCenterDto> GetCareerCommandCenterAsync(Guid candidateUserId);
        Task<List<CareerActionProgressDto>> GetActionProgressesAsync(Guid candidateUserId);
        Task<CareerActionProgressDto> UpdateActionProgressAsync(Guid candidateUserId, string actionKey, bool isCompleted);
    }
}
