using System;
using CareerMind.Domain.Enums;

namespace CareerMind.Domain.Entities
{
    public class ApplicationOptimizationSuggestion : BaseEntity
    {
        public Guid ApplicationOptimizationId { get; set; }
        public ApplicationOptimization ApplicationOptimization { get; set; } = null!;

        public SuggestionCategory Category { get; set; }
        public SuggestionPriority Priority { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? SuggestedAction { get; set; }
        public bool IsCompleted { get; set; } = false;
    }
}
