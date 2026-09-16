using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CareerMind.Application.DTOs.Interview;

namespace CareerMind.Application.Interfaces
{
    public interface ICareerInterviewService
    {
        Task<InterviewSessionDto> StartInterviewAsync(Guid candidateProfileId, StartInterviewRequestDto request);
        Task<IEnumerable<InterviewSessionDto>> GetCandidateInterviewsAsync(Guid candidateProfileId);
        Task<InterviewSessionDto> GetInterviewSessionAsync(Guid candidateProfileId, Guid sessionId);
        Task<InterviewFeedbackDto> SubmitAnswerAsync(Guid candidateProfileId, Guid sessionId, Guid questionId, SubmitInterviewAnswerRequestDto request);
        Task<InterviewReadinessDto> GetInterviewReadinessAsync(Guid candidateProfileId, Guid sessionId);
        Task<InterviewReadinessDto> CompleteInterviewAsync(Guid candidateProfileId, Guid sessionId);
    }
}
