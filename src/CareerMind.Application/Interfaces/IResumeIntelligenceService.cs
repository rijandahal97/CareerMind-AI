using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CareerMind.Application.DTOs.Resume;

namespace CareerMind.Application.Interfaces
{
    public interface IResumeIntelligenceService
    {
        // Upload
        Task<ResumeDto> UploadResumeAsync(Guid userId, ResumeUploadRequest request);

        // List / Get
        Task<List<ResumeDto>> GetResumesAsync(Guid userId);
        Task<ResumeDto> GetResumeAsync(Guid userId, Guid resumeId);

        // Delete
        Task<bool> DeleteResumeAsync(Guid userId, Guid resumeId);

        // Set primary
        Task<bool> SetPrimaryAsync(Guid userId, Guid resumeId);

        // Analyse
        Task<ResumeAnalysisDto> AnalyzeResumeAsync(Guid userId, Guid resumeId, Guid? targetJobId);

        // Get analyses
        Task<List<ResumeAnalysisDto>> GetAnalysesAsync(Guid userId, Guid resumeId);
        Task<ResumeAnalysisDto?> GetLatestAnalysisAsync(Guid userId, Guid resumeId);
    }
}
