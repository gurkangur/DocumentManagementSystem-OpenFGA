using DocumentManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace DocumentManagement.Data
{
    public class DocumentDbContext : DbContext
    {
        public DocumentDbContext(DbContextOptions<DocumentDbContext> options)
            : base(options)
        {
        }

        public DbSet<Document> Documents { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Document>()
                .HasKey(d => d.Id);

            modelBuilder.Entity<User>()
                .HasKey(u => u.Id);
        }
    }
}
