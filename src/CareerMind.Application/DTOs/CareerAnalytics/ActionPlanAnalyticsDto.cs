namespace CareerMind.Application.DTOs.CareerAnalytics
{
    public class ActionPlanAnalyticsDto
    {
        public int TotalPlannedActions { get; set; }
        public int CompletedActions { get; set; }
        public int InProgressActions { get; set; }
        public int PendingActions { get; set; }
        public decimal OverallCompletionPercentage { get; set; }
        
        public string CurrentPriorityAction { get; set; } = string.Empty;
        
        public decimal Progress30Days { get; set; }
        public decimal Progress60Days { get; set; }
        public decimal Progress90Days { get; set; }
    }
}
