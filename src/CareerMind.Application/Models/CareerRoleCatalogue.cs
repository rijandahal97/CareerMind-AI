using System.Collections.Generic;
using System.Linq;

namespace CareerMind.Application.Models
{
    public static class CareerRoleCatalogue
    {
        public static List<CareerRoleDefinition> GetRoles()
        {
            return new List<CareerRoleDefinition>
            {
                new CareerRoleDefinition
                {
                    RoleName = "Junior .NET Developer",
                    CareerCategory = "Software Engineering",
                    Description = "Entry-level developer focused on building .NET applications under supervision and learning core engineering practices.",
                    CareerProgressionLevel = 1,
                    SeniorityLevel = "Junior",
                    MinimumExperienceYears = 0,
                    RequiredSkills = new List<string> { "C#", ".NET", "SQL", "Git" },
                    PreferredSkills = new List<string> { "ASP.NET Core", "Entity Framework", "REST API" },
                    RelevantEducation = new List<string> { "Computer Science", "Software Engineering" },
                    RelatedRoles = new List<string> { "Backend Developer", "Full Stack Developer" }
                },
                new CareerRoleDefinition
                {
                    RoleName = "Backend Developer",
                    CareerCategory = "Software Engineering",
                    Description = "Mid-level developer responsible for designing, implementing, and maintaining backend APIs, business logic, and databases.",
                    CareerProgressionLevel = 2,
                    SeniorityLevel = "Mid-Level",
                    MinimumExperienceYears = 2,
                    RequiredSkills = new List<string> { "C#", "ASP.NET Core", "SQL Server", "Entity Framework", "REST API" },
                    PreferredSkills = new List<string> { "Docker", "Azure", "Microservices", "Unit Testing" },
                    RelevantEducation = new List<string> { "Computer Science" },
                    RelatedRoles = new List<string> { "Senior Backend Developer", "Junior .NET Developer" }
                },
                new CareerRoleDefinition
                {
                    RoleName = "Senior Backend Developer",
                    CareerCategory = "Software Engineering",
                    Description = "Experienced developer who can independently design scalable architectures, optimize performance, and mentor other engineers.",
                    CareerProgressionLevel = 3,
                    SeniorityLevel = "Senior",
                    MinimumExperienceYears = 5,
                    RequiredSkills = new List<string> { "C#", "ASP.NET Core", "SQL Server", "Docker", "System Design", "Microservices" },
                    PreferredSkills = new List<string> { "Azure", "Kubernetes", "CI/CD", "NoSQL" },
                    RelevantEducation = new List<string> { "Computer Science" },
                    RelatedRoles = new List<string> { "Software Architect", "Backend Developer" }
                },
                new CareerRoleDefinition
                {
                    RoleName = "Software Architect",
                    CareerCategory = "Software Engineering",
                    Description = "Technical leader who designs the overarching system architecture, selects technologies, and guides major technical decisions.",
                    CareerProgressionLevel = 4,
                    SeniorityLevel = "Architect",
                    MinimumExperienceYears = 8,
                    RequiredSkills = new List<string> { "System Design", "Microservices", "Cloud Architecture", "C#", "Database Design", "Leadership" },
                    PreferredSkills = new List<string> { "Azure", "Kubernetes" },
                    RelevantEducation = new List<string> { "Computer Science" },
                    RelatedRoles = new List<string> { "Senior Backend Developer", "CTO" }
                },
                new CareerRoleDefinition
                {
                    RoleName = "Frontend Developer",
                    CareerCategory = "Software Engineering",
                    Description = "Developer focused on building user interfaces, ensuring responsiveness, and creating seamless user experiences.",
                    CareerProgressionLevel = 2,
                    SeniorityLevel = "Mid-Level",
                    MinimumExperienceYears = 2,
                    RequiredSkills = new List<string> { "HTML", "CSS", "JavaScript", "React" },
                    PreferredSkills = new List<string> { "TypeScript", "Redux", "TailwindCSS" },
                    RelevantEducation = new List<string> { "Computer Science", "Web Development" },
                    RelatedRoles = new List<string> { "Full Stack Developer", "UI/UX Designer" }
                },
                new CareerRoleDefinition
                {
                    RoleName = "Full Stack Developer",
                    CareerCategory = "Software Engineering",
                    Description = "Developer capable of working on both frontend UI and backend API systems.",
                    CareerProgressionLevel = 2,
                    SeniorityLevel = "Mid-Level",
                    MinimumExperienceYears = 3,
                    RequiredSkills = new List<string> { "C#", "ASP.NET Core", "React", "JavaScript", "SQL Server" },
                    PreferredSkills = new List<string> { "TypeScript", "Docker" },
                    RelevantEducation = new List<string> { "Computer Science" },
                    RelatedRoles = new List<string> { "Backend Developer", "Frontend Developer" }
                }
            };
        }

        public static List<CareerRoleDefinition> GetRolesByCategory(string category)
        {
            return GetRoles().Where(r => r.CareerCategory == category).ToList();
        }
    }
}
