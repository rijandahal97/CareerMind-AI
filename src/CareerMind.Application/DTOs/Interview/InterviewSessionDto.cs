using System;
using System.Collections.Generic;
using CareerMind.Domain.Enums;

namespace CareerMind.Application.DTOs.Interview
{
    public class InterviewSessionDto
    {
        public Guid Id { get; set; }
        public Guid CandidateProfileId { get; set; }
        public Guid? JobId { get; set; }

        public string SessionTitle { get; set; } = string.Empty;
        public string InterviewType { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;

        public double? OverallReadinessScore { get; set; }
        public string Status { get; set; } = string.Empty;

        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<InterviewQuestionDto> Questions { get; set; } = new List<InterviewQuestionDto>();
    }
}
