namespace CareerMind.Application.DTOs.CareerCommandCenter
{
    public class CareerBlockerDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = "Medium"; // Critical, High, Medium, Low
        public decimal ImpactScore { get; set; }
        public string RelatedArea { get; set; } = string.Empty; // Skills, Resume, Profile, Experience, Career Direction
        public string RecommendedAction { get; set; } = string.Empty;
        public string RelatedModule { get; set; } = string.Empty;
    }
}
