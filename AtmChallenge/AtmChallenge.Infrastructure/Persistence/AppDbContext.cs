using Microsoft.EntityFrameworkCore;
using AtmChallenge.Domain.Entities;
using AtmChallenge.Domain.Entities.Card;

namespace AtmChallenge.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        
        public DbSet<Card> Cards { get; set; }
    }
}