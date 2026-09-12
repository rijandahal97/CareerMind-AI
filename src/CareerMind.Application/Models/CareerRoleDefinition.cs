using System;
using System.Collections.Generic;

namespace CareerMind.Application.Models
{
    public class CareerRoleDefinition
    {
        public string RoleName { get; set; } = string.Empty;
        public string CareerCategory { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CareerProgressionLevel { get; set; }
        public string SeniorityLevel { get; set; } = string.Empty;
        public int MinimumExperienceYears { get; set; }
        public List<string> RequiredSkills { get; set; } = new();
        public List<string> PreferredSkills { get; set; } = new();
        public List<string> RelevantEducation { get; set; } = new();
        public List<string> RelatedRoles { get; set; } = new();
    }
}
