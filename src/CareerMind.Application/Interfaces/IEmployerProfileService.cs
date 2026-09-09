using System;
using System.Threading.Tasks;
using CareerMind.Application.DTOs.Employer;

namespace CareerMind.Application.Interfaces
{
    public interface IEmployerProfileService
    {
        Task<EmployerProfileDto> GetProfileAsync(Guid userId);
        Task<EmployerProfileDto> UpdateProfileAsync(Guid userId, UpdateEmployerProfileRequest request);
    }
}
