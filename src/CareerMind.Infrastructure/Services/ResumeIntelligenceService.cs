using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CareerMind.Application.DTOs.Resume;
using CareerMind.Application.Interfaces;
using CareerMind.Domain.Entities;
using CareerMind.Infrastructure.Data;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace CareerMind.Infrastructure.Services
{
    public class ResumeIntelligenceService : IResumeIntelligenceService
    {
        private readonly CareerMindDbContext _db;
        private readonly ILogger<ResumeIntelligenceService> _logger;
        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB
        private static readonly string[] AllowedExtensions = { ".pdf", ".docx" };
        private static readonly string[] AllowedMimeTypes = { "application/pdf", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" };
        private const string UploadDirectory = "uploads/resumes";

        // ATS scoring weights (must sum to 100)
        private const double WeightStructure = 0.15;
        private const double WeightSkills = 0.30;
        private const double WeightKeywords = 0.20;
        private const double WeightExperience = 0.15;
        private const double WeightEducation = 0.10;
        private const double WeightCompleteness = 0.10;

        public ResumeIntelligenceService(CareerMindDbContext db, ILogger<ResumeIntelligenceService> logger)
        {
            _db = db;
            _logger = logger;
        }

        // ─────────────────────────── UPLOAD ────────────────────────────────
        public async Task<ResumeDto> UploadResumeAsync(Guid userId, ResumeUploadRequest request)
        {
            ValidateRequest(request);

            var profile = await GetProfileAsync(userId);

            // Safe filename
            var ext = Path.GetExtension(request.OriginalFileName).ToLowerInvariant();
            var safeFileName = $"{Guid.NewGuid()}{ext}";
            var dir = Path.Combine(Directory.GetCurrentDirectory(), UploadDirectory);
            Directory.CreateDirectory(dir);
            var storagePath = Path.Combine(dir, safeFileName);

            await using (var fs = new FileStream(storagePath, FileMode.Create))
                await request.Content.CopyToAsync(fs);

            var extractedText = ExtractText(storagePath, ext);
            extractedText = NormalizeText(extractedText);

            var resume = new Resume
            {
                CandidateProfileId = profile.Id,
                FileName = safeFileName,
                OriginalFileName = Path.GetFileName(request.OriginalFileName),
                FileType = request.ContentType,
                FileSize = request.Length,
                StoragePath = storagePath,
                ExtractedText = extractedText,
                UploadedAt = DateTime.UtcNow,
                IsPrimary = !(await _db.Resumes.AnyAsync(r => r.CandidateProfileId == profile.Id && !r.IsDeleted))
            };

            _db.Resumes.Add(resume);
            await _db.SaveChangesAsync();

            return MapToDto(resume, hasAnalysis: false, latestScore: null);
        }

        // ─────────────────────────── LIST / GET ────────────────────────────
        public async Task<List<ResumeDto>> GetResumesAsync(Guid userId)
        {
            var profile = await GetProfileAsync(userId);
            var resumes = await _db.Resumes
                .Where(r => r.CandidateProfileId == profile.Id && !r.IsDeleted)
                .OrderByDescending(r => r.UploadedAt)
                .ToListAsync();

            var result = new List<ResumeDto>();
            foreach (var r in resumes)
            {
                var latest = await _db.ResumeAnalyses
                    .Where(a => a.ResumeId == r.Id && !a.IsDeleted)
                    .OrderByDescending(a => a.AnalyzedAt)
                    .FirstOrDefaultAsync();
                result.Add(MapToDto(r, latest != null, latest?.ATSScore));
            }
            return result;
        }

        public async Task<ResumeDto> GetResumeAsync(Guid userId, Guid resumeId)
        {
            var resume = await GetOwnedResumeAsync(userId, resumeId);
            var latest = await _db.ResumeAnalyses
                .Where(a => a.ResumeId == resume.Id && !a.IsDeleted)
                .OrderByDescending(a => a.AnalyzedAt)
                .FirstOrDefaultAsync();
            return MapToDto(resume, latest != null, latest?.ATSScore);
        }

        // ─────────────────────────── DELETE ────────────────────────────────
        public async Task<bool> DeleteResumeAsync(Guid userId, Guid resumeId)
        {
            var resume = await GetOwnedResumeAsync(userId, resumeId);
            resume.IsDeleted = true;
            resume.DeletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        // ─────────────────────────── SET PRIMARY ───────────────────────────
        public async Task<bool> SetPrimaryAsync(Guid userId, Guid resumeId)
        {
            var profile = await GetProfileAsync(userId);
            var resumes = await _db.Resumes
                .Where(r => r.CandidateProfileId == profile.Id && !r.IsDeleted)
                .ToListAsync();

            foreach (var r in resumes)
                r.IsPrimary = (r.Id == resumeId);

            await _db.SaveChangesAsync();
            return true;
        }

        // ─────────────────────────── ANALYSE ───────────────────────────────
        public async Task<ResumeAnalysisDto> AnalyzeResumeAsync(Guid userId, Guid resumeId, Guid? targetJobId)
        {
            var resume = await GetOwnedResumeAsync(userId, resumeId);
            var profile = await GetFullProfileAsync(resume.CandidateProfileId);

            var resumeText = resume.ExtractedText;
            var textLower = resumeText.ToLowerInvariant();

            // ── 1. Load all skills from DB catalogue ──
            var allSkills = await _db.Skills.Where(s => s.IsActive && !s.IsDeleted).ToListAsync();

            // ── 2. Candidate skills ──
            var candidateSkills = profile.CandidateSkills
                .Where(cs => !cs.IsDeleted)
                .Select(cs => cs.Skill.Name)
                .ToList();

            // ── 3. Skill matching against resumed text ──
            var skillsFoundInResume = new List<string>();
            foreach (var skill in allSkills)
            {
                if (textLower.Contains(skill.Name.ToLowerInvariant()) ||
                    ContainsSkillAlias(textLower, skill.Name))
                {
                    skillsFoundInResume.Add(skill.Name);
                }
            }

            // ── 4. Target job skills (optional) ──
            List<JobSkill> requiredJobSkills = new();
            Job? targetJob = null;
            if (targetJobId.HasValue)
            {
                targetJob = await _db.Jobs
                    .Include(j => j.JobSkills)
                    .ThenInclude(js => js.Skill)
                    .FirstOrDefaultAsync(j => j.Id == targetJobId.Value && !j.IsDeleted);
                if (targetJob != null)
                    requiredJobSkills = targetJob.JobSkills.ToList();
            }

            var missingSkills = requiredJobSkills
                .Where(js => js.IsRequired && !skillsFoundInResume
                    .Any(sf => sf.Equals(js.Skill.Name, StringComparison.OrdinalIgnoreCase)))
                .Select(js => js.Skill.Name)
                .ToList();

            // ── 5. Score calculations ──
            int structureScore = CalculateStructureScore(resumeText);
            int skillsScore = CalculateSkillsScore(skillsFoundInResume, candidateSkills, requiredJobSkills);
            int keywordScore = CalculateKeywordScore(textLower, targetJob);
            int experienceScore = CalculateExperienceScore(profile, textLower);
            int educationScore = CalculateEducationScore(profile, textLower);
            int completenessScore = CalculateCompletenessScore(profile, textLower);

            int atsScore = (int)Math.Round(
                structureScore * WeightStructure +
                skillsScore * WeightSkills +
                keywordScore * WeightKeywords +
                experienceScore * WeightExperience +
                educationScore * WeightEducation +
                completenessScore * WeightCompleteness
            );
            atsScore = Math.Clamp(atsScore, 0, 100);

            // ── 6. Keywords missing ──
            var missingKeywords = GetMissingKeywords(textLower, targetJob);

            // ── 7. Strengths ──
            var strengths = BuildStrengths(profile, skillsFoundInResume, structureScore, experienceScore, educationScore);

            // ── 8. Suggestions ──
            var suggestions = BuildSuggestions(profile, textLower, missingSkills, missingKeywords,
                structureScore, skillsScore, experienceScore, educationScore, completenessScore);

            // ── 9. Persist ──
            var analysis = new ResumeAnalysis
            {
                ResumeId = resume.Id,
                TargetJobId = targetJobId,
                ATSScore = atsScore,
                StructureScore = structureScore,
                SkillsScore = skillsScore,
                KeywordScore = keywordScore,
                ExperienceScore = experienceScore,
                EducationScore = educationScore,
                CompletenessScore = completenessScore,
                SkillsFound = JsonSerializer.Serialize(skillsFoundInResume),
                MissingSkills = JsonSerializer.Serialize(missingSkills),
                MissingKeywords = JsonSerializer.Serialize(missingKeywords),
                Strengths = JsonSerializer.Serialize(strengths),
                Suggestions = JsonSerializer.Serialize(suggestions),
                AnalyzedAt = DateTime.UtcNow
            };

            _db.ResumeAnalyses.Add(analysis);
            await _db.SaveChangesAsync();

            return MapAnalysisToDto(analysis, targetJob?.Title);
        }

        // ─────────────────────────── GET ANALYSES ──────────────────────────
        public async Task<List<ResumeAnalysisDto>> GetAnalysesAsync(Guid userId, Guid resumeId)
        {
            var resume = await GetOwnedResumeAsync(userId, resumeId);
            var analyses = await _db.ResumeAnalyses
                .Where(a => a.ResumeId == resume.Id && !a.IsDeleted)
                .OrderByDescending(a => a.AnalyzedAt)
                .ToListAsync();
            return analyses.Select(a => MapAnalysisToDto(a, null)).ToList();
        }

        public async Task<ResumeAnalysisDto?> GetLatestAnalysisAsync(Guid userId, Guid resumeId)
        {
            var resume = await GetOwnedResumeAsync(userId, resumeId);
            var analysis = await _db.ResumeAnalyses
                .Where(a => a.ResumeId == resume.Id && !a.IsDeleted)
                .OrderByDescending(a => a.AnalyzedAt)
                .FirstOrDefaultAsync();
            return analysis == null ? null : MapAnalysisToDto(analysis, null);
        }

        // ═══════════════════════════ PRIVATE HELPERS ════════════════════════

        // ── File Validation ──
        private static void ValidateRequest(ResumeUploadRequest request)
        {
            if (request == null || request.Length == 0)
                throw new InvalidOperationException("No file was uploaded or file is empty.");

            if (request.Length > MaxFileSizeBytes)
                throw new InvalidOperationException($"File exceeds the maximum allowed size of 10 MB.");

            var ext = Path.GetExtension(request.OriginalFileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
                throw new InvalidOperationException($"File type '{ext}' is not supported. Upload PDF or DOCX.");

            if (!AllowedMimeTypes.Contains(request.ContentType))
                throw new InvalidOperationException("Invalid MIME type. Upload PDF or DOCX.");

            if (request.OriginalFileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                throw new InvalidOperationException("Filename contains invalid characters.");
        }

        // ── Text Extraction ──
        private string ExtractText(string path, string ext)
        {
            try
            {
                if (ext == ".pdf")
                    return ExtractPdfText(path);
                if (ext == ".docx")
                    return ExtractDocxText(path);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not extract text from {Path}", path);
            }
            return string.Empty;
        }

        private static string ExtractPdfText(string path)
        {
            var sb = new StringBuilder();
            using var pdf = PdfDocument.Open(path);
            foreach (Page page in pdf.GetPages())
                sb.AppendLine(page.Text);
            return sb.ToString();
        }

        private static string ExtractDocxText(string path)
        {
            var sb = new StringBuilder();
            using var doc = WordprocessingDocument.Open(path, false);
            var body = doc.MainDocumentPart?.Document?.Body;
            if (body == null) return string.Empty;
            foreach (var para in body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
                sb.AppendLine(para.InnerText);
            return sb.ToString();
        }

        private static string NormalizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            text = Regex.Replace(text, @"[ \t]+", " ");
            text = Regex.Replace(text, @"\n{3,}", "\n\n");
            return text.Trim();
        }

        // ── Score Components ──
        private static int CalculateStructureScore(string text)
        {
            var score = 0;
            var sections = new[] { "experience", "education", "skills", "summary", "objective", "certification", "project", "language" };
            var textLower = text.ToLowerInvariant();
            foreach (var s in sections)
                if (textLower.Contains(s)) score += 10;

            // Has contact info signals
            if (Regex.IsMatch(text, @"[\w\.-]+@[\w\.-]+\.\w+")) score += 10; // email
            if (Regex.IsMatch(text, @"\+?[\d\s\-]{7,}")) score += 5;          // phone
            if (Regex.IsMatch(text, @"linkedin\.com", RegexOptions.IgnoreCase)) score += 5;
            return Math.Min(score, 100);
        }

        private static int CalculateSkillsScore(List<string> foundInResume, List<string> candidateSkills, List<JobSkill> jobSkills)
        {
            if (!jobSkills.Any())
            {
                // No target job – score based on breadth of skills found
                return Math.Min(foundInResume.Count * 8, 100);
            }
            var required = jobSkills.Where(js => js.IsRequired).ToList();
            if (!required.Any()) return 70;
            var matched = required.Count(js =>
                foundInResume.Any(f => f.Equals(js.Skill.Name, StringComparison.OrdinalIgnoreCase)) ||
                candidateSkills.Any(cs => cs.Equals(js.Skill.Name, StringComparison.OrdinalIgnoreCase)));
            return (int)Math.Round((double)matched / required.Count * 100);
        }

        private static int CalculateKeywordScore(string textLower, Job? job)
        {
            if (job == null) return 60;
            var allKeywords = new List<string>();
            if (!string.IsNullOrWhiteSpace(job.Title)) allKeywords.AddRange(job.Title.Split(' '));
            if (!string.IsNullOrWhiteSpace(job.Requirements)) allKeywords.AddRange(job.Requirements.Split(' ').Take(20));
            if (!string.IsNullOrWhiteSpace(job.Description)) allKeywords.AddRange(job.Description.Split(' ').Take(20));

            allKeywords = allKeywords
                .Select(k => k.ToLowerInvariant().Trim('.', ',', ':', ';'))
                .Where(k => k.Length > 3)
                .Distinct()
                .ToList();

            if (!allKeywords.Any()) return 60;
            int matched = allKeywords.Count(kw => textLower.Contains(kw));
            return Math.Min((int)Math.Round((double)matched / allKeywords.Count * 100), 100);
        }

        private static int CalculateExperienceScore(CandidateProfile profile, string textLower)
        {
            int score = 40;
            if (profile.WorkExperiences.Any(we => !we.IsDeleted)) score += 30;
            if (profile.WorkExperiences.Count(we => !we.IsDeleted) >= 2) score += 15;
            if (textLower.Contains("achieve") || textLower.Contains("improv") || textLower.Contains("led") || textLower.Contains("increased")) score += 15;
            return Math.Min(score, 100);
        }

        private static int CalculateEducationScore(CandidateProfile profile, string textLower)
        {
            int score = 50;
            if (profile.Educations.Any(e => !e.IsDeleted)) score += 30;
            if (!string.IsNullOrWhiteSpace(profile.Educations.FirstOrDefault(e => !e.IsDeleted)?.Degree) &&
                profile.Educations.Any(e => !e.IsDeleted && !string.IsNullOrWhiteSpace(e.Grade))) score += 20;
            return Math.Min(score, 100);
        }

        private static int CalculateCompletenessScore(CandidateProfile profile, string textLower)
        {
            int score = 0;
            if (!string.IsNullOrWhiteSpace(profile.Headline)) score += 15;
            if (!string.IsNullOrWhiteSpace(profile.CareerSummary) || textLower.Contains("summary") || textLower.Contains("objective")) score += 15;
            if (!string.IsNullOrWhiteSpace(profile.LinkedInUrl) || textLower.Contains("linkedin")) score += 10;
            if (!string.IsNullOrWhiteSpace(profile.GitHubUrl) || textLower.Contains("github")) score += 10;
            if (profile.CandidateSkills.Any(cs => !cs.IsDeleted)) score += 20;
            if (profile.WorkExperiences.Any(we => !we.IsDeleted)) score += 15;
            if (profile.Educations.Any(e => !e.IsDeleted)) score += 15;
            return Math.Min(score, 100);
        }

        private static List<string> GetMissingKeywords(string textLower, Job? job)
        {
            if (job == null) return new List<string>();
            var keywords = new List<string>();
            if (!string.IsNullOrWhiteSpace(job.Requirements))
                keywords.AddRange(job.Requirements.Split(new[] { ' ', '\n', ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(k => k.ToLowerInvariant().Trim('.', ',', ':', ';'))
                    .Where(k => k.Length > 3));

            return keywords
                .Distinct()
                .Where(kw => !textLower.Contains(kw))
                .Take(10)
                .ToList();
        }

        private static bool ContainsSkillAlias(string textLower, string skillName)
        {
            var aliases = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["React"] = new() { "react.js", "reactjs" },
                ["Node.js"] = new() { "nodejs", "node js" },
                ["ASP.NET Core"] = new() { "aspnet core", "asp.net core", "aspnetcore" },
                ["SQL Server"] = new() { "mssql", "ms sql", "microsoft sql" },
                ["JavaScript"] = new() { "js" },
                ["TypeScript"] = new() { "ts" },
                ["Docker"] = new() { "containerization", "docker container" },
                [".NET"] = new() { "dotnet", ".net core", "dotnet core" },
            };
            if (aliases.TryGetValue(skillName, out var list))
                return list.Any(alias => textLower.Contains(alias));
            return false;
        }

        private static List<string> BuildStrengths(CandidateProfile profile, List<string> skillsFound, int structure, int experience, int education)
        {
            var strengths = new List<string>();
            if (structure >= 70) strengths.Add("Well-structured resume with clear sections.");
            if (skillsFound.Count >= 5) strengths.Add($"Strong technical skill breadth — {skillsFound.Count} relevant skills identified.");
            if (experience >= 70) strengths.Add("Demonstrates solid professional experience.");
            if (education >= 80) strengths.Add("Educational background aligns well.");
            if (!string.IsNullOrWhiteSpace(profile.LinkedInUrl)) strengths.Add("Professional online presence (LinkedIn) included.");
            if (!string.IsNullOrWhiteSpace(profile.GitHubUrl)) strengths.Add("GitHub profile included — a strong signal for technical roles.");
            if (!strengths.Any()) strengths.Add("Resume uploaded and ready for analysis. Complete your profile for richer insights.");
            return strengths;
        }

        private static List<object> BuildSuggestions(
            CandidateProfile profile, string textLower,
            List<string> missingSkills, List<string> missingKeywords,
            int structure, int skills, int experience, int education, int completeness)
        {
            var suggestions = new List<object>();

            if (missingSkills.Any())
            {
                foreach (var s in missingSkills.Take(3))
                    suggestions.Add(new { Priority = "Critical", Category = "Skills", Text = $"Add required skill: {s} to your profile and resume." });
            }

            if (missingKeywords.Any())
            {
                suggestions.Add(new { Priority = "High", Category = "Keywords", Text = $"Include missing keywords: {string.Join(", ", missingKeywords.Take(5))}." });
            }

            if (experience < 70)
                suggestions.Add(new { Priority = "High", Category = "Experience", Text = "Add quantifiable achievements (e.g., 'Reduced load time by 30%') to work experience." });

            if (!textLower.Contains("github") && string.IsNullOrWhiteSpace(profile.GitHubUrl))
                suggestions.Add(new { Priority = "Medium", Category = "Links", Text = "Add a GitHub profile link to showcase your projects." });

            if (!textLower.Contains("linkedin") && string.IsNullOrWhiteSpace(profile.LinkedInUrl))
                suggestions.Add(new { Priority = "Medium", Category = "Links", Text = "Add your LinkedIn URL for professional credibility." });

            if (structure < 70)
                suggestions.Add(new { Priority = "Medium", Category = "Structure", Text = "Ensure your resume has clearly labelled sections: Summary, Experience, Education, Skills." });

            if (!profile.Certifications.Any(c => !c.IsDeleted))
                suggestions.Add(new { Priority = "Low", Category = "Certifications", Text = "Add relevant certifications to strengthen your profile." });

            if (completeness < 60)
                suggestions.Add(new { Priority = "Medium", Category = "Profile", Text = "Complete your CareerMind profile (headline, summary, skills) to improve matching accuracy." });

            return suggestions;
        }

        // ── DB Helpers ──
        private async Task<CandidateProfile> GetProfileAsync(Guid userId)
        {
            var profile = await _db.CandidateProfiles
                .FirstOrDefaultAsync(cp => cp.UserId == userId && !cp.IsDeleted)
                ?? throw new InvalidOperationException("Candidate profile not found.");
            return profile;
        }

        private async Task<CandidateProfile> GetFullProfileAsync(Guid profileId)
        {
            var profile = await _db.CandidateProfiles
                .Include(cp => cp.CandidateSkills.Where(cs => !cs.IsDeleted))
                    .ThenInclude(cs => cs.Skill)
                .Include(cp => cp.WorkExperiences.Where(we => !we.IsDeleted))
                .Include(cp => cp.Educations.Where(e => !e.IsDeleted))
                .Include(cp => cp.Certifications.Where(c => !c.IsDeleted))
                .FirstOrDefaultAsync(cp => cp.Id == profileId && !cp.IsDeleted)
                ?? throw new InvalidOperationException("Candidate profile not found.");
            return profile;
        }

        private async Task<Resume> GetOwnedResumeAsync(Guid userId, Guid resumeId)
        {
            var profile = await GetProfileAsync(userId);
            var resume = await _db.Resumes
                .FirstOrDefaultAsync(r => r.Id == resumeId && r.CandidateProfileId == profile.Id && !r.IsDeleted)
                ?? throw new InvalidOperationException("Resume not found or access denied.");
            return resume;
        }

        // ── Mapping ──
        private static ResumeDto MapToDto(Resume r, bool hasAnalysis, int? latestScore) => new()
        {
            Id = r.Id,
            OriginalFileName = r.OriginalFileName,
            FileType = r.FileType,
            FileSize = r.FileSize,
            IsPrimary = r.IsPrimary,
            UploadedAt = r.UploadedAt,
            HasAnalysis = hasAnalysis,
            LatestATSScore = latestScore
        };

        private static ResumeAnalysisDto MapAnalysisToDto(ResumeAnalysis a, string? jobTitle)
        {
            var dto = new ResumeAnalysisDto
            {
                Id = a.Id,
                ResumeId = a.ResumeId,
                TargetJobId = a.TargetJobId,
                TargetJobTitle = jobTitle,
                ATSScore = a.ATSScore,
                ATSInterpretation = InterpretScore(a.ATSScore),
                StructureScore = a.StructureScore,
                SkillsScore = a.SkillsScore,
                KeywordScore = a.KeywordScore,
                ExperienceScore = a.ExperienceScore,
                EducationScore = a.EducationScore,
                CompletenessScore = a.CompletenessScore,
                AnalyzedAt = a.AnalyzedAt
            };

            if (!string.IsNullOrWhiteSpace(a.SkillsFound))
                dto.SkillsFound = JsonSerializer.Deserialize<List<string>>(a.SkillsFound) ?? new();
            if (!string.IsNullOrWhiteSpace(a.MissingSkills))
                dto.MissingSkills = JsonSerializer.Deserialize<List<string>>(a.MissingSkills) ?? new();
            if (!string.IsNullOrWhiteSpace(a.MissingKeywords))
                dto.MissingKeywords = JsonSerializer.Deserialize<List<string>>(a.MissingKeywords) ?? new();
            if (!string.IsNullOrWhiteSpace(a.Strengths))
                dto.Strengths = JsonSerializer.Deserialize<List<string>>(a.Strengths) ?? new();

            if (!string.IsNullOrWhiteSpace(a.Suggestions))
            {
                try
                {
                    var raw = JsonSerializer.Deserialize<List<JsonElement>>(a.Suggestions) ?? new();
                    dto.Suggestions = raw.Select(e => new SuggestionDto
                    {
                        Priority = e.TryGetProperty("Priority", out var p) ? p.GetString() ?? "" : "",
                        Category = e.TryGetProperty("Category", out var c) ? c.GetString() ?? "" : "",
                        Text = e.TryGetProperty("Text", out var t) ? t.GetString() ?? "" : ""
                    }).ToList();
                }
                catch { /* ignore deserialization errors */ }
            }

            return dto;
        }

        private static string InterpretScore(int score) => score switch
        {
            >= 85 => "Excellent",
            >= 70 => "Strong",
            >= 50 => "Needs Improvement",
            _ => "Critical"
        };
    }
}
