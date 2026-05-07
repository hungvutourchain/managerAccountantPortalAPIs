
using B2BAdmin.ApiDocument.Domains.Models;
using Microsoft.EntityFrameworkCore;

namespace B2BAdmin.ApiDocument.Infrastructure
{

    public partial class sqlDbContext : DbContext
    {
        public DbSet<UserAdmin> UserAdmins { get; set; } = null;

        public sqlDbContext()
        {
        }

        public sqlDbContext(DbContextOptions<sqlDbContext> options)
            : base(options)
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
