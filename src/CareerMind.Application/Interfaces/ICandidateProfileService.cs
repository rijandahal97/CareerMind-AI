using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CareerMind.Application.DTOs.Candidate;

namespace CareerMind.Application.Interfaces
{
    public interface ICandidateProfileService
    {
        Task<CandidateProfileDto> GetProfileAsync(Guid userId);
        Task<CandidateProfileDto> UpdateProfileAsync(Guid userId, UpdateCandidateProfileDto dto);
        
        // Skills
        Task<IEnumerable<SkillDto>> GetAvailableSkillsAsync(string? query = null);
        Task<CandidateSkillDto> AddSkillAsync(Guid userId, AddCandidateSkillDto dto);
        Task<CandidateSkillDto> UpdateSkillAsync(Guid userId, Guid id, UpdateCandidateSkillDto dto);
        Task<bool> RemoveSkillAsync(Guid userId, Guid id);
        
        // Education
        Task<EducationDto> AddEducationAsync(Guid userId, AddEducationDto dto);
        Task<EducationDto> UpdateEducationAsync(Guid userId, Guid id, UpdateEducationDto dto);
        Task<bool> RemoveEducationAsync(Guid userId, Guid id);
        
        // Experience
        Task<WorkExperienceDto> AddExperienceAsync(Guid userId, AddWorkExperienceDto dto);
        Task<WorkExperienceDto> UpdateExperienceAsync(Guid userId, Guid id, UpdateWorkExperienceDto dto);
        Task<bool> RemoveExperienceAsync(Guid userId, Guid id);
        
        // Certification
        Task<CertificationDto> AddCertificationAsync(Guid userId, AddCertificationDto dto);
        Task<CertificationDto> UpdateCertificationAsync(Guid userId, Guid id, UpdateCertificationDto dto);
        Task<bool> RemoveCertificationAsync(Guid userId, Guid id);
        
        // Language
        Task<IEnumerable<LanguageDto>> GetAvailableLanguagesAsync();
        Task<CandidateLanguageDto> AddLanguageAsync(Guid userId, AddCandidateLanguageDto dto);
        Task<CandidateLanguageDto> UpdateLanguageAsync(Guid userId, Guid LanguageId, UpdateCandidateLanguageDto dto);
        Task<bool> RemoveLanguageAsync(Guid userId, Guid LanguageId);
        
        // Preferences
        Task<CareerPreferenceDto> GetPreferencesAsync(Guid userId);
        Task<CareerPreferenceDto> UpdatePreferencesAsync(Guid userId, UpdateCareerPreferenceDto dto);
    }
}
