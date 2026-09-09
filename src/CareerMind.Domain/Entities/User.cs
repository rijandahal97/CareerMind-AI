using System;
using System.Collections.Generic;

namespace CareerMind.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public Guid RoleId { get; set; }
        public Role Role { get; set; } = null!;

        public CandidateProfile? CandidateProfile { get; set; }
        public EmployerProfile? EmployerProfile { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
