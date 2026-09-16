using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CareerMind.Application.Interfaces;
using CareerMind.Application.DTOs.Interview;
using CareerMind.Domain.Entities;
using CareerMind.Domain.Enums;
using CareerMind.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace CareerMind.Infrastructure.Services
{
    public class CareerInterviewService : ICareerInterviewService
    {
        private readonly CareerMindDbContext _context;
        private readonly ILogger<CareerInterviewService> _logger;

        public CareerInterviewService(CareerMindDbContext context, ILogger<CareerInterviewService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<InterviewSessionDto> StartInterviewAsync(Guid candidateProfileId, StartInterviewRequestDto request)
        {
            var profile = await _context.CandidateProfiles
                .Include(p => p.CandidateSkills).ThenInclude(cs => cs.Skill)
                .Include(p => p.WorkExperiences)
                .FirstOrDefaultAsync(p => p.Id == candidateProfileId);

            if (profile == null) throw new KeyNotFoundException("Candidate profile not found.");

            Job targetJob = null;
            if (request.TargetJobId.HasValue)
            {
                targetJob = await _context.Jobs
                    .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
                    .FirstOrDefaultAsync(j => j.Id == request.TargetJobId.Value);
            }

            var session = new InterviewSession
            {
                CandidateProfileId = profile.Id,
                JobId = targetJob?.Id,
                TargetJob = targetJob,
                SessionTitle = targetJob != null ? $"{request.InterviewType} Interview: {targetJob.Title}" : $"{request.InterviewType} General Interview",
                InterviewType = request.InterviewType,
                Difficulty = request.Difficulty,
                Status = InterviewStatus.InProgress,
                StartedAt = DateTime.UtcNow,
                Questions = GeneratePersonalizedQuestions(profile, targetJob, request.InterviewType, request.Difficulty, request.NumberOfQuestions)
            };

            _context.InterviewSessions.Add(session);
            await _context.SaveChangesAsync();

            return MapToDto(session);
        }

        private ICollection<InterviewQuestion> GeneratePersonalizedQuestions(CandidateProfile profile, Job job, InterviewType type, InterviewDifficulty difficulty, int count)
        {
            var questions = new List<InterviewQuestion>();
            int order = 1;

            if (type == InterviewType.Technical || type == InterviewType.Mixed)
            {
                // Generate based on JobSkills and CandidateSkills
                int techCount = type == InterviewType.Mixed ? (int)Math.Ceiling(count * 0.4) : count;
                var skillsToAssess = new List<string>();

                if (job != null && job.JobSkills.Any())
                {
                    skillsToAssess = job.JobSkills.Select(js => js.Skill.Name).Take(techCount).ToList();
                }
                else if (profile.CandidateSkills.Any())
                {
                    skillsToAssess = profile.CandidateSkills.Select(cs => cs.Skill.Name).Take(techCount).ToList();
                }
                else
                {
                    skillsToAssess.Add("General Programming");
                    skillsToAssess.Add("Software Architecture");
                }

                foreach (var skill in skillsToAssess.Take(techCount))
                {
                    questions.Add(new InterviewQuestion
                    {
                        QuestionText = $"Can you explain a complex problem you solved using {skill}?",
                        QuestionType = "Technical",
                        Category = "Technical Skills",
                        Difficulty = difficulty,
                        ExpectedTopics = skill,
                        QuestionOrder = order++
                    });
                }
            }

            if (type == InterviewType.Behavioral || type == InterviewType.Mixed)
            {
                int behavCount = type == InterviewType.Mixed ? (int)Math.Max(2, count * 0.4) : count;
                var pastTitles = profile.WorkExperiences.Select(w => w.JobTitle).FirstOrDefault() ?? "your previous role";

                for (int i = 0; i < behavCount; i++)
                {
                    questions.Add(new InterviewQuestion
                    {
                        QuestionText = i % 2 == 0 ? $"Tell me about a time you had a conflict at work while working as {pastTitles}." : "Describe a situation where you had to adapt to a significant change.",
                        QuestionType = "Behavioral",
                        Category = "Behavioral",
                        Difficulty = difficulty,
                        ExpectedTopics = "conflict resolution, adaptation, communication",
                        QuestionOrder = order++
                    });
                }
            }

            if (type == InterviewType.Mixed)
            {
                while (questions.Count < count)
                {
                    questions.Add(new InterviewQuestion
                    {
                        QuestionText = job != null ? $"Why do you think you are a good fit for the {job.Title} role?" : "Where do you see your career going in the next 5 years?",
                        QuestionType = "Role Specific",
                        Category = "Role Specific",
                        Difficulty = difficulty,
                        ExpectedTopics = "motivation, alignment, goals",
                        QuestionOrder = order++
                    });
                }
            }

            // Ensure we don't exceed requested count
            return questions.Take(count).ToList();
        }

        public async Task<IEnumerable<InterviewSessionDto>> GetCandidateInterviewsAsync(Guid candidateProfileId)
        {
            var sessions = await _context.InterviewSessions
                .Include(s => s.Questions)
                .Where(s => s.CandidateProfileId == candidateProfileId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return sessions.Select(MapToDto);
        }

        public async Task<InterviewSessionDto> GetInterviewSessionAsync(Guid candidateProfileId, Guid sessionId)
        {
            var session = await _context.InterviewSessions
                .Include(s => s.Questions).ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.CandidateProfileId == candidateProfileId);

            if (session == null) throw new UnauthorizedAccessException("Session not found or inaccessible.");

            return MapToDto(session);
        }

        public async Task<InterviewFeedbackDto> SubmitAnswerAsync(Guid candidateProfileId, Guid sessionId, Guid questionId, SubmitInterviewAnswerRequestDto request)
        {
            var session = await _context.InterviewSessions
                .Include(s => s.Questions).ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.CandidateProfileId == candidateProfileId);

            if (session == null) throw new UnauthorizedAccessException("Session not found or inaccessible.");
            if (session.Status == InterviewStatus.Completed) throw new InvalidOperationException("Interview is already completed.");

            if (string.IsNullOrWhiteSpace(request.AnswerText)) throw new ArgumentException("Answer cannot be empty.");

            var question = session.Questions.FirstOrDefault(q => q.Id == questionId);
            if (question == null) throw new KeyNotFoundException("Question not found in this session.");

            // Basic deterministic evaluation
            var answerWords = request.AnswerText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int wordCount = answerWords.Length;

            double completeness = Math.Min(100, Math.Max(0, (wordCount / 50.0) * 100)); // 50 words is 100% complete

            double relevance = 50;
            double technicalAccuracy = 50;
            if (!string.IsNullOrEmpty(question.ExpectedTopics))
            {
                var topics = question.ExpectedTopics.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim().ToLower());
                int matchedTopics = topics.Count(t => request.AnswerText.ToLower().Contains(t));
                relevance = topics.Any() ? Math.Min(100, (matchedTopics / (double)topics.Count()) * 100) : 75;
                technicalAccuracy = relevance * 0.9;
            }

            double communication = Math.Min(100, Math.Max(50, 100 - (Math.Abs(100 - wordCount) * 0.2)));
            double roleAlignment = 75;

            // Weighted score
            double totalScore = (relevance * 0.25) + (technicalAccuracy * 0.30) + (completeness * 0.20) + (communication * 0.15) + (roleAlignment * 0.10);

            var answer = new InterviewAnswer
            {
                InterviewQuestionId = questionId,
                AnswerText = request.AnswerText,
                AnsweredAt = DateTime.UtcNow,
                CompletenessScore = completeness,
                RelevanceScore = relevance,
                TechnicalAccuracyScore = technicalAccuracy,
                CommunicationScore = communication,
                RoleAlignmentScore = roleAlignment,
                Score = totalScore,
                Feedback = $"Your answer was evaluated deterministically. Score: {totalScore:F1}.",
                Strengths = relevance > 70 ? "Good coverage of expected topics." : "Clear communication.",
                Weaknesses = completeness < 50 ? "Answer is too brief." : "Lacks specific technical depth.",
                ImprovementSuggestion = "Try to use the STAR method (Situation, Task, Action, Result) to structure your answers better."
            };

            _context.InterviewAnswers.Add(answer);
            await _context.SaveChangesAsync();

            return new InterviewFeedbackDto
            {
                Score = answer.Score ?? 0,
                Strengths = answer.Strengths ?? "",
                Weaknesses = answer.Weaknesses ?? "",
                WhatWasMissing = "Specific numerical outcomes.",
                WhatToImprove = answer.ImprovementSuggestion ?? "",
                TopicsToRevise = question.ExpectedTopics ?? "",
                ExampleImprovementDirection = "Focus on providing clear examples to back up your claims."
            };
        }

        public async Task<InterviewReadinessDto> GetInterviewReadinessAsync(Guid candidateProfileId, Guid sessionId)
        {
            var session = await _context.InterviewSessions
                .Include(s => s.Questions).ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.CandidateProfileId == candidateProfileId);

            if (session == null) throw new UnauthorizedAccessException("Session not found or inaccessible.");

            return ComputeReadinessDto(session);
        }

        public async Task<InterviewReadinessDto> CompleteInterviewAsync(Guid candidateProfileId, Guid sessionId)
        {
            var session = await _context.InterviewSessions
                .Include(s => s.Questions).ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.CandidateProfileId == candidateProfileId);

            if (session == null) throw new UnauthorizedAccessException("Session not found or inaccessible.");

            if (session.Status != InterviewStatus.Completed)
            {
                session.Status = InterviewStatus.Completed;
                session.CompletedAt = DateTime.UtcNow;

                var readiness = ComputeReadinessDto(session);
                session.OverallReadinessScore = readiness.OverallReadinessScore;
                session.TechnicalScore = readiness.TechnicalScore;
                session.CommunicationScore = readiness.CommunicationScore;
                session.RoleAlignmentScore = readiness.RoleAlignmentScore;
                session.CompletenessScore = readiness.CompletenessScore;

                session.Strengths = string.Join(", ", readiness.Strengths);
                session.Weaknesses = string.Join(", ", readiness.Weaknesses);
                session.Recommendations = string.Join(", ", readiness.Recommendations);

                await _context.SaveChangesAsync();
            }

            return ComputeReadinessDto(session);
        }

        private InterviewReadinessDto ComputeReadinessDto(InterviewSession session)
        {
            var allAnswers = session.Questions.SelectMany(q => q.Answers).ToList();
            if (!allAnswers.Any())
            {
                return new InterviewReadinessDto { InterviewSessionId = session.Id };
            }

            double overall = allAnswers.Average(a => a.Score ?? 0);
            double tech = allAnswers.Average(a => a.TechnicalAccuracyScore ?? 0);
            double comm = allAnswers.Average(a => a.CommunicationScore ?? 0);
            double role = allAnswers.Average(a => a.RoleAlignmentScore ?? 0);
            double comp = allAnswers.Average(a => a.CompletenessScore ?? 0);

            var strengths = new List<string>();
            var weaknesses = new List<string>();

            if (tech > 75) strengths.Add("Strong technical accuracy");
            else weaknesses.Add("Needs stronger technical foundations");

            if (comm > 75) strengths.Add("Clear and effective communication");
            else weaknesses.Add("Answers could be more articulated");

            return new InterviewReadinessDto
            {
                InterviewSessionId = session.Id,
                OverallReadinessScore = Math.Round(overall, 1),
                TechnicalScore = Math.Round(tech, 1),
                CommunicationScore = Math.Round(comm, 1),
                RoleAlignmentScore = Math.Round(role, 1),
                CompletenessScore = Math.Round(comp, 1),
                Strengths = strengths.Any() ? strengths : new List<string> { "Consistent effort" },
                Weaknesses = weaknesses.Any() ? weaknesses : new List<string> { "Expand answers with more context" },
                Recommendations = new List<string> { "Practice STAR method", "Review missed technical topics" }
            };
        }

        private static InterviewSessionDto MapToDto(InterviewSession session)
        {
            return new InterviewSessionDto
            {
                Id = session.Id,
                CandidateProfileId = session.CandidateProfileId,
                JobId = session.JobId,
                SessionTitle = session.SessionTitle,
                InterviewType = session.InterviewType.ToString(),
                Difficulty = session.Difficulty.ToString(),
                OverallReadinessScore = session.OverallReadinessScore,
                Status = session.Status.ToString(),
                StartedAt = session.StartedAt,
                CompletedAt = session.CompletedAt,
                CreatedAt = session.CreatedAt,
                Questions = session.Questions.Select(q => new InterviewQuestionDto
                {
                    Id = q.Id,
                    InterviewSessionId = q.InterviewSessionId,
                    QuestionText = q.QuestionText,
                    QuestionType = q.QuestionType,
                    Category = q.Category,
                    Difficulty = q.Difficulty.ToString(),
                    QuestionOrder = q.QuestionOrder,
                    Answers = q.Answers.Select(a => new InterviewAnswerDto
                    {
                        Id = a.Id,
                        InterviewQuestionId = a.InterviewQuestionId,
                        AnswerText = a.AnswerText,
                        Score = a.Score,
                        Feedback = a.Feedback,
                        Strengths = a.Strengths,
                        Weaknesses = a.Weaknesses,
                        ImprovementSuggestion = a.ImprovementSuggestion,
                        AnsweredAt = a.AnsweredAt
                    }).ToList()
                }).OrderBy(q => q.QuestionOrder).ToList()
            };
        }
    }
}
