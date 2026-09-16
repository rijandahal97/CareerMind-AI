using System;
using CareerMind.Domain.Enums;

namespace CareerMind.Application.DTOs.Interview
{
    public class StartInterviewRequestDto
    {
        public Guid? TargetJobId { get; set; }
        public InterviewType InterviewType { get; set; } = InterviewType.Mixed;
        public InterviewDifficulty Difficulty { get; set; } = InterviewDifficulty.Intermediate;
        public int NumberOfQuestions { get; set; } = 10;
    }
}
