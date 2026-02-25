using DiaryApp.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DiaryApp.Core.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<DiaryEntry> DiaryEntries { get; set; }
        public DbSet<Person> Peoples { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DiaryEntry>().HasData(
                new DiaryEntry
                {
                    Id = 1,
                    Title = "Learning .Net MVC",
                    Content = "learning .net mvc with Punjha",
                    DateCreated = new DateTime(2025, 1, 1)
                },
                new DiaryEntry
                {
                    Id = 2,
                    Title = "Learning Migrations",
                    Content = "Learning Migrations mvc with Punjha",
                    DateCreated = new DateTime(2025, 1, 2)
                },
                new DiaryEntry
                {
                    Id = 3,
                    Title = "Input database",
                    Content = "Learning Input database with Punjha",
                    DateCreated = new DateTime(2025, 1, 3)
                }
            );

            modelBuilder.Entity<Person>().HasData(
                new Person
                {
                    Id = 1,
                    Nombre = "Pablo Eugenio Cominiello",
                    Content = "Kili",
                    Born = new DateTime(1976, 6, 30)
                }
            );

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Person)
                .WithMany(pe => pe.Payments)
                .HasForeignKey(p => p.PeoplesId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Payment>()
                .HasIndex(p => new { p.PeoplesId, p.Ano, p.Mes })
                .IsUnique()
                .HasDatabaseName("IX_Payments_PeoplesId_Ano_Mes");
        }
    }
}