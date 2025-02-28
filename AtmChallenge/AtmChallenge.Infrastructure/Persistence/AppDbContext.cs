using Microsoft.EntityFrameworkCore;
using AtmChallenge.Domain.Entities;

namespace AtmChallenge.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
    }
}