using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistance.Configuration;

namespace Persistance
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }

        public DbSet<Match> Matches { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<MyLegue> Legues { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration<Match>(new MatchConiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}