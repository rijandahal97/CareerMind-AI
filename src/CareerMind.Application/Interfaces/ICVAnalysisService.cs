using System.Threading.Tasks;

namespace CareerMind.Application.Interfaces.AI
{
    public interface ICVAnalysisService
    {
        Task<string> AnalyzeResumeAsync(byte[] resumeContent, string filename);
        Task<object> ExtractSkillsAsync(string resumeText); // Using object temporarily, should be a structured DTO later
    }
}
