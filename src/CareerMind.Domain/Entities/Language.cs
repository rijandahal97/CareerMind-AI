using System.Collections.Generic;

namespace CareerMind.Domain.Entities
{
    public class Language : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public ICollection<CandidateLanguage> CandidateLanguages { get; set; } = new List<CandidateLanguage>();
    }
}
