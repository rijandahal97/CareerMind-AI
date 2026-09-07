using System.Threading.Tasks;

namespace CareerMind.Application.Interfaces.AI
{
    public interface IInterviewAIService
    {
        Task<string> GenerateMockInterviewQuestionAsync(string jobRole, string difficulty);
        Task<object> EvaluateInterviewResponseAsync(string question, string response); // Object temporary until DTOs
    }
}
