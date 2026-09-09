using System;
using System.Collections.Generic;

namespace CareerMind.Domain.Entities
{
    public class EmployerProfile : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        
        public string CompanyName { get; set; } = string.Empty;
        public string? CompanyDescription { get; set; }
        public string? Industry { get; set; }
        public string? CompanySize { get; set; } // e.g., "1-10", "11-50", "51-200"
        public string? Website { get; set; }
        public string? LogoUrl { get; set; }
        public string? Location { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        
        public string VerificationStatus { get; set; } = "Pending"; // Pending, Verified, Rejected
        
        public ICollection<Job> Jobs { get; set; } = new List<Job>();
    }
}
