using Microsoft.EntityFrameworkCore;
using DiaryApp.Mobile.Models;

namespace DiaryApp.Mobile.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<DiaryEntry> DiaryEntries { get; set; }
    public DbSet<Person> Peoples { get; set; }
    public DbSet<Payment> Payments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurar relación entre Payment y Person
        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Person)
            .WithMany(pe => pe.Payments)
            .HasForeignKey(p => p.PeoplesId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índice único compuesto
        modelBuilder.Entity<Payment>()
            .HasIndex(p => new { p.PeoplesId, p.Ano, p.Mes })
            .IsUnique()
            .HasDatabaseName("IX_Payments_PeoplesId_Ano_Mes");

        // Datos de prueba
        modelBuilder.Entity<DiaryEntry>().HasData(
            new DiaryEntry
            {
                Id = 1,
                Title = "Learning .Net MAUI",
                Content = "Learning .NET MAUI mobile development",
                DateCreated = new DateTime(2025, 1, 1)
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
    }
}