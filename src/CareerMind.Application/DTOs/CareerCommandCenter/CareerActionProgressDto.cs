using System;

namespace CareerMind.Application.DTOs.CareerCommandCenter
{
    public class CareerActionProgressDto
    {
        public Guid Id { get; set; }
        public Guid CandidateProfileId { get; set; }
        public string ActionKey { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
