using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Code_Quizzer.Models;

namespace Code_Quizzer.Data
{
    // Inheriting from IdentityDbContext<ApplicationUser> configures the standard ASP.NET Identity tables
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Database Tables
        public DbSet<QuestionEntity> Questions { get; set; }
        public DbSet<QuestionChoice> QuestionChoices { get; set; }
        public DbSet<UserScoreEntity> UserScores { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. Configures standard identity tables correctly
            base.OnModelCreating(modelBuilder);

            // 2. Explicitly bind the UserScoreEntity foreign key relationship to your ApplicationUser table
            modelBuilder.Entity<UserScoreEntity>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 3. Establish the cascading relationship from Question Choices to parent Question Entities
            modelBuilder.Entity<QuestionChoice>()
                .HasOne<QuestionEntity>()
                .WithMany(q => q.Choices) // Maps to the List<QuestionChoice> Choices navigation property
                .HasForeignKey(p => p.QuestionEntityId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}