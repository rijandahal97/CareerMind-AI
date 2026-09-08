using System;
using CareerMind.Domain.Enums;

namespace CareerMind.Domain.Entities
{
    public class CandidateLanguage : BaseEntity
    {
        public Guid CandidateProfileId { get; set; }
        public CandidateProfile CandidateProfile { get; set; } = null!;
        
        public Guid LanguageId { get; set; }
        public Language Language { get; set; } = null!;
        
        public ProficiencyLevel ProficiencyLevel { get; set; }
    }
}
