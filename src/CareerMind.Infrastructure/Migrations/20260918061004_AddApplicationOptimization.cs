using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareerMind.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationOptimization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationOptimizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CandidateProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResumeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApplicationReadinessScore = table.Column<double>(type: "float", nullable: true),
                    ProfileAlignmentScore = table.Column<double>(type: "float", nullable: true),
                    ResumeAlignmentScore = table.Column<double>(type: "float", nullable: true),
                    SkillAlignmentScore = table.Column<double>(type: "float", nullable: true),
                    ExperienceAlignmentScore = table.Column<double>(type: "float", nullable: true),
                    EducationAlignmentScore = table.Column<double>(type: "float", nullable: true),
                    InterviewReadinessScore = table.Column<double>(type: "float", nullable: true),
                    ApplicationStatus = table.Column<int>(type: "int", nullable: false),
                    IsRecommendedToApply = table.Column<bool>(type: "bit", nullable: false),
                    OptimizationVersion = table.Column<int>(type: "int", nullable: false),
                    OptimizedHeadline = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OptimizedSummary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CoverLetter = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationOptimizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationOptimizations_CandidateProfiles_CandidateProfileId",
                        column: x => x.CandidateProfileId,
                        principalTable: "CandidateProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationOptimizations_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApplicationOptimizations_Resumes_ResumeId",
                        column: x => x.ResumeId,
                        principalTable: "Resumes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ApplicationOptimizationSuggestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationOptimizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SuggestedAction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationOptimizationSuggestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationOptimizationSuggestions_ApplicationOptimizations_ApplicationOptimizationId",
                        column: x => x.ApplicationOptimizationId,
                        principalTable: "ApplicationOptimizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationOptimizations_CandidateProfileId_JobId",
                table: "ApplicationOptimizations",
                columns: new[] { "CandidateProfileId", "JobId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationOptimizations_JobId",
                table: "ApplicationOptimizations",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationOptimizations_ResumeId",
                table: "ApplicationOptimizations",
                column: "ResumeId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationOptimizationSuggestions_ApplicationOptimizationId",
                table: "ApplicationOptimizationSuggestions",
                column: "ApplicationOptimizationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationOptimizationSuggestions");

            migrationBuilder.DropTable(
                name: "ApplicationOptimizations");
        }
    }
}
