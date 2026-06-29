using Microsoft.EntityFrameworkCore;

namespace AustimAPI.Models
{
    public class apiDBContext : DbContext
    {
        public apiDBContext(DbContextOptions options) : base(options) { }

        public DbSet<User> User { get; set; }
        public DbSet<Child> Child { get; set; }
        public DbSet<Screening> Screening { get; set; }
        public DbSet<Question> Question { get; set; }
        public DbSet<QuestionnaireAnswer> QuestionnaireAnswer { get; set; }
        public DbSet<AIResult> AIResult { get; set; }
        public DbSet<PasswordReset> PasswordReset { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Child>()
                .HasOne(c => c.User).WithMany(u => u.Children)
                .HasForeignKey(c => c.ParentID).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PasswordReset>()
                .HasOne(p => p.User).WithMany()
                .HasForeignKey(p => p.UserID).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuestionnaireAnswer>()
                .HasOne(a => a.Screening).WithMany(s => s.Answers)
                .HasForeignKey(a => a.ScreeningID).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuestionnaireAnswer>()
                .HasOne(a => a.Question).WithMany(q => q.Answers)
                .HasForeignKey(a => a.QuestionID).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AIResult>()
                .HasOne(r => r.Screening).WithOne(s => s.AIResult)
                .HasForeignKey<AIResult>(r => r.ScreeningID).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Screening>()
                .HasOne(s => s.Child).WithMany(c => c.Screenings)
                .HasForeignKey(s => s.ChildID).OnDelete(DeleteBehavior.Cascade);
        }
    }
}