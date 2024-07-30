using Exam.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Exam.Data
{
    public class MyDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
    {
        public DbSet<User> Users { get; set; }
        public MyDbContext(DbContextOptions options) : base(options) {}

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder
                .Entity<User>();
        }
    }
}