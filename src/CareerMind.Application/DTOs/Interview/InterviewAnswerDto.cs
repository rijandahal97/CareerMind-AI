using System;

namespace CareerMind.Application.DTOs.Interview
{
    public class InterviewAnswerDto
    {
        public Guid Id { get; set; }
        public Guid InterviewQuestionId { get; set; }

        public string AnswerText { get; set; } = string.Empty;

        public double? Score { get; set; }
        public string? Feedback { get; set; }
        public string? Strengths { get; set; }
        public string? Weaknesses { get; set; }
        public string? ImprovementSuggestion { get; set; }

        public DateTime? AnsweredAt { get; set; }
    }
}
