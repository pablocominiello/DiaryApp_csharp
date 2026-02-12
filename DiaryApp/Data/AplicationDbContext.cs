using Microsoft.EntityFrameworkCore;

namespace DiaryApp.Data
{
    public class AplicationDbContext : DbContext
    {
        public AplicationDbContext(DbContextOptions<AplicationDbContext> options) : base(options)
        {
        }
        public DbSet<DiaryApp.Models.DiaryEntry> DiaryEntries { get; set; }
        public DbSet<DiaryApp.Models.Person> Peoples { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<Models.DiaryEntry>().HasData
                (
                    new Models.DiaryEntry
                    {
                        Id = 1,
                        Title = "Learning .Net MVC",
                        Content = "learning .net mvc with Punjha",
                        DateCreated = new DateTime(2025, 1, 1)
                    },
                    new Models.DiaryEntry
                    {
                        Id = 2,
                        Title = "Learning Migrations",
                        Content = "Learning Migrations mvc with Punjha",
                        DateCreated = new DateTime(2025, 1, 2)
                    },
                    new Models.DiaryEntry
                    {
                        Id = 3,
                        Title = "Input database",
                        Content = "Learning Input database with Punjha",
                        DateCreated = new DateTime(2025, 1, 3)
                    }
                );

            modelBuilder.Entity<Models.Person>().HasData
                (
                    new Models.Person
                    {
                        Id = 1,
                        Nombre = "Pablo Eugenio Cominiello",
                        Content = "Kili",
                        Born = new DateTime(1976, 6, 30)
                    }
                );
        }
    }
}
