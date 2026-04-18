using DiaryApp.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PersonModel = DiaryApp.Shared.Models.Person;

namespace DiaryApp.Core.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<DiaryEntry> DiaryEntries { get; set; } = null!;
        public DbSet<PersonModel> Peoples { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed data para DiaryEntries
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

            // Configurar relación entre Payment y Person
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Person)
                .WithMany(pe => pe.Payments)
                .HasForeignKey(p => p.PeoplesId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ CORREGIDO: Índice NO único (permite múltiples pagos por persona/mes)
            // Este índice mejora el rendimiento de búsquedas pero NO impide duplicados
            modelBuilder.Entity<Payment>()
                .HasIndex(p => new { p.PeoplesId, p.Ano, p.Mes })
                .HasDatabaseName("IX_Payments_PeoplesId_Ano_Mes");

            // ✅ Relación OPCIONAL entre Person y IdentityUser
            modelBuilder.Entity<PersonModel>()
                .HasOne<IdentityUser>()
                .WithOne()
                .HasForeignKey<PersonModel>(p => p.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // ✅ Índice único (pero permitiendo null)
            modelBuilder.Entity<PersonModel>()
                .HasIndex(p => p.UserId)
                .IsUnique()
                .HasFilter("[UserId] IS NOT NULL");
        }
    }
}