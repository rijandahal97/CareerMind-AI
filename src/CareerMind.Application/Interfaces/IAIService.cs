using System.Threading.Tasks;

namespace CareerMind.Application.Interfaces.AI
{
    public interface IAIService
    {
        Task<string> GetCompletionAsync(string prompt);
        Task<T> GetStructuredDataAsync<T>(string prompt) where T : class;
    }
}
