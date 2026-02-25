using Microsoft.EntityFrameworkCore;
using DiaryApp.Shared.Models;  // ✅ IMPORTANTE

namespace DiaryApp.Api.Data;

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