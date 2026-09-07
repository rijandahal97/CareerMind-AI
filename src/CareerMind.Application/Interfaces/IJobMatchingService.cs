using System.Threading.Tasks;

namespace CareerMind.Application.Interfaces.AI
{
    public interface IJobMatchingService
    {
        Task<decimal> CalculateMatchScoreAsync(string candidateProfileData, string jobRequirementsData);
        Task<string> GenerateMatchExplanationAsync(string candidateProfileData, string jobRequirementsData);
    }
}
