using System;

namespace CareerMind.Application.DTOs.CareerGap
{
    public class SkillGapDetailDto
    {
        public Guid SkillId { get; set; }
        public string SkillName { get; set; } = string.Empty;
        public string CurrentProficiency { get; set; } = string.Empty;
        public string RequiredProficiency { get; set; } = string.Empty;
        public string Importance { get; set; } = string.Empty;
        public string GapSeverity { get; set; } = string.Empty;
        public int Priority { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
    }
}
