using System;
using System.Collections.Generic;
using CareerMind.Domain.Enums;

namespace CareerMind.Domain.Entities
{
    public class InterviewQuestion : BaseEntity
    {
        public Guid InterviewSessionId { get; set; }
        public InterviewSession InterviewSession { get; set; } = null!;

        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty; // e.g. Technical, Behavioral
        public string Category { get; set; } = string.Empty; // Technical Skills, Experience, Problem Solving, etc.
        public InterviewDifficulty Difficulty { get; set; } = InterviewDifficulty.Intermediate;

        public string? ExpectedTopics { get; set; } // Comma-separated or JSON string of expected keywords/topics

        public int QuestionOrder { get; set; }

        public ICollection<InterviewAnswer> Answers { get; set; } = new List<InterviewAnswer>();
    }
}
