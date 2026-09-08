using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CareerMind.Application.DTOs.Candidate;
using CareerMind.Application.Interfaces;
using CareerMind.Domain.Entities;
using CareerMind.Infrastructure.Data;

namespace CareerMind.Infrastructure.Services
{
    public class CandidateProfileService : ICandidateProfileService
    {
        private readonly CareerMindDbContext _context;

        public CandidateProfileService(CareerMindDbContext context)
        {
            _context = context;
        }

        public async Task<CandidateProfileDto> GetProfileAsync(Guid userId)
        {
            var profile = await _context.CandidateProfiles
                .Include(p => p.CandidateSkills)
                    .ThenInclude(cs => cs.Skill)
                .Include(p => p.Educations)
                .Include(p => p.WorkExperiences)
                .Include(p => p.Certifications)
                .Include(p => p.CandidateLanguages)
                    .ThenInclude(cl => cl.Language)
                .Include(p => p.CareerPreference)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                return null;
            }

            return MapToDto(profile);
        }

        public async Task<CandidateProfileDto> UpdateProfileAsync(Guid userId, UpdateCandidateProfileDto dto)
        {
            var profile = await _context.CandidateProfiles
                .Include(p => p.CandidateSkills)
                .Include(p => p.Educations)
                .Include(p => p.WorkExperiences)
                .Include(p => p.Certifications)
                .Include(p => p.CandidateLanguages)
                .Include(p => p.CareerPreference)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                profile = new CandidateProfile
                {
                    UserId = userId
                };
                _context.CandidateProfiles.Add(profile);
            }

            profile.Headline = dto.Headline;
            profile.Bio = dto.Bio;
            profile.CareerSummary = dto.CareerSummary;
            profile.PhoneNumber = dto.PhoneNumber;
            profile.DateOfBirth = dto.DateOfBirth;
            profile.ProfileImageUrl = dto.ProfileImageUrl;
            profile.CurrentJobTitle = dto.CurrentJobTitle;
            profile.CurrentCompany = dto.CurrentCompany;
            profile.YearsOfExperience = dto.YearsOfExperience;
            profile.LinkedInUrl = dto.LinkedInUrl;
            profile.GitHubUrl = dto.GitHubUrl;
            profile.PortfolioUrl = dto.PortfolioUrl;
            profile.AvailabilityStatus = dto.AvailabilityStatus;
            
            profile.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Refresh to include navigation properties if any were updated in this scoped pass (not strictly necessary for simple updates)
            return await GetProfileAsync(userId);
        }

        public async Task<IEnumerable<SkillDto>> GetAvailableSkillsAsync(string? query = null)
        {
            var skillsQuery = _context.Skills.AsQueryable();
            if (!string.IsNullOrWhiteSpace(query))
            {
                skillsQuery = skillsQuery.Where(s => s.Name.Contains(query));
            }
            
            return await skillsQuery
                .Select(s => new SkillDto 
                { 
                    Id = s.Id, 
                    Name = s.Name, 
                    Category = s.Category, 
                    Description = s.Description,
                    SkillType = s.SkillType
                }).ToListAsync();
        }

        public async Task<CandidateSkillDto> AddSkillAsync(Guid userId, AddCandidateSkillDto dto)
        {
            var profile = await _context.CandidateProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null) throw new Exception("Profile not found");

            var cs = new CandidateSkill
            {
                CandidateProfileId = profile.Id,
                SkillId = dto.SkillId,
                ProficiencyLevel = dto.ProficiencyLevel,
                YearsOfExperience = dto.YearsOfExperience,
                LastUsedYear = dto.LastUsedYear,
                IsPrimary = dto.IsPrimary
            };
            
            _context.CandidateSkills.Add(cs);
            await _context.SaveChangesAsync();
            
            var addedSkill = await _context.Skills.FindAsync(dto.SkillId);

            return new CandidateSkillDto
            {
                Id = cs.Id,
                SkillId = cs.SkillId,
                SkillName = addedSkill?.Name ?? string.Empty,
                ProficiencyLevel = cs.ProficiencyLevel,
                YearsOfExperience = cs.YearsOfExperience,
                LastUsedYear = cs.LastUsedYear,
                IsPrimary = cs.IsPrimary
            };
        }

        public async Task<CandidateSkillDto> UpdateSkillAsync(Guid userId, Guid id, UpdateCandidateSkillDto dto)
        {
            var profile = await _context.CandidateProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null) throw new Exception("Profile not found");

            var cs = await _context.CandidateSkills.Include(s => s.Skill).FirstOrDefaultAsync(x => x.Id == id && x.CandidateProfileId == profile.Id);
            if (cs == null) throw new Exception("Skill not found");

            cs.ProficiencyLevel = dto.ProficiencyLevel;
            cs.YearsOfExperience = dto.YearsOfExperience;
            cs.LastUsedYear = dto.LastUsedYear;
            cs.IsPrimary = dto.IsPrimary;
            cs.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new CandidateSkillDto
            {
                Id = cs.Id,
                SkillId = cs.SkillId,
                SkillName = cs.Skill.Name,
                ProficiencyLevel = cs.ProficiencyLevel,
                YearsOfExperience = cs.YearsOfExperience,
                LastUsedYear = cs.LastUsedYear,
                IsPrimary = cs.IsPrimary
            };
        }

        public async Task<bool> RemoveSkillAsync(Guid userId, Guid id)
        {
            var profile = await _context.CandidateProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null) return false;

            var cs = await _context.CandidateSkills.FirstOrDefaultAsync(x => x.Id == id && x.CandidateProfileId == profile.Id);
            if (cs == null) return false;

            _context.CandidateSkills.Remove(cs);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<EducationDto> AddEducationAsync(Guid userId, AddEducationDto dto)
        {
            var profile = await _context.CandidateProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            var edu = new Education
            {
                CandidateProfileId = profile.Id,
                Institution = dto.Institution,
                Degree = dto.Degree,
                FieldOfStudy = dto.FieldOfStudy,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Grade = dto.Grade,
                Description = dto.Description
            };
            _context.Educations.Add(edu);
            await _context.SaveChangesAsync();

            return new EducationDto
            {
                Id = edu.Id,
                Institution = edu.Institution,
                Degree = edu.Degree,
                FieldOfStudy = edu.FieldOfStudy,
                StartDate = edu.StartDate,
                EndDate = edu.EndDate,
                Grade = edu.Grade,
                Description = edu.Description
            };
        }

        public async Task<EducationDto> UpdateEducationAsync(Guid userId, Guid id, UpdateEducationDto dto)
        {
            var profile = await _context.CandidateProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            var edu = await _context.Educations.FirstOrDefaultAsync(e => e.Id == id && e.CandidateProfileId == profile.Id);
            if (edu == null) throw new Exception("Education not found");

            edu.Institution = dto.Institution;
            edu.Degree = dto.Degree;
            edu.FieldOfStudy = dto.FieldOfStudy;
            edu.StartDate = dto.StartDate;
            edu.EndDate = dto.EndDate;
            edu.Grade = dto.Grade;
            edu.Description = dto.Description;
            edu.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            
            return new EducationDto
            {
                Id = edu.Id,
                Institution = edu.Institution,
                Degree = edu.Degree,
                FieldOfStudy = edu.FieldOfStudy,
                StartDate = edu.StartDate,
                EndDate = edu.EndDate,
                Grade = edu.Grade,
                Description = edu.Description
            };
        }

        public async Task<bool> RemoveEducationAsync(Guid userId, Guid id)
        {
            var profile = await _context.CandidateProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            var edu = await _context.Educations.FirstOrDefaultAsync(e => e.Id == id && e.CandidateProfileId == profile.Id);
            if (edu == null) return false;

            _context.Educations.Remove(edu);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<WorkExperienceDto> AddExperienceAsync(Guid userId, AddWorkExperienceDto dto)
        {
            var profile = await _context.CandidateProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            var exp = new WorkExperience
            {
                CandidateProfileId = profile.Id,
                CompanyName = dto.CompanyName,
                JobTitle = dto.JobTitle,
                EmploymentType = dto.EmploymentType,
                Location = dto.Location,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsCurrent = dto.IsCurrent,
                Description = dto.Description
            };
            _context.WorkExperiences.Add(exp);
            await _context.SaveChangesAsync();
            
            return new WorkExperienceDto
            {
                Id = exp.Id,
                CompanyName = exp.CompanyName,
                JobTitle = exp.JobTitle,
                EmploymentType = exp.EmploymentType,
                Location = exp.Location,
                StartDate = exp.StartDate,
                EndDate = exp.EndDate,
                IsCurrent = exp.IsCurrent,
                Description = exp.Description
            };
        }

        public async Task<WorkExperienceDto> UpdateExperienceAsync(Guid userId, Guid id, UpdateWorkExperienceDto dto)
        {
            var profile = await _context.CandidateProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            var exp = await _context.WorkExperiences.FirstOrDefaultAsync(e => e.Id == id && e.CandidateProfileId == profile.Id);
            if (exp == null) throw new Exception("Experience not found");

            exp.CompanyName = dto.CompanyName;
            exp.JobTitle = dto.JobTitle;
            exp.EmploymentType = dto.EmploymentType;
            exp.Location = dto.Location;
            exp.StartDate = dto.StartDate;
            exp.EndDate = dto.EndDate;
            exp.IsCurrent = dto.IsCurrent;
            exp.Description = dto.Description;
            exp.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            
            return new WorkExperienceDto
            {
                Id = exp.Id,
                CompanyName = exp.CompanyName,
                JobTitle = exp.JobTitle,
                EmploymentType = exp.EmploymentType,
                Location = exp.Location,
                StartDate = exp.StartDate,
                EndDate = exp.EndDate,
                IsCurrent = exp.IsCurrent,
                Description = exp.Description
            };
        }

        public async Task<bool> RemoveExperienceAsync(Guid userId, Guid id)
        {
            var profile = await _context.CandidateProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            var exp = await _context.WorkExperiences.FirstOrDefaultAsync(e => e.Id == id && e.CandidateProfileId == profile.Id);
            if (exp == null) return false;

            _context.WorkExperiences.Remove(exp);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<CertificationDto> AddCertificationAsync(Guid userId, AddCertificationDto dto)
        {
            var profile = await _context.CandidateProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            var cert = new Certification
            {
                CandidateProfileId = profile.Id,
                Name = dto.Name,
                IssuingOrganization = dto.IssuingOrganization,
                IssueDate = dto.IssueDate,
                ExpiryDate = dto.ExpiryDate,
                CredentialId = dto.CredentialId,
                CredentialUrl = dto.CredentialUrl,
                Description = dto.Description
            };
            _context.Certifications.Add(cert);
            await _context.SaveChangesAsync();

            return new CertificationDto
            {
                Id = cert.Id,
                Name = cert.Name,
                IssuingOrganization = cert.IssuingOrganization,
                IssueDate = cert.IssueDate,
                ExpiryDate = cert.ExpiryDate,
                CredentialId = cert.CredentialId,
                CredentialUrl = cert.CredentialUrl,
                Description = cert.Description
            };
        }

        public async Task<CertificationDto> UpdateCertificationAsync(Guid userId, Guid id, UpdateCertificationDto dto)
        {
            var profile = await _context.CandidateProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            var cert = await _context.Certifications.FirstOrDefaultAsync(e => e.Id == id && e.CandidateProfileId == profile.Id);
            if (cert == null) throw new Exception("Certification not found");

            cert.Name = dto.Name;
            cert.IssuingOrganization = dto.IssuingOrganization;
            cert.IssueDate = dto.IssueDate;
            cert.ExpiryDate = dto.ExpiryDate;
            cert.CredentialId = dto.CredentialId;
            cert.CredentialUrl = dto.CredentialUrl;
            cert.Description = dto.Description;
            cert.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            
            return new CertificationDto
            {
                Id = cert.Id,
                Name = cert.Name,
                IssuingOrganization = cert.IssuingOrganization,
                IssueDate = cert.IssueDate,
                ExpiryDate = cert.ExpiryDate,
                CredentialId = cert.CredentialId,
                CredentialUrl = cert.CredentialUrl,
                Description = cert.Description
            };
        }

        public async Task<bool> RemoveCertificationAsync(Guid userId, Guid id)
        {
            var profile = await _context.CandidateProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            var cert = await _context.Certifications.FirstOrDefaultAsync(e => e.Id == id && e.CandidateProfileId == profile.Id);
            if (cert == null) return false;

            _context.Certifications.Remove(cert);
            await _context.SaveChangesAsync();
            return true;
        }

        // Language Implementation limits here
        public async Task<IEnumerable<LanguageDto>> GetAvailableLanguagesAsync()
        {
            return await _context.Languages.Select(l => new LanguageDto { Id = l.Id, Name = l.Name, Code = l.Code }).ToListAsync();
        }
        
        public async Task<CandidateLanguageDto> AddLanguageAsync(Guid userId, AddCandidateLanguageDto dto)
        {
            var profile = await _context.CandidateProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            var lang = new CandidateLanguage
            {
                CandidateProfileId = profile.Id,
                LanguageId = dto.LanguageId,
                ProficiencyLevel = dto.ProficiencyLevel
            };
            _context.CandidateLanguages.Add(lang);
            await _context.SaveChangesAsync();
            
            var name = (await _context.Languages.FindAsync(dto.LanguageId))?.Name;
            return new CandidateLanguageDto { Id = lang.Id, LanguageId = lang.LanguageId, LanguageName = name, ProficiencyLevel = lang.ProficiencyLevel };
        }

        public async Task<CandidateLanguageDto> UpdateLanguageAsync(Guid userId, Guid id, UpdateCandidateLanguageDto dto)
        {
            var profile = await _context.CandidateProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            var lang = await _context.CandidateLanguages.Include(l => l.Language).FirstOrDefaultAsync(e => e.Id == id && e.CandidateProfileId == profile.Id);
            lang.ProficiencyLevel = dto.ProficiencyLevel;
            lang.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return new CandidateLanguageDto { Id = lang.Id, LanguageId = lang.LanguageId, LanguageName = lang.Language.Name, ProficiencyLevel = lang.ProficiencyLevel };
        }

        public async Task<bool> RemoveLanguageAsync(Guid userId, Guid id)
        {
            var profile = await _context.CandidateProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            var lang = await _context.CandidateLanguages.FirstOrDefaultAsync(e => e.Id == id && e.CandidateProfileId == profile.Id);
            if (lang == null) return false;
            _context.CandidateLanguages.Remove(lang);
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<CareerPreferenceDto> GetPreferencesAsync(Guid userId)
        {
            var profile = await _context.CandidateProfiles.Include(p => p.CareerPreference).FirstOrDefaultAsync(p => p.UserId == userId);
            var pref = profile?.CareerPreference;
            if (pref == null) return null;
            return new CareerPreferenceDto
            {
                Id = pref.Id,
                PreferredJobRoles = pref.PreferredJobRoles,
                PreferredIndustries = pref.PreferredIndustries,
                PreferredLocations = pref.PreferredLocations,
                OpenToRemote = pref.OpenToRemote,
                WillingToRelocate = pref.WillingToRelocate,
                PreferredEmploymentTypes = pref.PreferredEmploymentTypes,
                PreferredSalaryMin = pref.PreferredSalaryMin,
                PreferredSalaryMax = pref.PreferredSalaryMax,
                CareerInterests = pref.CareerInterests
            };
        }

        public async Task<CareerPreferenceDto> UpdatePreferencesAsync(Guid userId, UpdateCareerPreferenceDto dto)
        {
            var profile = await _context.CandidateProfiles.Include(p => p.CareerPreference).FirstOrDefaultAsync(p => p.UserId == userId);
            var pref = profile.CareerPreference;
            if (pref == null)
            {
                pref = new CareerPreference { CandidateProfileId = profile.Id };
                _context.CareerPreferences.Add(pref);
            }
            pref.PreferredJobRoles = dto.PreferredJobRoles;
            pref.PreferredIndustries = dto.PreferredIndustries;
            pref.PreferredLocations = dto.PreferredLocations;
            pref.OpenToRemote = dto.OpenToRemote;
            pref.WillingToRelocate = dto.WillingToRelocate;
            pref.PreferredEmploymentTypes = dto.PreferredEmploymentTypes;
            pref.PreferredSalaryMin = dto.PreferredSalaryMin;
            pref.PreferredSalaryMax = dto.PreferredSalaryMax;
            pref.CareerInterests = dto.CareerInterests;
            pref.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new CareerPreferenceDto
            {
                Id = pref.Id,
                PreferredJobRoles = pref.PreferredJobRoles,
                PreferredIndustries = pref.PreferredIndustries,
                PreferredLocations = pref.PreferredLocations,
                OpenToRemote = pref.OpenToRemote,
                WillingToRelocate = pref.WillingToRelocate,
                PreferredEmploymentTypes = pref.PreferredEmploymentTypes,
                PreferredSalaryMin = pref.PreferredSalaryMin,
                PreferredSalaryMax = pref.PreferredSalaryMax,
                CareerInterests = pref.CareerInterests
            };
        }

        private CandidateProfileDto MapToDto(CandidateProfile profile)
        {
            int completionPercentage = 0;
            int filledSections = 0;
            int totalSections = 7;
            
            // 1. Profile
            if (!string.IsNullOrEmpty(profile.Headline) && !string.IsNullOrEmpty(profile.Bio)) filledSections++;
            // 2. Skills
            if (profile.CandidateSkills.Any()) filledSections++;
            // 3. Education
            if (profile.Educations.Any()) filledSections++;
            // 4. Experience
            if (profile.WorkExperiences.Any()) filledSections++;
            // 5. Certifications
            if (profile.Certifications.Any()) filledSections++;
            // 6. Languages
            if (profile.CandidateLanguages.Any()) filledSections++;
            // 7. Preferences
            if (profile.CareerPreference != null) filledSections++;

            completionPercentage = (int)((filledSections / (double)totalSections) * 100);

            return new CandidateProfileDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                Headline = profile.Headline,
                Bio = profile.Bio,
                CareerSummary = profile.CareerSummary,
                PhoneNumber = profile.PhoneNumber,
                DateOfBirth = profile.DateOfBirth,
                ProfileImageUrl = profile.ProfileImageUrl,
                ResumeUrl = profile.ResumeUrl,
                CurrentJobTitle = profile.CurrentJobTitle,
                CurrentCompany = profile.CurrentCompany,
                YearsOfExperience = profile.YearsOfExperience,
                LinkedInUrl = profile.LinkedInUrl,
                GitHubUrl = profile.GitHubUrl,
                PortfolioUrl = profile.PortfolioUrl,
                AvailabilityStatus = profile.AvailabilityStatus,
                CompletionPercentage = completionPercentage,
                
                Skills = profile.CandidateSkills.Select(cs => new CandidateSkillDto
                {
                    Id = cs.Id,
                    SkillId = cs.SkillId,
                    SkillName = cs.Skill?.Name ?? "",
                    ProficiencyLevel = cs.ProficiencyLevel,
                    YearsOfExperience = cs.YearsOfExperience,
                    LastUsedYear = cs.LastUsedYear,
                    IsPrimary = cs.IsPrimary
                }).ToList(),
                
                Educations = profile.Educations.Select(e => new EducationDto 
                {
                    Id = e.Id,
                    Institution = e.Institution,
                    Degree = e.Degree,
                    FieldOfStudy = e.FieldOfStudy,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    Grade = e.Grade,
                    Description = e.Description
                }).ToList(),

                WorkExperiences = profile.WorkExperiences.Select(w => new WorkExperienceDto
                {
                    Id = w.Id,
                    CompanyName = w.CompanyName,
                    JobTitle = w.JobTitle,
                    EmploymentType = w.EmploymentType,
                    Location = w.Location,
                    StartDate = w.StartDate,
                    EndDate = w.EndDate,
                    IsCurrent = w.IsCurrent,
                    Description = w.Description
                }).ToList(),

                Certifications = profile.Certifications.Select(c => new CertificationDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    IssuingOrganization = c.IssuingOrganization,
                    IssueDate = c.IssueDate,
                    ExpiryDate = c.ExpiryDate,
                    CredentialId = c.CredentialId,
                    CredentialUrl = c.CredentialUrl,
                    Description = c.Description
                }).ToList(),

                Languages = profile.CandidateLanguages.Select(cl => new CandidateLanguageDto
                {
                    Id = cl.Id,
                    LanguageId = cl.LanguageId,
                    LanguageName = cl.Language?.Name ?? "",
                    ProficiencyLevel = cl.ProficiencyLevel
                }).ToList(),

                CareerPreference = profile.CareerPreference == null ? null : new CareerPreferenceDto
                {
                    Id = profile.CareerPreference.Id,
                    PreferredJobRoles = profile.CareerPreference.PreferredJobRoles,
                    PreferredIndustries = profile.CareerPreference.PreferredIndustries,
                    PreferredLocations = profile.CareerPreference.PreferredLocations,
                    OpenToRemote = profile.CareerPreference.OpenToRemote,
                    WillingToRelocate = profile.CareerPreference.WillingToRelocate,
                    PreferredEmploymentTypes = profile.CareerPreference.PreferredEmploymentTypes,
                    PreferredSalaryMin = profile.CareerPreference.PreferredSalaryMin,
                    PreferredSalaryMax = profile.CareerPreference.PreferredSalaryMax,
                    CareerInterests = profile.CareerPreference.CareerInterests
                }
            };
        }
    }
}
