using DiaryApp.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DiaryApp.Core.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<DiaryEntry> DiaryEntries { get; set; } = null!;
        public DbSet<Person> Peoples { get; set; } = null!;
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

            // Índice único compuesto para evitar duplicados
            modelBuilder.Entity<Payment>()
                .HasIndex(p => new { p.PeoplesId, p.Ano, p.Mes })
                .IsUnique()
                .HasDatabaseName("IX_Payments_PeoplesId_Ano_Mes");

            // ✅ Relación OPCIONAL entre Person y IdentityUser
            modelBuilder.Entity<Person>()
                .HasOne<IdentityUser>()
                .WithOne()
                .HasForeignKey<Person>(p => p.UserId)
                .OnDelete(DeleteBehavior.SetNull)  // ✅ Cambio: permitir null al eliminar usuario
                .IsRequired(false);  // ✅ Cambio: hacer la relación opcional

            // ✅ Índice único (pero permitiendo null)
            modelBuilder.Entity<Person>()
                .HasIndex(p => p.UserId)
                .IsUnique()
                .HasFilter("[UserId] IS NOT NULL");  // ✅ Solo aplicar unicidad cuando UserId no es null
        }
    }
}