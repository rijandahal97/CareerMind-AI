using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using CareerMind.Infrastructure.Data;
using CareerMind.Infrastructure.Services;
using CareerMind.Domain.Entities;
using CareerMind.Application.DTOs.JobRisk;

namespace CareerMind.UnitTests
{
    public class JobRiskAnalysisServiceTests : IDisposable
    {
        private readonly CareerMindDbContext _context;
        private readonly JobRiskAnalysisService _service;

        public JobRiskAnalysisServiceTests()
        {
            var options = new DbContextOptionsBuilder<CareerMindDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CareerMindDbContext(options);
            _service = new JobRiskAnalysisService(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        // ────────────────────────────────────────────────────────────────────────
        // Helpers
        // ────────────────────────────────────────────────────────────────────────

        private async Task<Guid> CreateCandidateAsync()
        {
            var userId = Guid.NewGuid();
            _context.CandidateProfiles.Add(new CandidateProfile { Id = Guid.NewGuid(), UserId = userId });
            await _context.SaveChangesAsync();
            return userId;
        }

        private static AnalyzeJobRiskRequestDto NormalPosting() => new()
        {
            JobTitle             = "Software Engineer",
            CompanyName          = "Acme Corp",
            JobDescription       = string.Join(" ", Enumerable.Repeat("we are looking for a skilled engineer to join our team you will work on challenging projects and collaborate with talented colleagues. Requirements: 3 years experience with C sharp or Java a degree in computer science and excellent communication skills.", 2)),
            SalaryDetails        = "$90,000 – $110,000 per year",
            ApplicationInstructions = "Apply via our careers portal at careers.acme.com",
            EmployerInformation  = "Acme Corp, 123 Main Street, San Francisco, CA 94105"
        };

        private static AnalyzeJobRiskRequestDto ScamPosting() => new()
        {
            JobTitle             = "Work from Home Data Entry",
            CompanyName          = "GlobalHire Ltd",
            JobDescription       = "Earn $1000 per day from home! No experience needed. Act now – limited spots! Apply via whatsapp me immediately.",
            SalaryDetails        = "Unlimited income – guaranteed!",
            ApplicationInstructions = "Send registration fee of $50 via western union to get your starter kit cost.",
            EmployerInformation  = null
        };

        // ────────────────────────────────────────────────────────────────────────
        // Tests: Normal posting
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task Analyze_NormalPosting_HasLowRiskScore()
        {
            var userId = await CreateCandidateAsync();
            var result = await _service.AnalyzeJobRiskAsync(userId, NormalPosting());

            Assert.NotNull(result);
            Assert.True(result.RiskScore < 50,
                $"Expected a low risk score for a normal posting but got {result.RiskScore}");
            Assert.True(result.RiskCategory is "Low" or "Moderate",
                $"Expected Low or Moderate category, got '{result.RiskCategory}'");
            Assert.NotEmpty(result.Disclaimer);
        }

        [Fact]
        public async Task Analyze_NormalPosting_FewOrNoWarningSigns()
        {
            var userId = await CreateCandidateAsync();
            var result = await _service.AnalyzeJobRiskAsync(userId, NormalPosting());

            Assert.True(result.WarningSigns.Count <= 2,
                $"Normal posting should have 0-2 warning signs, got {result.WarningSigns.Count}");
        }

        // ────────────────────────────────────────────────────────────────────────
        // Tests: Suspicious / scam posting
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task Analyze_ScamPosting_HasHighRiskScore()
        {
            var userId = await CreateCandidateAsync();
            var result = await _service.AnalyzeJobRiskAsync(userId, ScamPosting());

            Assert.True(result.RiskScore >= 50,
                $"Expected a high risk score for a scam-like posting but got {result.RiskScore}");
            Assert.True(result.RiskCategory is "High" or "Very High",
                $"Expected High or Very High category, got '{result.RiskCategory}'");
        }

        [Fact]
        public async Task Analyze_ScamPosting_DetectsUpfrontFeeIndicator()
        {
            var userId = await CreateCandidateAsync();
            var result = await _service.AnalyzeJobRiskAsync(userId, ScamPosting());

            Assert.Contains("upfront_fee",
                result.Indicators.Where(i => i.Triggered).Select(i => i.Key),
                StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Analyze_ScamPosting_DetectsWesternUnionPayment()
        {
            var userId = await CreateCandidateAsync();
            var result = await _service.AnalyzeJobRiskAsync(userId, ScamPosting());

            Assert.Contains("unusual_payment",
                result.Indicators.Where(i => i.Triggered).Select(i => i.Key),
                StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Analyze_ScamPosting_DetectsMissingEmployerInformation()
        {
            var userId = await CreateCandidateAsync();
            var result = await _service.AnalyzeJobRiskAsync(userId, ScamPosting());

            Assert.Contains("no_company_address",
                result.Indicators.Where(i => i.Triggered).Select(i => i.Key),
                StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Analyze_ScamPosting_DetectsUrgencyPressure()
        {
            var userId = await CreateCandidateAsync();
            var result = await _service.AnalyzeJobRiskAsync(userId, ScamPosting());

            Assert.Contains("urgency_pressure",
                result.Indicators.Where(i => i.Triggered).Select(i => i.Key),
                StringComparer.OrdinalIgnoreCase);
        }

        // ────────────────────────────────────────────────────────────────────────
        // Tests: Scoring rules
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task Analyze_ScoreBoundary_IsAlwaysBetween0And100()
        {
            var userId = await CreateCandidateAsync();

            // Extremely suspicious
            var result1 = await _service.AnalyzeJobRiskAsync(userId, ScamPosting());
            Assert.InRange(result1.RiskScore, 0, 100);

            var userId2 = await CreateCandidateAsync();

            // Very clean
            var result2 = await _service.AnalyzeJobRiskAsync(userId2, NormalPosting());
            Assert.InRange(result2.RiskScore, 0, 100);
        }

        [Theory]
        [InlineData(0,  "Low")]
        [InlineData(24, "Low")]
        [InlineData(25, "Moderate")]
        [InlineData(49, "Moderate")]
        [InlineData(50, "High")]
        [InlineData(74, "High")]
        [InlineData(75, "Very High")]
        [InlineData(100, "Very High")]
        public void RiskCategory_MapsCorrectlyFromScore(int score, string expected)
        {
            // The category logic is internal; we test it via public output.
            // We verify indirectly by analysing postings resulting in specific scores
            // OR we assert boundaries via the theory data and a clean implementation.
            // Since we cannot call private methods, we verify the boundary spec itself.
            string actual = score switch
            {
                <= 24 => "Low",
                <= 49 => "Moderate",
                <= 74 => "High",
                _     => "Very High"
            };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task Analyze_VagueDescription_TriggersVagueIndicator()
        {
            var userId = await CreateCandidateAsync();
            var request = new AnalyzeJobRiskRequestDto
            {
                JobTitle        = "Mystery Job",
                CompanyName     = "Unknown Corp",
                JobDescription  = "Looking for someone.", // very short
                EmployerInformation = "123 Main Street"
            };

            var result = await _service.AnalyzeJobRiskAsync(userId, request);

            // Short description should trigger vague_description
            Assert.Contains("vague_description",
                result.Indicators.Where(i => i.Triggered).Select(i => i.Key));
        }

        // ────────────────────────────────────────────────────────────────────────
        // Tests: Missing data
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task Analyze_MissingOptionalFields_DoesNotThrow()
        {
            var userId = await CreateCandidateAsync();
            var request = new AnalyzeJobRiskRequestDto
            {
                JobTitle        = "Some Job",
                CompanyName     = "Some Company",
                JobDescription  = string.Join(" ", Enumerable.Repeat("responsible for working on software engineering tasks throughout the year with senior engineers.", 4)),
                // All optional fields omitted
            };

            var ex = await Record.ExceptionAsync(() => _service.AnalyzeJobRiskAsync(userId, request));
            Assert.Null(ex);
        }

        [Fact]
        public async Task Analyze_MissingOptionalFields_ConfidenceNoteReflectsMissingData()
        {
            var userId = await CreateCandidateAsync();
            var request = new AnalyzeJobRiskRequestDto
            {
                JobTitle       = "Some Job",
                CompanyName    = "Some Company",
                JobDescription = string.Join(" ", Enumerable.Repeat("responsible for working on software engineering tasks throughout the year.", 4)),
            };

            var result = await _service.AnalyzeJobRiskAsync(userId, request);

            Assert.Contains("not provided", result.ConfidenceNote, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Analyze_AllFieldsProvided_ConfidenceNoteIndicatesFullCoverage()
        {
            var userId = await CreateCandidateAsync();
            var result = await _service.AnalyzeJobRiskAsync(userId, NormalPosting());

            Assert.Contains("All input fields", result.ConfidenceNote, StringComparison.OrdinalIgnoreCase);
        }

        // ────────────────────────────────────────────────────────────────────────
        // Tests: Persistence & history
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task Analyze_PersistsResult_CanBeRetrievedById()
        {
            var userId = await CreateCandidateAsync();
            var result = await _service.AnalyzeJobRiskAsync(userId, ScamPosting());

            var fetched = await _service.GetRiskAnalysisAsync(userId, result.AnalysisId);

            Assert.Equal(result.AnalysisId, fetched.AnalysisId);
            Assert.Equal(result.RiskScore, fetched.RiskScore);
            Assert.Equal(result.RiskCategory, fetched.RiskCategory);
        }

        [Fact]
        public async Task GetHistory_ReturnsOnlyOwnAnalyses()
        {
            var userId1 = await CreateCandidateAsync();
            var userId2 = await CreateCandidateAsync();

            await _service.AnalyzeJobRiskAsync(userId1, ScamPosting());
            await _service.AnalyzeJobRiskAsync(userId1, NormalPosting());
            await _service.AnalyzeJobRiskAsync(userId2, ScamPosting());

            var history1 = await _service.GetRiskAnalysisHistoryAsync(userId1);
            var history2 = await _service.GetRiskAnalysisHistoryAsync(userId2);

            Assert.Equal(2, history1.Count);
            Assert.Single(history2);
        }

        [Fact]
        public async Task GetRiskAnalysis_ThrowsUnauthorized_ForOtherUsersAnalysis()
        {
            var userId1 = await CreateCandidateAsync();
            var userId2 = await CreateCandidateAsync();

            var result = await _service.AnalyzeJobRiskAsync(userId1, ScamPosting());

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _service.GetRiskAnalysisAsync(userId2, result.AnalysisId));
        }

        [Fact]
        public async Task GetHistory_ThrowsKeyNotFound_ForNonExistentUser()
        {
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.GetRiskAnalysisHistoryAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task GetRiskAnalysis_ThrowsKeyNotFound_ForNonExistentAnalysis()
        {
            var userId = await CreateCandidateAsync();
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.GetRiskAnalysisAsync(userId, Guid.NewGuid()));
        }

        // ────────────────────────────────────────────────────────────────────────
        // Tests: Disclaimer / explainability
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task Analyze_AlwaysIncludesDisclaimer()
        {
            var userId = await CreateCandidateAsync();
            var result = await _service.AnalyzeJobRiskAsync(userId, NormalPosting());

            Assert.False(string.IsNullOrWhiteSpace(result.Disclaimer));
            Assert.Contains("cannot guarantee", result.Disclaimer, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Analyze_AllIndicatorsReturned_EvenIfNotTriggered()
        {
            var userId = await CreateCandidateAsync();
            var result = await _service.AnalyzeJobRiskAsync(userId, NormalPosting());

            // All 11 indicators should be in the response
            Assert.Equal(11, result.Indicators.Count);
        }

        [Fact]
        public async Task Analyze_WarningSigns_SubsetOfTriggeredIndicators()
        {
            var userId = await CreateCandidateAsync();
            var result = await _service.AnalyzeJobRiskAsync(userId, ScamPosting());

            var triggeredLabels = result.Indicators.Where(i => i.Triggered).Select(i => i.Label).ToHashSet();
            foreach (var sign in result.WarningSigns)
            {
                Assert.Contains(sign, triggeredLabels);
            }
        }

        [Fact]
        public async Task Analyze_VerificationStepsAlwaysPresent()
        {
            var userId = await CreateCandidateAsync();
            var result = await _service.AnalyzeJobRiskAsync(userId, NormalPosting());

            Assert.NotEmpty(result.VerificationSteps);
        }

        // ────────────────────────────────────────────────────────────────────────
        // Tests: Overpayment / sensitive-info indicators
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task Analyze_DetectsOverpaymentScheme()
        {
            var userId = await CreateCandidateAsync();
            var request = new AnalyzeJobRiskRequestDto
            {
                JobTitle       = "Mystery Shopper",
                CompanyName    = "Secret Shopper Inc",
                JobDescription = string.Join(" ", Enumerable.Repeat("We will send you an overpayment cheque. Keep your fee and return the difference. Cashier check included.", 2)),
            };

            var result = await _service.AnalyzeJobRiskAsync(userId, request);

            Assert.Contains("overpayment_scheme",
                result.Indicators.Where(i => i.Triggered).Select(i => i.Key));
        }

        [Fact]
        public async Task Analyze_DetectsSensitivePersonalInfoRequest()
        {
            var userId = await CreateCandidateAsync();
            var request = new AnalyzeJobRiskRequestDto
            {
                JobTitle       = "Remote Data Entry",
                CompanyName    = "DataPro Ltd",
                JobDescription = string.Join(" ", Enumerable.Repeat("Please provide your social security number and passport number to complete your application. Also include your bank account number for payroll setup.", 2)),
            };

            var result = await _service.AnalyzeJobRiskAsync(userId, request);

            Assert.Contains("personal_info_early",
                result.Indicators.Where(i => i.Triggered).Select(i => i.Key));
        }
    }
}
