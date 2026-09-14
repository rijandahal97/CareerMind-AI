using System;
using System.Collections.Generic;

namespace CareerMind.Domain.Entities
{
    public class Resume : BaseEntity
    {
        public Guid CandidateProfileId { get; set; }
        public CandidateProfile CandidateProfile { get; set; } = null!;

        public string FileName { get; set; } = string.Empty; // stored safe filename
        public string OriginalFileName { get; set; } = string.Empty; // original upload name
        public string FileType { get; set; } = string.Empty; // e.g., "application/pdf"
        public long FileSize { get; set; }
        public string StoragePath { get; set; } = string.Empty; // physical or cloud path
        public string ExtractedText { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsPrimary { get; set; }

        public ICollection<ResumeAnalysis> Analyses { get; set; } = new List<ResumeAnalysis>();
    }
}
