using CareerMind.Domain.Entities;
using CareerMind.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace CareerMind.Infrastructure.Data.Seed
{
    public static class CareerMindDataSeeder
    {
        public static async Task SeedAsync(CareerMindDbContext context)
        {
            if (!await context.Languages.AnyAsync())
            {
                var languages = new[]
                {
                    new Language { Name = "English", Code = "en" },
                    new Language { Name = "Nepali", Code = "ne" },
                    new Language { Name = "Hindi", Code = "hi" },
                    new Language { Name = "Spanish", Code = "es" },
                    new Language { Name = "French", Code = "fr" },
                    new Language { Name = "German", Code = "de" },
                    new Language { Name = "Japanese", Code = "ja" },
                    new Language { Name = "Chinese", Code = "zh" }
                };
                context.Languages.AddRange(languages);
            }

            if (!await context.Skills.AnyAsync())
            {
                var skills = new[]
                {
                    new Skill { Name = "C#", Category = "Programming Languages", SkillType = SkillType.Technical },
                    new Skill { Name = "Java", Category = "Programming Languages", SkillType = SkillType.Technical },
                    new Skill { Name = "Python", Category = "Programming Languages", SkillType = SkillType.Technical },
                    new Skill { Name = "JavaScript", Category = "Programming Languages", SkillType = SkillType.Technical },
                    new Skill { Name = "TypeScript", Category = "Programming Languages", SkillType = SkillType.Technical },
                    new Skill { Name = "React", Category = "Frontend Frameworks", SkillType = SkillType.Technical },
                    new Skill { Name = "Angular", Category = "Frontend Frameworks", SkillType = SkillType.Technical },
                    new Skill { Name = "Vue", Category = "Frontend Frameworks", SkillType = SkillType.Technical },
                    new Skill { Name = "ASP.NET Core", Category = "Backend Frameworks", SkillType = SkillType.Technical },
                    new Skill { Name = "Node.js", Category = "Backend Frameworks", SkillType = SkillType.Technical },
                    new Skill { Name = "SQL", Category = "Databases", SkillType = SkillType.Technical },
                    new Skill { Name = "SQL Server", Category = "Databases", SkillType = SkillType.Technical },
                    new Skill { Name = "PostgreSQL", Category = "Databases", SkillType = SkillType.Technical },
                    new Skill { Name = "MongoDB", Category = "Databases", SkillType = SkillType.Technical },
                    new Skill { Name = "Docker", Category = "DevOps", SkillType = SkillType.Tool },
                    new Skill { Name = "Git", Category = "Version Control", SkillType = SkillType.Tool },
                    new Skill { Name = "GitHub", Category = "Version Control", SkillType = SkillType.Tool },
                    new Skill { Name = "Azure", Category = "Cloud", SkillType = SkillType.Technical },
                    new Skill { Name = "AWS", Category = "Cloud", SkillType = SkillType.Technical },
                    new Skill { Name = "Machine Learning", Category = "AI & Data", SkillType = SkillType.Technical },
                    new Skill { Name = "Artificial Intelligence", Category = "AI & Data", SkillType = SkillType.Technical },
                    new Skill { Name = "Data Analysis", Category = "Data", SkillType = SkillType.Technical },
                    new Skill { Name = "UI/UX", Category = "Design", SkillType = SkillType.Technical },
                    new Skill { Name = "Figma", Category = "Design", SkillType = SkillType.Tool }
                };
                context.Skills.AddRange(skills);
            }

            await context.SaveChangesAsync();
        }
    }
}
