using System;
using System.ComponentModel.DataAnnotations;

namespace CareerMind.Application.DTOs.Employer
{
    public class EmployerProfileDto
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? CompanyDescription { get; set; }
        public string? Industry { get; set; }
        public string? CompanySize { get; set; }
        public string? Website { get; set; }
        public string? LogoUrl { get; set; }
        public string? Location { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string VerificationStatus { get; set; } = string.Empty;
    }

    public class UpdateEmployerProfileRequest
    {
        [Required]
        [MaxLength(100)]
        public string CompanyName { get; set; } = string.Empty;
        
        [MaxLength(2000)]
        public string? CompanyDescription { get; set; }
        
        [MaxLength(100)]
        public string? Industry { get; set; }
        
        [MaxLength(50)]
        public string? CompanySize { get; set; }
        
        [Url]
        [MaxLength(200)]
        public string? Website { get; set; }
        
        [Url]
        [MaxLength(500)]
        public string? LogoUrl { get; set; }
        
        [MaxLength(200)]
        public string? Location { get; set; }
        
        [EmailAddress]
        [MaxLength(100)]
        public string? ContactEmail { get; set; }
        
        [Phone]
        [MaxLength(50)]
        public string? ContactPhone { get; set; }
    }
}
