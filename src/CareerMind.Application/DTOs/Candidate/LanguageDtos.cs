using System;
using CareerMind.Domain.Enums;

namespace CareerMind.Application.DTOs.Candidate
{
    public class LanguageDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
    }

    public class CandidateLanguageDto
    {
        public Guid Id { get; set; }
        public Guid LanguageId { get; set; }
        public string LanguageName { get; set; } = string.Empty;
        public ProficiencyLevel ProficiencyLevel { get; set; }
    }

    public class AddCandidateLanguageDto
    {
        public Guid LanguageId { get; set; }
        public ProficiencyLevel ProficiencyLevel { get; set; }
    }

    public class UpdateCandidateLanguageDto
    {
        public ProficiencyLevel ProficiencyLevel { get; set; }
    }
}
