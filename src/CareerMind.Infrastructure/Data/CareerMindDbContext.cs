using CareerMind.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CareerMind.Infrastructure.Data
{
    public class CareerMindDbContext : DbContext
    {
        public CareerMindDbContext(DbContextOptions<CareerMindDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<CandidateProfile> CandidateProfiles { get; set; } = null!;
        public DbSet<Skill> Skills { get; set; } = null!;
        public DbSet<CandidateSkill> CandidateSkills { get; set; } = null!;
        public DbSet<Company> Companies { get; set; } = null!;
        public DbSet<Job> Jobs { get; set; } = null!;
        public DbSet<JobSkill> JobSkills { get; set; } = null!;
        public DbSet<JobApplication> JobApplications { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Apply configurations from current assembly
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // M-to-M for CandidateSkill
            modelBuilder.Entity<CandidateSkill>()
                .HasKey(cs => new { cs.CandidateProfileId, cs.SkillId });
                
            modelBuilder.Entity<CandidateSkill>()
                .HasOne(cs => cs.CandidateProfile)
                .WithMany(cp => cp.CandidateSkills)
                .HasForeignKey(cs => cs.CandidateProfileId);
                
            modelBuilder.Entity<CandidateSkill>()
                .HasOne(cs => cs.Skill)
                .WithMany(s => s.CandidateSkills)
                .HasForeignKey(cs => cs.SkillId);

            // M-to-M for JobSkill
            modelBuilder.Entity<JobSkill>()
                .HasKey(js => new { js.JobId, js.SkillId });
                
            modelBuilder.Entity<JobSkill>()
                .HasOne(js => js.Job)
                .WithMany(j => j.JobSkills)
                .HasForeignKey(js => js.JobId);
                
            modelBuilder.Entity<JobSkill>()
                .HasOne(js => js.Skill)
                .WithMany(s => s.JobSkills)
                .HasForeignKey(js => js.SkillId);

            // 1-to-1 User <-> CandidateProfile
            modelBuilder.Entity<User>()
                .HasOne(u => u.CandidateProfile)
                .WithOne(c => c.User)
                .HasForeignKey<CandidateProfile>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 1-to-1 User <-> Company
            modelBuilder.Entity<User>()
                .HasOne(u => u.Company)
                .WithOne(c => c.User)
                .HasForeignKey<Company>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // JobApplication to Job
            modelBuilder.Entity<JobApplication>()
                .HasOne(ja => ja.Job)
                .WithMany(j => j.Applications)
                .HasForeignKey(ja => ja.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            // JobApplication to CandidateProfile
            modelBuilder.Entity<JobApplication>()
                .HasOne(ja => ja.CandidateProfile)
                .WithMany(cp => cp.JobApplications)
                .HasForeignKey(ja => ja.CandidateProfileId)
                .OnDelete(DeleteBehavior.NoAction);
                
            // Setup User/Role constraint
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // User to RefreshTokens
            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
