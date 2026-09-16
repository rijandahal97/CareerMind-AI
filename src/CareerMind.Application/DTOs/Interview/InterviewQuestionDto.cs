using System;
using System.Collections.Generic;

namespace CareerMind.Application.DTOs.Interview
{
    public class InterviewQuestionDto
    {
        public Guid Id { get; set; }
        public Guid InterviewSessionId { get; set; }

        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public int QuestionOrder { get; set; }

        public List<InterviewAnswerDto> Answers { get; set; } = new List<InterviewAnswerDto>();
    }
}
