using System;
using System.Threading.Tasks;
using CareerMind.Application.DTOs.Employer;
using CareerMind.Application.Interfaces;
using CareerMind.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareerMind.Infrastructure.Services
{
    public class EmployerProfileService : IEmployerProfileService
    {
        private readonly CareerMindDbContext _context;

        public EmployerProfileService(CareerMindDbContext context)
        {
            _context = context;
        }

        public async Task<EmployerProfileDto> GetProfileAsync(Guid userId)
        {
            var profile = await _context.EmployerProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null) throw new Exception("Employer profile not found.");

            return new EmployerProfileDto
            {
                Id = profile.Id,
                CompanyName = profile.CompanyName,
                CompanyDescription = profile.CompanyDescription,
                Industry = profile.Industry,
                CompanySize = profile.CompanySize,
                Website = profile.Website,
                LogoUrl = profile.LogoUrl,
                Location = profile.Location,
                ContactEmail = profile.ContactEmail,
                ContactPhone = profile.ContactPhone,
                VerificationStatus = profile.VerificationStatus
            };
        }

        public async Task<EmployerProfileDto> UpdateProfileAsync(Guid userId, UpdateEmployerProfileRequest request)
        {
            var profile = await _context.EmployerProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null) throw new Exception("Employer profile not found.");

            profile.CompanyName = request.CompanyName;
            profile.CompanyDescription = request.CompanyDescription;
            profile.Industry = request.Industry;
            profile.CompanySize = request.CompanySize;
            profile.Website = request.Website;
            profile.LogoUrl = request.LogoUrl;
            profile.Location = request.Location;
            profile.ContactEmail = request.ContactEmail;
            profile.ContactPhone = request.ContactPhone;

            await _context.SaveChangesAsync();

            return await GetProfileAsync(userId);
        }
    }
}
