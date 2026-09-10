using System;
using System.Collections.Generic;

namespace CareerMind.Application.DTOs.Job
{
    public class JobMatchResultDto
    {
        public Guid JobId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        
        public decimal OverallMatchScore { get; set; }
        public decimal SkillMatchScore { get; set; }
        public decimal ExperienceMatchScore { get; set; }
        public decimal EducationMatchScore { get; set; }
        public decimal PreferenceMatchScore { get; set; }

        public List<string> MatchedSkills { get; set; } = new();
        public List<MissingSkillDto> MissingSkills { get; set; } = new();

        public string ExperienceStatus { get; set; } = string.Empty;
        public string EducationStatus { get; set; } = string.Empty;
        public string PreferenceStatus { get; set; } = string.Empty;

        public List<string> Strengths { get; set; } = new();
        public List<string> Gaps { get; set; } = new();
        public List<string> Explanations { get; set; } = new();
    }

    public class MissingSkillDto
    {
        public string SkillName { get; set; } = string.Empty;
        public string RequiredProficiency { get; set; } = string.Empty;
        public int Importance { get; set; }
        public string GapStatus { get; set; } = string.Empty;
    }
}
