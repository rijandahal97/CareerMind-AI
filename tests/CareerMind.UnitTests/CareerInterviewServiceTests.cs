using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using CareerMind.Application.DTOs.Interview;
using CareerMind.Domain.Entities;
using CareerMind.Domain.Enums;
using CareerMind.Infrastructure.Data;
using CareerMind.Infrastructure.Services;

namespace CareerMind.UnitTests
{
    public class CareerInterviewServiceTests : IDisposable
    {
        private readonly CareerMindDbContext _context;
        private readonly CareerInterviewService _service;
        private readonly Mock<ILogger<CareerInterviewService>> _mockLogger;

        public CareerInterviewServiceTests()
        {
            var options = new DbContextOptionsBuilder<CareerMindDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CareerMindDbContext(options);
            _mockLogger = new Mock<ILogger<CareerInterviewService>>();
            _service = new CareerInterviewService(_context, _mockLogger.Object);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private async Task<Guid> SetupCandidateWithSkills()
        {
            var user = new User { Id = Guid.NewGuid(), Email = "test@test.com", FirstName = "Test", LastName = "User" };
            var profile = new CandidateProfile { Id = Guid.NewGuid(), UserId = user.Id, Headline = "Developer" };

            var s1 = new Skill { Id = Guid.NewGuid(), Name = "C#" };
            var s2 = new Skill { Id = Guid.NewGuid(), Name = "SQL" };

            _context.Users.Add(user);
            _context.Skills.AddRange(s1, s2);
            await _context.SaveChangesAsync();

            profile.CandidateSkills = new List<CandidateSkill>
            {
                new CandidateSkill { SkillId = s1.Id, ProficiencyLevel = (ProficiencyLevel)4 },
                new CandidateSkill { SkillId = s2.Id, ProficiencyLevel = (ProficiencyLevel)3 }
            };

            _context.CandidateProfiles.Add(profile);
            await _context.SaveChangesAsync();
            return profile.Id;
        }

        private async Task<Guid> SetupJobWithSkills()
        {
            var ep = new EmployerProfile { Id = Guid.NewGuid(), CompanyName = "Tech Corp" };
            var cat = new JobCategory { Id = Guid.NewGuid(), Name = "Software" };
            var job = new Job { Id = Guid.NewGuid(), EmployerProfileId = ep.Id, JobCategoryId = cat.Id, Title = "Software Engineer" };

            var s1 = new Skill { Id = Guid.NewGuid(), Name = "ASP.NET Core" };
            _context.EmployerProfiles.Add(ep);
            _context.JobCategories.Add(cat);
            _context.Skills.Add(s1);
            await _context.SaveChangesAsync();

            job.JobSkills = new List<JobSkill> { new JobSkill { SkillId = s1.Id } };
            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();
            return job.Id;
        }

        [Fact]
        public async Task StartInterview_WithValidCandidateAndJob_CreatesPersonalizedSession()
        {
            var candidateId = await SetupCandidateWithSkills();
            var jobId = await SetupJobWithSkills();

            var req = new StartInterviewRequestDto
            {
                TargetJobId = jobId,
                InterviewType = InterviewType.Mixed,
                Difficulty = InterviewDifficulty.Advanced,
                NumberOfQuestions = 10
            };

            var res = await _service.StartInterviewAsync(candidateId, req);

            Assert.NotNull(res);
            Assert.Equal("Mixed Interview: Software Engineer", res.SessionTitle);
            Assert.True(res.Questions.Count > 0);
            Assert.Equal("Advanced", res.Difficulty);
        }

        [Fact]
        public async Task StartInterview_MissingCandidate_ThrowsExpectedException()
        {
            var req = new StartInterviewRequestDto();
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.StartInterviewAsync(Guid.NewGuid(), req));
        }

        [Fact]
        public async Task StartInterview_GeneratesQuestionsBasedOnJobSkills()
        {
            var candidateId = await SetupCandidateWithSkills();
            var jobId = await SetupJobWithSkills();

            var req = new StartInterviewRequestDto { TargetJobId = jobId, InterviewType = InterviewType.Technical, NumberOfQuestions = 1 };
            var res = await _service.StartInterviewAsync(candidateId, req);

            var firstQuestion = res.Questions.First();
            Assert.Contains("ASP.NET Core", firstQuestion.QuestionText);
            Assert.Equal("Technical", firstQuestion.QuestionType);
        }

        [Fact]
        public async Task StartInterview_MissingCandidateSkill_GeneratesRelevantAssessmentQuestion()
        {
            // Similar to above tests, if candidate is missing skill, job skills are prioritized.
            var candidateId = await SetupCandidateWithSkills();
            var jobId = await SetupJobWithSkills(); // Job has ASP.NET Core, candidate does not

            var req = new StartInterviewRequestDto { TargetJobId = jobId, InterviewType = InterviewType.Technical, NumberOfQuestions = 1 };
            var res = await _service.StartInterviewAsync(candidateId, req);

            // Job skill (ASP.NET Core) should be assessed because candidate doesn't explicitly have it (or even if they do)
            Assert.Contains("ASP.NET Core", res.Questions.First().QuestionText);
        }

        [Fact]
        public async Task SubmitAnswer_ValidAnswer_CalculatesEvaluation()
        {
            var candidateId = await SetupCandidateWithSkills();
            var req = new StartInterviewRequestDto { InterviewType = InterviewType.Behavioral, NumberOfQuestions = 1 };
            var session = await _service.StartInterviewAsync(candidateId, req);

            var qId = session.Questions.First().Id;
            var submitReq = new SubmitInterviewAnswerRequestDto { AnswerText = "I approached the problem by defining clear metrics and communicating with the team." };

            var feedback = await _service.SubmitAnswerAsync(candidateId, session.Id, qId, submitReq);

            Assert.NotNull(feedback);
            Assert.True(feedback.Score > 0);
        }

        [Fact]
        public async Task SubmitAnswer_EmptyAnswer_IsHandledCorrectly()
        {
            var candidateId = await SetupCandidateWithSkills();
            var req = new StartInterviewRequestDto { NumberOfQuestions = 1 };
            var session = await _service.StartInterviewAsync(candidateId, req);

            var qId = session.Questions.First().Id;
            var submitReq = new SubmitInterviewAnswerRequestDto { AnswerText = "   " };

            await Assert.ThrowsAsync<ArgumentException>(() => _service.SubmitAnswerAsync(candidateId, session.Id, qId, submitReq));
        }

        [Fact]
        public async Task SubmitAnswer_UnauthorizedSession_IsRejected()
        {
            var candidateId = await SetupCandidateWithSkills();
            var req = new StartInterviewRequestDto { NumberOfQuestions = 1 };
            var session = await _service.StartInterviewAsync(candidateId, req);

            var qId = session.Questions.First().Id;
            var submitReq = new SubmitInterviewAnswerRequestDto { AnswerText = "Test answer" };

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.SubmitAnswerAsync(Guid.NewGuid(), session.Id, qId, submitReq));
        }

        [Fact]
        public async Task CompleteInterview_CalculatesReadinessScore()
        {
            var candidateId = await SetupCandidateWithSkills();
            var req = new StartInterviewRequestDto { NumberOfQuestions = 1 };
            var session = await _service.StartInterviewAsync(candidateId, req);
            var qId = session.Questions.First().Id;

            await _service.SubmitAnswerAsync(candidateId, session.Id, qId, new SubmitInterviewAnswerRequestDto { AnswerText = "I solved it using my great communication skills." });
            var readiness = await _service.CompleteInterviewAsync(candidateId, session.Id);

            Assert.True(readiness.OverallReadinessScore > 0);

            var completedSession = await _context.InterviewSessions.FindAsync(session.Id);
            Assert.Equal(InterviewStatus.Completed, completedSession.Status);
        }

        [Fact]
        public async Task ReadinessScore_IsWithinZeroToHundred()
        {
            var candidateId = await SetupCandidateWithSkills();
            var req = new StartInterviewRequestDto { NumberOfQuestions = 2 };
            var session = await _service.StartInterviewAsync(candidateId, req);

            foreach(var q in session.Questions)
            {
                await _service.SubmitAnswerAsync(candidateId, session.Id, q.Id, new SubmitInterviewAnswerRequestDto { AnswerText = "A short incomplete answer." });
            }

            var readiness = await _service.CompleteInterviewAsync(candidateId, session.Id);

            Assert.True(readiness.OverallReadinessScore >= 0 && readiness.OverallReadinessScore <= 100);
            Assert.True(readiness.TechnicalScore >= 0 && readiness.TechnicalScore <= 100);
            Assert.True(readiness.CommunicationScore >= 0 && readiness.CommunicationScore <= 100);
        }

        [Fact]
        public async Task InterviewHistory_ReturnsOnlyCandidateSessions()
        {
            var candidate1 = await SetupCandidateWithSkills();
            var candidate2 = await SetupCandidateWithSkills();

            await _service.StartInterviewAsync(candidate1, new StartInterviewRequestDto());
            await _service.StartInterviewAsync(candidate2, new StartInterviewRequestDto());

            var c1Sessions = await _service.GetCandidateInterviewsAsync(candidate1);
            Assert.Single(c1Sessions);
        }

        [Fact]
        public async Task DuplicateQuestionGeneration_IsPrevented()
        {
            var candidateId = await SetupCandidateWithSkills();
            var jobId = await SetupJobWithSkills();
            var req = new StartInterviewRequestDto { TargetJobId = jobId, InterviewType = InterviewType.Technical, NumberOfQuestions = 5 }; // Exceeds available distinct skills

            var session = await _service.StartInterviewAsync(candidateId, req);

            // Should pad with general questions or cap out without duplicate identical texts
            var uniqueQuestions = session.Questions.Select(q => q.QuestionText).Distinct().Count();
            Assert.Equal(session.Questions.Count, uniqueQuestions);
        }

        [Fact]
        public async Task BehavioralQuestion_UsesAppropriateEvaluationLogic()
        {
            var candidateId = await SetupCandidateWithSkills();
            var req = new StartInterviewRequestDto { InterviewType = InterviewType.Behavioral, NumberOfQuestions = 1 };

            var session = await _service.StartInterviewAsync(candidateId, req);
            var qId = session.Questions.First().Id;
            var feedback = await _service.SubmitAnswerAsync(candidateId, session.Id, qId, new SubmitInterviewAnswerRequestDto { AnswerText = "Behavioral answers rely on good STAR models, conflict resolution, adaptation, communication in situations." });

            Assert.True(feedback.Score > 50);
        }
    }
}
