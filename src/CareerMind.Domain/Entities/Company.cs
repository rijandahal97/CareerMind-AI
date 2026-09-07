using System;
using System.Collections.Generic;

namespace CareerMind.Domain.Entities
{
    public class Company : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? LogoUrl { get; set; }
        
        public ICollection<Job> Jobs { get; set; } = new List<Job>();
    }
}
