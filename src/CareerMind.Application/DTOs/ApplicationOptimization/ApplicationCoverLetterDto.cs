using System;

namespace CareerMind.Application.DTOs.ApplicationOptimization
{
    public class ApplicationCoverLetterDto
    {
        public Guid JobId { get; set; }
        public string CoverLetter { get; set; } = string.Empty;
    }
}
