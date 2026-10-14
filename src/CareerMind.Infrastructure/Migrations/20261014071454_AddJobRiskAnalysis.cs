using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareerMind.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJobRiskAnalysis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobRiskAnalyses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CandidateProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JobDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SalaryDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApplicationInstructions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmployerInformation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RiskScore = table.Column<int>(type: "int", nullable: false),
                    RiskCategory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TriggeredIndicatorsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WarningSummaryJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Explanation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VerificationStepsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConfidenceNote = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobRiskAnalyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobRiskAnalyses_CandidateProfiles_CandidateProfileId",
                        column: x => x.CandidateProfileId,
                        principalTable: "CandidateProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobRiskAnalyses_CandidateProfileId",
                table: "JobRiskAnalyses",
                column: "CandidateProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobRiskAnalyses");
        }
    }
}
