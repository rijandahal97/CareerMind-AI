using System.Threading.Tasks;

namespace CareerMind.Application.Interfaces.AI
{
    public interface IAIChatService
    {
        Task<string> ChatWithAssistantAsync(string message, string contextHistory);
    }
}
