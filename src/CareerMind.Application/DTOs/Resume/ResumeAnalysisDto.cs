using System;
using System.Collections.Generic;

namespace CareerMind.Application.DTOs.Resume
{
    public class ResumeAnalysisDto
    {
        public Guid Id { get; set; }
        public Guid ResumeId { get; set; }
        public Guid? TargetJobId { get; set; }
        public string? TargetJobTitle { get; set; }

        // Overall
        public int ATSScore { get; set; }
        public string ATSInterpretation { get; set; } = string.Empty;

        // Component Scores
        public int StructureScore { get; set; }
        public int SkillsScore { get; set; }
        public int KeywordScore { get; set; }
        public int ExperienceScore { get; set; }
        public int EducationScore { get; set; }
        public int CompletenessScore { get; set; }

        // Detailed Findings
        public List<string> SkillsFound { get; set; } = new();
        public List<string> MissingSkills { get; set; } = new();
        public List<string> MissingKeywords { get; set; } = new();
        public List<string> Strengths { get; set; } = new();
        public List<SuggestionDto> Suggestions { get; set; } = new();

        public DateTime AnalyzedAt { get; set; }
    }

    public class SuggestionDto
    {
        public string Priority { get; set; } = string.Empty; // Critical, High, Medium, Low
        public string Category { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
}
