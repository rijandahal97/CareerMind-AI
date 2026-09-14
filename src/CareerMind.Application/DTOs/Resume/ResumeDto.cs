using System;

namespace CareerMind.Application.DTOs.Resume
{
    public class ResumeDto
    {
        public Guid Id { get; set; }
        public string OriginalFileName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public bool IsPrimary { get; set; }
        public DateTime UploadedAt { get; set; }
        public bool HasAnalysis { get; set; }
        public int? LatestATSScore { get; set; }
    }
}
