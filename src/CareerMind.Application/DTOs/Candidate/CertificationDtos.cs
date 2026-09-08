using System;

namespace CareerMind.Application.DTOs.Candidate
{
    public class CertificationDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string IssuingOrganization { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? CredentialId { get; set; }
        public string? CredentialUrl { get; set; }
        public string? Description { get; set; }
    }

    public class AddCertificationDto
    {
        public string Name { get; set; } = string.Empty;
        public string IssuingOrganization { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? CredentialId { get; set; }
        public string? CredentialUrl { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateCertificationDto : AddCertificationDto
    {
    }
}
