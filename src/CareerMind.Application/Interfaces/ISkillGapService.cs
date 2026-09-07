using System.Threading.Tasks;

namespace CareerMind.Application.Interfaces.AI
{
    public interface ISkillGapService
    {
        Task<object> AnalyzeSkillGapAsync(string currentSkills, string requiredSkills); // Object temporary until DTOs
    }
}
