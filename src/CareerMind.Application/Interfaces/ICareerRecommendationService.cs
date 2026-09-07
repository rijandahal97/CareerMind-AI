using System.Threading.Tasks;

namespace CareerMind.Application.Interfaces.AI
{
    public interface ICareerRecommendationService
    {
        Task<object> GenerateCareerRoadmapAsync(string candidateProfileData, string careerGoal); // Object temporary until DTOs
    }
}
