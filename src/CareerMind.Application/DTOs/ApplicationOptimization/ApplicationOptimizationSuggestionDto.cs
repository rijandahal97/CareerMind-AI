using System;
using System.Collections.Generic;
using CareerMind.Domain.Enums;

namespace CareerMind.Application.DTOs.ApplicationOptimization
{
    public class ApplicationOptimizationSuggestionDto
    {
        public Guid Id { get; set; }
        public SuggestionCategory Category { get; set; }
        public SuggestionPriority Priority { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? SuggestedAction { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
