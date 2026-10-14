using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CareerMind.Application.DTOs.JobRisk;

namespace CareerMind.Application.Interfaces
{
    /// <summary>
    /// Deterministic, explainable job scam risk scoring.
    /// No external APIs or machine-learning models are used.
    /// </summary>
    public interface IJobRiskAnalysisService
    {
        /// <summary>
        /// Analyse a job posting submitted by a user and persist the result.
        /// </summary>
        Task<JobRiskAnalysisResultDto> AnalyzeJobRiskAsync(Guid candidateUserId, AnalyzeJobRiskRequestDto request);

        /// <summary>
        /// Return all previous analyses performed by the given user, newest first.
        /// </summary>
        Task<List<JobRiskAnalysisSummaryDto>> GetRiskAnalysisHistoryAsync(Guid candidateUserId);

        /// <summary>
        /// Return a single previously-saved analysis, only if it belongs to the requesting user.
        /// Throws <see cref="UnauthorizedAccessException"/> when ownership check fails.
        /// </summary>
        Task<JobRiskAnalysisResultDto> GetRiskAnalysisAsync(Guid candidateUserId, Guid analysisId);
    }
}
