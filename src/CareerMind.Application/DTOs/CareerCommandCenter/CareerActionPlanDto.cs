using System.Collections.Generic;

namespace CareerMind.Application.DTOs.CareerCommandCenter
{
    public class CareerActionPlanDto
    {
        public List<CareerActionDto> ThirtyDayActions { get; set; } = new List<CareerActionDto>();
        public List<CareerActionDto> SixtyDayActions { get; set; } = new List<CareerActionDto>();
        public List<CareerActionDto> NinetyDayActions { get; set; } = new List<CareerActionDto>();
        public int TotalActions { get; set; }
        public int CompletedActions { get; set; }
        public decimal ProgressPercentage { get; set; }
    }
}
