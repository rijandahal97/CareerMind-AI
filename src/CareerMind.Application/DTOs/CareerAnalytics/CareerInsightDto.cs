namespace CareerMind.Application.DTOs.CareerAnalytics
{
    public class CareerInsightDto
    {
        public string InsightId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // e.g., "Success", "Warning", "Info"
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string RecommendedAction { get; set; } = string.Empty;
        public string ActionLink { get; set; } = string.Empty; // Route to module
    }
}
